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
using Microsoft.AspNetCore.SignalR;
using Repository.UnitOfWork;
using Service.AesEncryptionService;
using Service.HubService.PrivateMessageHub;

namespace Service.PrivateMessageService
{
    public class PrivateMessageService : IPrivateMessageService
    {
        private readonly string _bucketName = "sync-music-storage";
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHubContext<PrivateMessageHub> _hubContext;
        private readonly IAesEncryptionService _aesEncryptionService;
        private readonly IAmazonS3 _s3Client;
        public PrivateMessageService(
            IUnitOfWork unitOfWork, 
            IHubContext<PrivateMessageHub> hubContext, 
            IAesEncryptionService aesEncryptionService, 
            IAmazonS3 s3Client
            )
        {
            _unitOfWork = unitOfWork;
            _hubContext = hubContext;
            _aesEncryptionService = aesEncryptionService;
            _s3Client = s3Client;
        }
        public async Task<PrivateMessageDto> SendMessageAsync(string senderId, SendMessageRequest request)
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

            var message = new PrivateMessage
            {
                Content = encryptedContent,
                SenderId = senderId,
                ReceiverId = request.ReceiverId,
                Timestamp = DateTime.UtcNow,
                Attachments = attachmentDtos,
                ReplyToMessageId = request.ReplyToMessageId
            };

            var addedMessage = await _unitOfWork.PrivateMessages.AddAsync(message);
            await _unitOfWork.SaveAsync();

            RepliedMessageDto? repliedMessageDto = null;
            if (!string.IsNullOrEmpty(request.ReplyToMessageId))
            {
                var repliedMessage = await _unitOfWork.PrivateMessages.GetByIdAsync(request.ReplyToMessageId);
                if (repliedMessage != null)
                {
                    repliedMessageDto = new RepliedMessageDto
                    {
                        Id = repliedMessage.Id,
                        Content = _aesEncryptionService.Decrypt(repliedMessage.Content),
                        Timestamp = repliedMessage.Timestamp,
                        SenderId = repliedMessage.SenderId,
                    };
                }
            }

            var messageSendrealtime = MapToDtoEncryption(addedMessage);
            var messageDto = MapToDto(addedMessage);
            messageDto.RepliedMessage = repliedMessageDto;
            messageSendrealtime.RepliedMessage = repliedMessageDto;

            // Send real-time message to conversation group
            await _hubContext.Clients.Group(GetConversationId(senderId, request.ReceiverId))
                        .SendAsync("ReceiveMessage", messageSendrealtime);

            // Send notification to receiver's group
            await _hubContext.Clients.Group(request.ReceiverId)
                        .SendAsync("ReceiveNotification", new { 
                            //type = "message",
                            senderId = senderId, 
                            content = _aesEncryptionService.Decrypt(encryptedContent),
                            receiverId = request.ReceiverId 
                        });

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
        public async Task<PrivateMessageDto> EditMessageAsync(string userId, EditMessageRequest request)
        {
            var message = await _unitOfWork.PrivateMessages.GetByIdAsync(request.MessageId);
            if (message == null || message.SenderId != userId)
                return null;

            message.Content = _aesEncryptionService.Encrypt(request.NewContent);
            message.EditedTimestamp = DateTime.UtcNow;
            message.IsEdited = true;
            var updatedMessage = await _unitOfWork.PrivateMessages.UpdateAsync(message.Id, message);
            await _unitOfWork.SaveAsync();
            RepliedMessageDto? repliedMessageDto = null;
            if (!string.IsNullOrEmpty(updatedMessage.ReplyToMessageId))
            {
                var repliedMessage = await _unitOfWork.PrivateMessages.GetByIdAsync(updatedMessage.ReplyToMessageId);
                if (repliedMessage != null)
                {
                    repliedMessageDto = new RepliedMessageDto
                    {
                        Id = repliedMessage.Id,
                        Content = _aesEncryptionService.Decrypt(repliedMessage.Content),
                        Timestamp = repliedMessage.Timestamp,
                        SenderId = repliedMessage.SenderId,
                    };
                }
            }

            var messageDto = MapToDto(updatedMessage);
            var messageSendrealtime = MapToDtoEncryption(updatedMessage);
            messageDto.RepliedMessage = repliedMessageDto;
            messageSendrealtime.RepliedMessage = repliedMessageDto;
            await _hubContext.Clients.Group(GetConversationId(message.SenderId, message.ReceiverId))
                .SendAsync("MessageUpdated", messageSendrealtime);

            return messageDto;
        }

