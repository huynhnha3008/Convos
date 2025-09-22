using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessObject.Models;
using BusinessObject.Dtos;
using BusinessObject.RequestObjects;
using Repository.UnitOfWork;
using Service.AesEncryptionService;
using Service.NotificationService;
using Microsoft.AspNetCore.SignalR;
using Service.HubService.WhiteboardHub;

namespace Service.WhiteboardService
{
    public class WhiteboardService : IWhiteboardService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAesEncryptionService _aesEncryptionService;
        private readonly INotificationService _notificationService;
        private readonly IHubContext<WhiteboardHub, IWhiteboardHubClient> _whiteboardHub;

        public WhiteboardService(
            IUnitOfWork unitOfWork,
            IAesEncryptionService aesEncryptionService,
            INotificationService notificationService,
            IHubContext<WhiteboardHub, IWhiteboardHubClient> whiteboardHub)
        {
            _unitOfWork = unitOfWork;
            _aesEncryptionService = aesEncryptionService;
            _notificationService = notificationService;
            _whiteboardHub = whiteboardHub;
        }

        public async Task<WhiteboardDto> CreateWhiteboardAsync(string userId, CreateWhiteboardRequest request)
        {
            var encryptedContent = _aesEncryptionService.Encrypt(request.Content.ToString());
            var encryptedExcalidrawData = _aesEncryptionService.Encrypt(request.ExcalidrawData.ToString());

            var whiteboard = new Whiteboard
            {
                Title = request.Title,
                Content = encryptedContent,
                ExcalidrawData = encryptedExcalidrawData,
                ChannelId = request.ChannelId,
                CreatedBy = userId,
                LastEditedBy = userId,
                Timestamp = DateTime.UtcNow,
                Collaborators = new List<string> { userId }
            };

            var addedWhiteboard = await _unitOfWork.Whiteboards.AddAsync(whiteboard);
            await _unitOfWork.SaveAsync();

            var whiteboardDto = MapToDto(addedWhiteboard);
            //await _notificationService.NotifyWhiteboardCreated(request.ChannelId, whiteboardDto);
            return whiteboardDto;
        }

        public async Task<WhiteboardDto> GetWhiteboardByIdAsync(string whiteboardId)
        {
            var whiteboard = await _unitOfWork.Whiteboards.GetByIdAsync(whiteboardId);
            if (whiteboard == null || whiteboard.IsDeleted)
                return null;

            return MapToDto(whiteboard);
        }

        public async Task<List<WhiteboardDto>> GetChannelWhiteboardsAsync(string channelId, int skip = 0, int limit = 50)
        {
            var allWhiteboards = await _unitOfWork.Whiteboards.GetAllAsync();
            var whiteboards = allWhiteboards
                .Where(w => w.ChannelId == channelId && !w.IsDeleted)
                .OrderByDescending(w => w.Timestamp)
                .Skip(skip)
                .Take(limit)
                .ToList();

            return whiteboards.Select(MapToDto).ToList();
        }

        public async Task<WhiteboardDto> UpdateWhiteboardAsync(string userId, string whiteboardId, UpdateWhiteboardRequest request)
        {
            var whiteboard = await _unitOfWork.Whiteboards.GetByIdAsync(whiteboardId);
            if (whiteboard == null || whiteboard.IsDeleted || 
                (whiteboard.CreatedBy != userId && !whiteboard.Collaborators.Contains(userId)))
                return null;

            if (!string.IsNullOrEmpty(request.Title))
                whiteboard.Title = request.Title;

            if (request.Content != null)
                whiteboard.Content = _aesEncryptionService.Encrypt(request.Content.ToString());

            if (request.ExcalidrawData != null)
                whiteboard.ExcalidrawData = _aesEncryptionService.Encrypt(request.ExcalidrawData.ToString());

            whiteboard.LastEditedBy = userId;
            whiteboard.EditedTimestamp = DateTime.UtcNow;
            whiteboard.IsEdited = true;
            whiteboard.Version++;

            var updatedWhiteboard = await _unitOfWork.Whiteboards.UpdateAsync(whiteboardId, whiteboard);
            await _unitOfWork.SaveAsync();

            var whiteboardDto = MapToDto(updatedWhiteboard);
            await _whiteboardHub.Clients.Group(whiteboardId).WhiteboardUpdated(whiteboardId, userId, whiteboardDto);
            return whiteboardDto;
        }

        public async Task<bool> DeleteWhiteboardAsync(string userId, string whiteboardId)
        {
            var whiteboard = await _unitOfWork.Whiteboards.GetByIdAsync(whiteboardId);
            if (whiteboard == null || whiteboard.IsDeleted || whiteboard.CreatedBy != userId)
                return false;

            whiteboard.IsDeleted = true;
            await _unitOfWork.Whiteboards.UpdateAsync(whiteboardId, whiteboard);
            await _unitOfWork.SaveAsync();
            //await _notificationService.NotifyWhiteboardDeleted(whiteboard.ChannelId, whiteboardId);
            return true;
        }

        public async Task<bool> AddCollaboratorAsync(string userId, string whiteboardId, string collaboratorId)
        {
            var whiteboard = await _unitOfWork.Whiteboards.GetByIdAsync(whiteboardId);
            if (whiteboard == null || whiteboard.IsDeleted || whiteboard.CreatedBy != userId)
                return false;

            if (!whiteboard.Collaborators.Contains(collaboratorId))
            {
                whiteboard.Collaborators.Add(collaboratorId);
                await _unitOfWork.Whiteboards.UpdateAsync(whiteboardId, whiteboard);
                await _unitOfWork.SaveAsync();
                await _whiteboardHub.Clients.Group(whiteboardId).CollaboratorAdded(whiteboardId, collaboratorId);
            }

            return true;
        }

