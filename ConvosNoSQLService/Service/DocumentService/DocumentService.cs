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
using Service.HubService.DocumentHub;

namespace Service.DocumentService
{
    public class DocumentService : IDocumentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAesEncryptionService _aesEncryptionService;
        private readonly INotificationService _notificationService;
        private readonly IHubContext<DocumentHub, IDocumentHubClient> _documentHub;

        public DocumentService(
            IUnitOfWork unitOfWork,
            IAesEncryptionService aesEncryptionService,
            INotificationService notificationService,
            IHubContext<DocumentHub, IDocumentHubClient> documentHub)
        {
            _unitOfWork = unitOfWork;
            _aesEncryptionService = aesEncryptionService;
            _notificationService = notificationService;
            _documentHub = documentHub;
        }

        public async Task<DocumentDto> CreateDocumentAsync(string userId, CreateDocumentRequest request)
        {
            var encryptedContent = _aesEncryptionService.Encrypt(request.Content.ToString());
            var encryptedEditorJsData = _aesEncryptionService.Encrypt(request.EditorJsData.ToString());

            var document = new Document
            {
                Title = request.Title,
                Content = encryptedContent,
                EditorJsData = encryptedEditorJsData,
                ChannelId = request.ChannelId,
                CreatedBy = userId,
                LastEditedBy = userId,
                Timestamp = DateTime.UtcNow,
                Collaborators = new List<string> { userId }
            };

            var addedDocument = await _unitOfWork.Documents.AddAsync(document);
            await _unitOfWork.SaveAsync();

            var documentDto = MapToDto(addedDocument);
            //await _notificationService.NotifyDocumentCreated(request.ChannelId, documentDto);
            return documentDto;
        }

        public async Task<DocumentDto> GetDocumentByIdAsync(string documentId)
        {
            var document = await _unitOfWork.Documents.GetByIdAsync(documentId);
            if (document == null || document.IsDeleted)
                return null;

            return MapToDto(document);
        }

        public async Task<List<DocumentDto>> GetChannelDocumentsAsync(string channelId, int skip = 0, int limit = 50)
        {
            var allDocuments = await _unitOfWork.Documents.GetAllAsync();
            var documents = allDocuments
                .Where(d => d.ChannelId == channelId && !d.IsDeleted)
                .OrderByDescending(d => d.Timestamp)
                .Skip(skip)
                .Take(limit)
                .ToList();

            return documents.Select(MapToDto).ToList();
        }

        public async Task<DocumentDto> UpdateDocumentAsync(string userId, string documentId, UpdateDocumentRequest request)
        {
            var document = await _unitOfWork.Documents.GetByIdAsync(documentId);
            if (document == null || document.IsDeleted || 
                (document.CreatedBy != userId && !document.Collaborators.Contains(userId)))
                return null;

            if (!string.IsNullOrEmpty(request.Title))
                document.Title = request.Title;

            if (request.Content != null)
                document.Content = _aesEncryptionService.Encrypt(request.Content.ToString());

            if (request.EditorJsData != null)
                document.EditorJsData = _aesEncryptionService.Encrypt(request.EditorJsData.ToString());

            document.LastEditedBy = userId;
            document.EditedTimestamp = DateTime.UtcNow;
            document.IsEdited = true;
            document.Version++;

            var updatedDocument = await _unitOfWork.Documents.UpdateAsync(documentId, document);
            await _unitOfWork.SaveAsync();

            var documentDto = MapToDto(updatedDocument);
            //await _notificationService.NotifyDocumentUpdated(document.ChannelId, documentDto);
            return documentDto;
        }

        public async Task<bool> DeleteDocumentAsync(string userId, string documentId)
        {
            var document = await _unitOfWork.Documents.GetByIdAsync(documentId);
            if (document == null || document.IsDeleted || document.CreatedBy != userId)
                return false;

            document.IsDeleted = true;
            await _unitOfWork.Documents.UpdateAsync(documentId, document);
            await _unitOfWork.SaveAsync();
            //await _notificationService.NotifyDocumentDeleted(document.ChannelId, documentId);
            return true;
        }

        public async Task<bool> AddCollaboratorAsync(string userId, string documentId, string collaboratorId)
        {
            var document = await _unitOfWork.Documents.GetByIdAsync(documentId);
            if (document == null || document.IsDeleted || document.CreatedBy != userId)
                return false;

            if (!document.Collaborators.Contains(collaboratorId))
            {
                document.Collaborators.Add(collaboratorId);
                await _unitOfWork.Documents.UpdateAsync(documentId, document);
                await _unitOfWork.SaveAsync();
                await _documentHub.Clients.Group(documentId).CollaboratorAdded(documentId, collaboratorId);
            }

            return true;
        }