        public async Task<bool> DeleteMessageAsync(string userId, string messageId)
        {
            var message = await _unitOfWork.PrivateMessages.GetByIdAsync(messageId);
            if (message == null || message.SenderId != userId)
                return false;

            message.IsDeleted = true;
            await _unitOfWork.PrivateMessages.UpdateAsync(message.Id, message);
            await _unitOfWork.SaveAsync();

            await _hubContext.Clients.Group(GetConversationId(message.SenderId, message.ReceiverId))
                    .SendAsync("MessageDeleted", messageId);

            return true;
        }

        public async Task<List<PrivateMessageDto>> GetConversationAsync(string userId1, string userId2, int skip = 0, int limit = 50)
        {
            var allMessages = await _unitOfWork.PrivateMessages.GetAllAsync();

            var conversation = allMessages.Where(m =>
                (m.SenderId == userId1 && m.ReceiverId == userId2 && !m.IsDeleted) ||
                (m.SenderId == userId2 && m.ReceiverId == userId1 && !m.IsDeleted))
                .OrderByDescending(m => m.Timestamp)
                .Skip(skip)
                .Take(limit)
                .ToList();

            var allMessagesReturn = new List<PrivateMessageDto>();

            foreach (var message in conversation)
            {
                var dto = MapToDtoEncryption(message);

                if (!string.IsNullOrEmpty(message.ReplyToMessageId))
                {
                    var repliedMessage = await _unitOfWork.PrivateMessages.GetByIdAsync(message.ReplyToMessageId);
                    if (repliedMessage != null)
                    {
                        dto.RepliedMessage = new RepliedMessageDto
                        {
                            Id = repliedMessage.Id,
                            Content = _aesEncryptionService.Decrypt(repliedMessage.Content),
                            SenderId = repliedMessage.SenderId,
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
            var message = await _unitOfWork.PrivateMessages.GetByIdAsync(messageId);
            if (message == null || message.ReceiverId != userId)
                return false;

            if (!message.ReadBy.Contains(userId))
            {
                message.ReadBy.Add(userId);
                await _unitOfWork.PrivateMessages.UpdateAsync(message.Id, message);
                await _unitOfWork.SaveAsync();
            }
            return true;
        }
        public async Task<PrivateMessageDto> AddReactionAsync(string userId, string messageId, string emoji)
        {
            var message = await _unitOfWork.PrivateMessages.GetByIdAsync(messageId);

            if (message == null)
                return null;

            var existingReaction = message.Reactions.FirstOrDefault(r => r.Emoji == emoji);

            if (existingReaction == null)
            {
                var newReaction = new Reaction
                {
                    MemberId = userId,
                    MessageId = messageId,
                    Emoji = emoji,
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

            await _unitOfWork.PrivateMessages.UpdateAsync(messageId, message);
            await _unitOfWork.SaveAsync();

            var messageDto = await GetMessageWithReactionsAsync(messageId);

            await _hubContext.Clients.Group(GetConversationId(messageDto.SenderId, messageDto.ReceiverId))
                .SendAsync("ReactionAdded", messageDto);

            return messageDto;
        }
        public async Task<PrivateMessageDto> RemoveReactionAsync(string userId, string messageId, string emoji)
        {
            var message = await _unitOfWork.PrivateMessages.GetByIdAsync(messageId);

            if (message == null)
                return null;

            var reactionToRemove = message.Reactions
                .FirstOrDefault(r => r.Emoji == emoji && r.UserIds.Contains(userId));

            if (reactionToRemove != null)
            {
                reactionToRemove.UserIds.Remove(userId);

                if (reactionToRemove.UserIds.Count == 0)
                {
                    message.Reactions.Remove(reactionToRemove);
                }
            }

            await _unitOfWork.PrivateMessages.UpdateAsync(messageId, message);
            await _unitOfWork.SaveAsync();

            var messageDto = await GetMessageWithReactionsAsync(messageId);

            await _hubContext.Clients.Group(GetConversationId(messageDto.SenderId, messageDto.ReceiverId))
                .SendAsync("ReactionRemoved", messageDto);

            return messageDto;
        }

        private string GetConversationId(string userId1, string userId2)
        {
            var sortedIds = new[] { userId1, userId2 }.OrderBy(id => id);
            return string.Join("_", sortedIds);
        }
        private async Task<PrivateMessageDto> GetMessageWithReactionsAsync(string messageId)
        {
            var message = await _unitOfWork.PrivateMessages.GetByIdAsync(messageId);
            if (message == null)
                return null;
            RepliedMessageDto? repliedMessageDto = null;
            if (!string.IsNullOrEmpty(message.ReplyToMessageId))
            {
                var repliedMessage = await _unitOfWork.PrivateMessages.GetByIdAsync(message.ReplyToMessageId);
                if (repliedMessage != null)
                {
                    repliedMessageDto = new RepliedMessageDto
                    {
                        Id = repliedMessage.Id,
                        Content = _aesEncryptionService.Decrypt(repliedMessage.Content),
                        Timestamp = repliedMessage.Timestamp,
                        SenderId = repliedMessage.SenderId,
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

        private PrivateMessageDto MapToDto(PrivateMessage message)
        {
            return new PrivateMessageDto
            {
                Id = message.Id,
                Content = message.Content,
                Timestamp = message.Timestamp,
                SenderId = message.SenderId,
                ReceiverId = message.ReceiverId,
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


        private PrivateMessageDto MapToDtoEncryption(PrivateMessage message)
        {
            return new PrivateMessageDto
            {
                Id = message.Id,
                Content = _aesEncryptionService.Decrypt(message.Content),
                Timestamp = message.Timestamp,
                SenderId = message.SenderId,
                ReceiverId = message.ReceiverId,
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

        public async Task<PrivateMessageDto> PinMessageAsync(string userId, string messageId)
        {
            var message = await _unitOfWork.PrivateMessages.GetByIdAsync(messageId);
            if (message == null)
                return null;

            message.IsPinned = true;
            message.PinnedAt = DateTime.UtcNow;
            message.PinnedBy = userId;

            var updatedMessage = await _unitOfWork.PrivateMessages.UpdateAsync(message.Id, message);
            await _unitOfWork.SaveAsync();

            var messageDto = MapToDtoEncryption(updatedMessage);
            await _hubContext.Clients.Group(GetConversationId(message.SenderId, message.ReceiverId))
                .SendAsync("MessagePinned", messageDto);

            return messageDto;
        }

        public async Task<PrivateMessageDto> UnpinMessageAsync(string userId, string messageId)
        {
            var message = await _unitOfWork.PrivateMessages.GetByIdAsync(messageId);
            if (message == null)
                return null;

            message.IsPinned = false;
            message.PinnedAt = null;
            message.PinnedBy = string.Empty;

            var updatedMessage = await _unitOfWork.PrivateMessages.UpdateAsync(message.Id, message);
            await _unitOfWork.SaveAsync();

            var messageDto = MapToDtoEncryption(updatedMessage);
            await _hubContext.Clients.Group(GetConversationId(message.SenderId, message.ReceiverId))
                .SendAsync("MessageUnpinned", messageDto);

            return messageDto;
        }

        public async Task<List<PrivateMessageDto>> GetPinnedMessagesAsync(string senderId, string otherUserId)
        {
            var allMessages = await _unitOfWork.PrivateMessages.GetAllAsync();
            var pinnedMessages = allMessages
                .Where(m => 
                    ((m.SenderId == senderId && m.ReceiverId == otherUserId) || 
                     (m.SenderId == otherUserId && m.ReceiverId == senderId)) && 
                    m.IsPinned && 
                    !m.IsDeleted)
                .OrderByDescending(m => m.PinnedAt)
                .ToList();

            return pinnedMessages.Select(m => MapToDtoEncryption(m)).ToList();
        }
    }
}