using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using BusinessObject.Dtos;
using BusinessObject.Models;
using BusinessObject.RequestObjects;
using Microsoft.AspNetCore.Http;
using Repository.UnitOfWork;
using Service.AesEncryptionService;
using Service.NotificationService;

namespace Service.ChannelMessageService
{
    public class ChannelMessageService : IChannelMessageService
    {
        private readonly string _bucketName = "sync-music-storage";
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAesEncryptionService _aesEncryptionService;
        private readonly INotificationService _notificationService;
        private readonly IAmazonS3 _s3Client;

        public ChannelMessageService(
            IUnitOfWork unitOfWork,
            IAesEncryptionService aesEncryptionService,
            INotificationService notificationService,
            IAmazonS3 s3Client
            )
        {
            _unitOfWork = unitOfWork;
            _aesEncryptionService = aesEncryptionService;
            _notificationService = notificationService;
            _s3Client = s3Client;
        }

        public async Task<MessageDto> SendMessageAsync(string senderId, SendChannelMessageRequest request)
        {
            var encryptedContent = _aesEncryptionService.Encrypt(request.Content);

            var attachmentDtos = new List<Attachment>();

            if (request.Attachments != null)
            {
                foreach (var attachment in request.Attachments)
                {
                    var fileUrl = await UploadFileAsync(attachment, attachment.ContentType);
                    attachmentDtos.Add(new Attachment
                    {
                        FileName = attachment.FileName,
                        FileType = attachment.ContentType,
                        FileSize = attachment.Length,
                        FileUrl = fileUrl
                    });
                }
            }

            var message = new Message
            {
                Content = encryptedContent,
                MemberId = senderId,
                ChannelId = request.ChannelId,
                Timestamp = DateTime.UtcNow,
                Attachments = attachmentDtos,
                ReplyToMessageId = request.ReplyToMessageId
            };

            var addedMessage = await _unitOfWork.Messages.AddAsync(message);
            await _unitOfWork.SaveAsync();

            RepliedMessageDto? repliedMessageDto = null;
            if (!string.IsNullOrEmpty(request.ReplyToMessageId))
            {
                var repliedMessage = await _unitOfWork.Messages.GetByIdAsync(request.ReplyToMessageId);
                if (repliedMessage != null)
                {
                    repliedMessageDto = new RepliedMessageDto
                    {
                        Id = repliedMessage.Id,
                        Content = _aesEncryptionService.Decrypt(repliedMessage.Content),
                        Timestamp = repliedMessage.Timestamp,
                        SenderId = repliedMessage.MemberId,
                    };
                }
            }
            var messageSendrealtime = MapToDtoEncryption(addedMessage);
            var messageDto = MapToDto(addedMessage);
            messageDto.RepliedMessage = repliedMessageDto;
            messageSendrealtime.RepliedMessage = repliedMessageDto;

            // Send real-time message to channel group
            await _notificationService.NotifyMessageReceived(request.ChannelId, messageSendrealtime);

            // Send notification to channel members
            await _notificationService.NotifyChannelMessageReceived(request.ChannelId, messageSendrealtime);

            return messageDto;
        }
        private async Task<string> UploadFileAsync(IFormFile file, string fileType)
        {
            var fileTransferUtility = new TransferUtility(_s3Client);
            var fileExtension = Path.GetExtension(file.FileName);
            var filePath = $"{fileType}/{Guid.NewGuid()}{fileExtension}";

            using (var stream = file.OpenReadStream())
            {
                await fileTransferUtility.UploadAsync(stream, _bucketName, filePath);
            }

            var aclResponse = await _s3Client.PutACLAsync(new PutACLRequest
            {
                BucketName = _bucketName,
                Key = filePath,
                CannedACL = S3CannedACL.PublicRead
            });

            var url = $"https://{_bucketName}.s3.amazonaws.com/{filePath}";

            return url;
        }
        public async Task<MessageDto> EditMessageAsync(string userId, EditChannelMessageRequest request)
        {
            var encryptedContent = _aesEncryptionService.Encrypt(request.NewContent);
            var message = await _unitOfWork.Messages.GetByIdAsync(request.MessageId);
            if (message == null || message.MemberId != userId)
                return null;

            message.Content = encryptedContent;
            message.EditedTimestamp = DateTime.UtcNow;
            message.IsEdited = true;
            var updatedMessage = await _unitOfWork.Messages.UpdateAsync(message.Id, message);
            await _unitOfWork.SaveAsync();

            RepliedMessageDto? repliedMessageDto = null;
            if (!string.IsNullOrEmpty(updatedMessage.ReplyToMessageId))
            {
                var repliedMessage = await _unitOfWork.Messages.GetByIdAsync(updatedMessage.ReplyToMessageId);
                if (repliedMessage != null)
                {
                    repliedMessageDto = new RepliedMessageDto
                    {
                        Id = repliedMessage.Id,
                        Content = _aesEncryptionService.Decrypt(repliedMessage.Content),
                        Timestamp = repliedMessage.Timestamp,
                        SenderId = repliedMessage.MemberId,
                    };
                }
            }

            var messageDto = MapToDto(updatedMessage);
            var messageSendrealtime = MapToDtoEncryption(updatedMessage);
            messageDto.RepliedMessage = repliedMessageDto;
            messageSendrealtime.RepliedMessage = repliedMessageDto;
            await _notificationService.NotifyMessageEdited(message.ChannelId, messageSendrealtime);
            return messageDto;
        }