        public async Task<bool> RemoveCollaboratorAsync(string userId, string whiteboardId, string collaboratorId)
        {
            var whiteboard = await _unitOfWork.Whiteboards.GetByIdAsync(whiteboardId);
            if (whiteboard == null || whiteboard.IsDeleted || whiteboard.CreatedBy != userId)
                return false;

            if (whiteboard.Collaborators.Contains(collaboratorId))
            {
                whiteboard.Collaborators.Remove(collaboratorId);
                await _unitOfWork.Whiteboards.UpdateAsync(whiteboardId, whiteboard);
                await _unitOfWork.SaveAsync();
                await _whiteboardHub.Clients.Group(whiteboardId).CollaboratorRemoved(whiteboardId, collaboratorId);
            }

            return true;
        }

        public async Task<bool> PinWhiteboardAsync(string userId, string whiteboardId)
        {
            var whiteboard = await _unitOfWork.Whiteboards.GetByIdAsync(whiteboardId);
            if (whiteboard == null || whiteboard.IsDeleted)
                return false;

            whiteboard.Pinned = true;
            await _unitOfWork.Whiteboards.UpdateAsync(whiteboardId, whiteboard);
            await _unitOfWork.SaveAsync();
           // await _notificationService.NotifyWhiteboardPinned(whiteboard.ChannelId, whiteboardId);
            return true;
        }

        public async Task<bool> UnpinWhiteboardAsync(string userId, string whiteboardId)
        {
            var whiteboard = await _unitOfWork.Whiteboards.GetByIdAsync(whiteboardId);
            if (whiteboard == null || whiteboard.IsDeleted)
                return false;

            whiteboard.Pinned = false;
            await _unitOfWork.Whiteboards.UpdateAsync(whiteboardId, whiteboard);
            await _unitOfWork.SaveAsync();
           // await _notificationService.NotifyWhiteboardUnpinned(whiteboard.ChannelId, whiteboardId);
            return true;
        }

        public async Task<bool> MarkAsReadAsync(string userId, string whiteboardId)
        {
            var whiteboard = await _unitOfWork.Whiteboards.GetByIdAsync(whiteboardId);
            if (whiteboard == null || whiteboard.IsDeleted || whiteboard.CreatedBy == userId)
                return false;

            if (!whiteboard.ReadBy.Contains(userId))
            {
                whiteboard.ReadBy.Add(userId);
                await _unitOfWork.Whiteboards.UpdateAsync(whiteboardId, whiteboard);
                await _unitOfWork.SaveAsync();
              //  await _notificationService.NotifyWhiteboardRead(whiteboard.ChannelId, whiteboardId, userId);
            }

            return true;
        }

        public async Task<WhiteboardDto> AddTagAsync(string userId, string whiteboardId, string tag)
        {
            var whiteboard = await _unitOfWork.Whiteboards.GetByIdAsync(whiteboardId);
            if (whiteboard == null || whiteboard.IsDeleted || 
                (whiteboard.CreatedBy != userId && !whiteboard.Collaborators.Contains(userId)))
                return null;

            if (!whiteboard.Tags.Contains(tag))
            {
                whiteboard.Tags.Add(tag);
                await _unitOfWork.Whiteboards.UpdateAsync(whiteboardId, whiteboard);
                await _unitOfWork.SaveAsync();
            }

            return MapToDto(whiteboard);
        }

        public async Task<WhiteboardDto> RemoveTagAsync(string userId, string whiteboardId, string tag)
        {
            var whiteboard = await _unitOfWork.Whiteboards.GetByIdAsync(whiteboardId);
            if (whiteboard == null || whiteboard.IsDeleted || 
                (whiteboard.CreatedBy != userId && !whiteboard.Collaborators.Contains(userId)))
                return null;

            if (whiteboard.Tags.Contains(tag))    
            {
                whiteboard.Tags.Remove(tag);
                await _unitOfWork.Whiteboards.UpdateAsync(whiteboardId, whiteboard);
                await _unitOfWork.SaveAsync();
            }

            return MapToDto(whiteboard);
        }

        private WhiteboardDto MapToDto(Whiteboard whiteboard)
        {
            var decryptedContent = _aesEncryptionService.Decrypt(whiteboard.Content);
            var decryptedExcalidrawData = _aesEncryptionService.Decrypt(whiteboard.ExcalidrawData);

            return new WhiteboardDto
            {
                Id = whiteboard.Id,
                Title = whiteboard.Title,
                Content = decryptedContent,
                ExcalidrawData = decryptedExcalidrawData,
                Timestamp = whiteboard.Timestamp,
                EditedTimestamp = whiteboard.EditedTimestamp,
                ChannelId = whiteboard.ChannelId,
                CreatedBy = whiteboard.CreatedBy,
                LastEditedBy = whiteboard.LastEditedBy,
                IsEdited = whiteboard.IsEdited,
                Pinned = whiteboard.Pinned,
                Version = whiteboard.Version,
                Collaborators = whiteboard.Collaborators,
                ReadBy = whiteboard.ReadBy,
                Tags = whiteboard.Tags,
                Attachments = whiteboard.Attachments?.Select(a => new AttachmentDto
                {
                    FileName = a.FileName,
                    FileType = a.FileType,
                    FileSize = a.FileSize,
                    FileUrl = a.FileUrl
                }).ToList()
            };
        }
    }
}