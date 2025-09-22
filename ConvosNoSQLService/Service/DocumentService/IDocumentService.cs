using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessObject.Models;
using BusinessObject.Dtos;
using BusinessObject.RequestObjects;

namespace Service.DocumentService
{
    public interface IDocumentService
    {
        Task<DocumentDto> CreateDocumentAsync(string userId, CreateDocumentRequest request);
        Task<DocumentDto> GetDocumentByIdAsync(string documentId);
        Task<List<DocumentDto>> GetChannelDocumentsAsync(string channelId, int skip = 0, int limit = 50);
        Task<DocumentDto> UpdateDocumentAsync(string userId, string documentId, UpdateDocumentRequest request);
        Task<bool> DeleteDocumentAsync(string userId, string documentId);
        Task<bool> AddCollaboratorAsync(string userId, string documentId, string collaboratorId);
        Task<bool> RemoveCollaboratorAsync(string userId, string documentId, string collaboratorId);
        Task<bool> PinDocumentAsync(string userId, string documentId);
        Task<bool> UnpinDocumentAsync(string userId, string documentId);
        Task<bool> MarkAsReadAsync(string userId, string documentId);
        Task<DocumentDto> AddTagAsync(string userId, string documentId, string tag);
        Task<DocumentDto> RemoveTagAsync(string userId, string documentId, string tag);
    }
}