        public async Task<bool> RemoveCollaboratorAsync(string userId, string documentId, string collaboratorId)
        {
            var document = await _unitOfWork.Documents.GetByIdAsync(documentId);
            if (document == null || document.IsDeleted || document.CreatedBy != userId)
                return false;

            if (document.Collaborators.Contains(collaboratorId))
            {
                document.Collaborators.Remove(collaboratorId);
                await _unitOfWork.Documents.UpdateAsync(documentId, document);
                await _unitOfWork.SaveAsync();
                await _documentHub.Clients.Group(documentId).CollaboratorRemoved(documentId, collaboratorId);
            }

            return true;
        }

        public async Task<bool> PinDocumentAsync(string userId, string documentId)
        {
            var document = await _unitOfWork.Documents.GetByIdAsync(documentId);
            if (document == null || document.IsDeleted)
                return false;

            document.Pinned = true;
            await _unitOfWork.Documents.UpdateAsync(documentId, document);
            await _unitOfWork.SaveAsync();
           // await _notificationService.NotifyDocumentPinned(document.ChannelId, documentId);
            return true;
        }

        public async Task<bool> UnpinDocumentAsync(string userId, string documentId)
        {
            var document = await _unitOfWork.Documents.GetByIdAsync(documentId);
            if (document == null || document.IsDeleted)
                return false;

            document.Pinned = false;
            await _unitOfWork.Documents.UpdateAsync(documentId, document);
            await _unitOfWork.SaveAsync();
           // await _notificationService.NotifyDocumentUnpinned(document.ChannelId, documentId);
            return true;
        }

        public async Task<bool> MarkAsReadAsync(string userId, string documentId)
        {
            var document = await _unitOfWork.Documents.GetByIdAsync(documentId);
            if (document == null || document.IsDeleted || document.CreatedBy == userId)
                return false;

            if (!document.ReadBy.Contains(userId))
            {
                document.ReadBy.Add(userId);
                await _unitOfWork.Documents.UpdateAsync(documentId, document);
                await _unitOfWork.SaveAsync();
              //  await _notificationService.NotifyDocumentRead(document.ChannelId, documentId, userId);
            }

            return true;
        }

        public async Task<DocumentDto> AddTagAsync(string userId, string documentId, string tag)
        {
            var document = await _unitOfWork.Documents.GetByIdAsync(documentId);
            if (document == null || document.IsDeleted || 
                (document.CreatedBy != userId && !document.Collaborators.Contains(userId)))
                return null;

            if (!document.Tags.Contains(tag))
            {
                document.Tags.Add(tag);
                await _unitOfWork.Documents.UpdateAsync(documentId, document);
                await _unitOfWork.SaveAsync();
            }

            return MapToDto(document);
        }

        public async Task<DocumentDto> RemoveTagAsync(string userId, string documentId, string tag)
        {
            var document = await _unitOfWork.Documents.GetByIdAsync(documentId);
            if (document == null || document.IsDeleted || 
                (document.CreatedBy != userId && !document.Collaborators.Contains(userId)))
                return null;

            if (document.Tags.Contains(tag))
            {
                document.Tags.Remove(tag);
                await _unitOfWork.Documents.UpdateAsync(documentId, document);
                await _unitOfWork.SaveAsync();
            }

            return MapToDto(document);
        }

        private DocumentDto MapToDto(Document document)
        {
            var decryptedContent = _aesEncryptionService.Decrypt(document.Content);
            var decryptedEditorJsData = _aesEncryptionService.Decrypt(document.EditorJsData);

            return new DocumentDto
            {
                Id = document.Id,
                Title = document.Title,
                Content = decryptedContent,
                EditorJsData = decryptedEditorJsData,
                Timestamp = document.Timestamp,
                EditedTimestamp = document.EditedTimestamp,
                ChannelId = document.ChannelId,
                CreatedBy = document.CreatedBy,
                LastEditedBy = document.LastEditedBy,
                IsEdited = document.IsEdited,
                Pinned = document.Pinned,
                Version = document.Version,
                Collaborators = document.Collaborators,
                ReadBy = document.ReadBy,
                Tags = document.Tags,
                Attachments = document.Attachments?.Select(a => new AttachmentDto
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