        public async Task<bool> DeleteMessageAsync(string userId, string messageId)
        {
            var message = await _unitOfWork.Messages.GetByIdAsync(messageId);
            if (message == null || message.MemberId != userId)
                return false;

            message.IsDeleted = true;
            await _unitOfWork.Messages.UpdateAsync(message.Id, message);
            await _unitOfWork.SaveAsync();
            await _notificationService.NotifyMessageDeleted(message.ChannelId, messageId);
            return true;
        }

        public async Task<List<MessageDto>> GetChannelMessagesAsync(string memberId, string channelId, int skip = 0, int limit = 50)
        {
            var allMessages = await _unitOfWork.Messages.GetAllAsync();
            var conversation = allMessages.Where(m => m.ChannelId == channelId && !m.IsDeleted)
                .OrderByDescending(m => m.Timestamp)
                .Skip(skip)
                .Take(limit)
                .ToList();
            var allMessagesReturn = new List<MessageDto>();

            foreach (var message in conversation)
            {
                var dto = MapToDtoEncryption(message);

                if (!string.IsNullOrEmpty(message.ReplyToMessageId))
                {
                    var repliedMessage = await _unitOfWork.Messages.GetByIdAsync(message.ReplyToMessageId);
                    if (repliedMessage != null)
                    {
                        dto.RepliedMessage = new RepliedMessageDto
                        {
                            Id = repliedMessage.Id,
                            Content = _aesEncryptionService.Decrypt(repliedMessage.Content),
                            SenderId = repliedMessage.MemberId,
                            Timestamp = repliedMessage.Timestamp
                        };
                    }
                }

                allMessagesReturn.Add(dto);
            }
            return allMessagesReturn;
        }
        public async Task<bool> MarkAsReadAsync(string userId, string messageId)
        {
            var message = await _unitOfWork.Messages.GetByIdAsync(messageId);
            if (message == null || message.MemberId == userId)
                return false;

            if (!message.ReadBy.Contains(userId))
            {
                message.ReadBy.Add(userId);
                await _unitOfWork.Messages.UpdateAsync(message.Id, message);
                await _unitOfWork.SaveAsync();
                await _notificationService.NotifyMessageRead(message.ChannelId, messageId, userId);
            }

            return true;
        }
        public async Task<MessageDto> AddReactionAsync(string userId, string messageId, string emojiIcon)
        {
            var message = await _unitOfWork.Messages.GetByIdAsync(messageId);

            if (message == null)
                return null;

            var existingReaction = message.Reactions.FirstOrDefault(r => r.Emoji == emojiIcon);

            if (existingReaction == null)
            {
                var newReaction = new Reaction
                {
                    MemberId = userId,
                    Emoji = emojiIcon,
                    MessageId = messageId,
                    UserIds = new List<string> { userId }
                };
                message.Reactions.Add(newReaction);
            }
            else
            {
                if (!existingReaction.UserIds.Contains(userId))
                {
                    existingReaction.UserIds.Add(userId);
                }
            }
            await _unitOfWork.Messages.UpdateAsync(messageId, message);
            await _unitOfWork.SaveAsync();
            var messageDto = await GetMessageWithReactionsAsync(messageId);
            await _notificationService.NotifyReactionAdded(message.ChannelId, messageDto);
            return messageDto;
        }

        public async Task<MessageDto> RemoveReactionAsync(string userId, string messageId, string emojiIcon)
        {
            var message = await _unitOfWork.Messages.GetByIdAsync(messageId);
            if (message == null)
                return null;

            var reactionToRemove = message.Reactions
                .FirstOrDefault(r => r.Emoji == emojiIcon && r.UserIds.Contains(userId));

            if (reactionToRemove != null)
            {
                reactionToRemove.UserIds.Remove(userId);

                if (reactionToRemove.UserIds.Count == 0)
                {
                    message.Reactions.Remove(reactionToRemove);
                }
            }
            await _unitOfWork.Messages.UpdateAsync(messageId, message);
            await _unitOfWork.SaveAsync();

            var messageDto = await GetMessageWithReactionsAsync(messageId);
            await _notificationService.NotifyReactionRemoved(message.ChannelId, messageDto);
            return messageDto;
        }

        private string GetConversationId(string userId1, string userId2)
        {
            var sortedIds = new[] { userId1, userId2 }.OrderBy(id => id);
            return string.Join("_", sortedIds);
        }
        private async Task<MessageDto> GetMessageWithReactionsAsync(string messageId)
        {
            var message = await _unitOfWork.Messages.GetByIdAsync(messageId);
            if (message == null)
                return null;
            RepliedMessageDto? repliedMessageDto = null;
            if (!string.IsNullOrEmpty(message.ReplyToMessageId))
            {
                var repliedMessage = await _unitOfWork.Messages.GetByIdAsync(message.ReplyToMessageId);
                if (repliedMessage != null)
                {
                    repliedMessageDto = new RepliedMessageDto
                    {
                        Id = repliedMessage.Id,
                        Content = _aesEncryptionService.Decrypt(repliedMessage.Content),
                        Timestamp = repliedMessage.Timestamp,
                        SenderId = repliedMessage.MemberId,
                    };
                }
            }


            var messageDto = MapToDtoEncryption(message);
            messageDto.RepliedMessage = repliedMessageDto;
            messageDto.Reactions = message.Reactions.Select(r => new ReactionDto
            {
                Emoji = r.Emoji,
                UserIds = r.UserIds
            }).ToList();

            return messageDto;
        }
        private MessageDto MapToDto(Message message)
        {
            return new MessageDto
            {
                Id = message.Id,
                Content = message.Content,
                Timestamp = message.Timestamp,
                MemberId = message.MemberId,
                ChannelId = message.ChannelId,
                IsEdited = message.IsEdited,
                IsPinned = message.IsPinned,
                PinnedAt = message.PinnedAt,
                PinnedBy = message.PinnedBy,
                Attachments = message?.Attachments?.Select(a => new AttachmentDto
                {
                    FileName = a.FileName,
                    FileType = a.FileType,
                    FileSize = a.FileSize,
                    FileUrl = a.FileUrl
                }).ToList(),
                Reactions = message?.Reactions?.Select(r => new ReactionDto
                {
                    Emoji = r.Emoji,
                    UserIds = r.UserIds
                }).ToList()
            };
        }

        private MessageDto MapToDtoEncryption(Message message)
        {
            return new MessageDto
            {
                Id = message.Id,
                Content = _aesEncryptionService.Decrypt(message.Content),
                Timestamp = message.Timestamp,
                MemberId = message.MemberId,
                ChannelId = message.ChannelId,
                IsEdited = message.IsEdited,
                IsPinned = message.IsPinned,
                PinnedAt = message.PinnedAt,
                PinnedBy = message.PinnedBy,
                Attachments = message?.Attachments?.Select(a => new AttachmentDto
                {
                    FileName = a.FileName,
                    FileType = a.FileType,
                    FileSize = a.FileSize,
                    FileUrl = a.FileUrl
                }).ToList(),
                Reactions = message?.Reactions?.Select(r => new ReactionDto
                {
                    Emoji = r.Emoji,
                    UserIds = r.UserIds
                }).ToList()
            };
        }

        public async Task<MessageDto> PinMessageAsync(string userId, string messageId)
        {
            var message = await _unitOfWork.Messages.GetByIdAsync(messageId);
            if (message == null)
                return null;

            message.IsPinned = true;
            message.PinnedAt = DateTime.UtcNow;
            message.PinnedBy = userId;

            var updatedMessage = await _unitOfWork.Messages.UpdateAsync(message.Id, message);
            await _unitOfWork.SaveAsync();

            var messageDto = MapToDtoEncryption(updatedMessage);
            await _notificationService.NotifyMessagePinned(message.ChannelId, messageDto);

            return messageDto;
        }

        public async Task<MessageDto> UnpinMessageAsync(string userId, string messageId)
        {
            var message = await _unitOfWork.Messages.GetByIdAsync(messageId);
            if (message == null)
                return null;

            message.IsPinned = false;
            message.PinnedAt = null;
            message.PinnedBy = string.Empty;

            var updatedMessage = await _unitOfWork.Messages.UpdateAsync(message.Id, message);
            await _unitOfWork.SaveAsync();

            var messageDto = MapToDtoEncryption(updatedMessage);
            await _notificationService.NotifyMessageUnpinned(message.ChannelId, messageDto);

            return messageDto;
        }

        public async Task<List<MessageDto>> GetPinnedMessagesAsync(string channelId)
        {
            var allMessages = await _unitOfWork.Messages.GetAllAsync();
            var pinnedMessages = allMessages
                .Where(m => m.ChannelId == channelId && m.IsPinned && !m.IsDeleted)
                .OrderByDescending(m => m.PinnedAt)
                .ToList();

            return pinnedMessages.Select(m => MapToDtoEncryption(m)).ToList();
        }
    }

}