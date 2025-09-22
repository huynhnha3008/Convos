using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessObject.Dtos;
using BusinessObject.RequestObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Service.DocumentService;

namespace ConvosNoSQLWebApi.Controllers
{
    [ApiController]
    [Route("api/documents")]
    public class DocumentController : ControllerBase
    {
        private readonly ILogger<DocumentController> _logger;
        private readonly IDocumentService _documentService;

        public DocumentController(
            ILogger<DocumentController> logger,
            IDocumentService documentService)
        {
            _logger = logger;
            _documentService = documentService;
        }

        [HttpPost]
        public async Task<ActionResult<DocumentDto>> CreateDocument([FromBody] CreateDocumentRequest request)
        {
            try
            {

                var document = await _documentService.CreateDocumentAsync(request.UserId, request);
                return Ok(document);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating document");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{documentId}")]
        public async Task<ActionResult<DocumentDto>> GetDocument(string documentId)
        {
            try
            {
                var document = await _documentService.GetDocumentByIdAsync(documentId);
                if (document == null)
                    return NotFound();

                return Ok(document);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving document");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("channel/{channelId}")]
        public async Task<ActionResult<List<DocumentDto>>> GetChannelDocuments(
            string channelId,
            [FromQuery] int skip = 0,
            [FromQuery] int limit = 50)
        {
            try
            {
                var documents = await _documentService.GetChannelDocumentsAsync(channelId, skip, limit);
                return Ok(documents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving channel documents");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{documentId}")]
        public async Task<ActionResult<DocumentDto>> UpdateDocument(
            string documentId,
            [FromBody] UpdateDocumentRequest request)
        {
            try
            {


                var document = await _documentService.UpdateDocumentAsync(request.UserId, documentId, request);
                if (document == null)
                    return NotFound();

                return Ok(document);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating document");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{documentId}")]
        public async Task<ActionResult> DeleteDocument(string documentId, string userId)
        {
            try
            {

                var result = await _documentService.DeleteDocumentAsync(userId, documentId);
                if (!result)
                    return NotFound();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting document");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("{documentId}/collaborators/{collaboratorId}")]
        public async Task<ActionResult> AddCollaborator(string documentId, string collaboratorId, string userId)
        {
            try
            {

                var result = await _documentService.AddCollaboratorAsync(userId, documentId, collaboratorId);
                if (!result)
                    return NotFound();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding collaborator");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{documentId}/collaborators/{collaboratorId}")]
        public async Task<ActionResult> RemoveCollaborator(string documentId, string collaboratorId, string userId)
        {
            try
            {

                var result = await _documentService.RemoveCollaboratorAsync(userId, documentId, collaboratorId);
                if (!result)
                    return NotFound();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing collaborator");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("{documentId}/pin")]
        public async Task<ActionResult> PinDocument(string documentId, string userId)
        {
            try
            {

                var result = await _documentService.PinDocumentAsync(userId, documentId);
                if (!result)
                    return NotFound();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pinning document");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{documentId}/pin")]
        public async Task<ActionResult> UnpinDocument(string documentId, string userId)
        {
            try
            {

                var result = await _documentService.UnpinDocumentAsync(userId, documentId);
                if (!result)
                    return NotFound();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unpinning document");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("{documentId}/read")]
        public async Task<ActionResult> MarkAsRead(string documentId, string userId)
        {
            try
            {

                var result = await _documentService.MarkAsReadAsync(userId, documentId);
                if (!result)
                    return NotFound();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking document as read");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("{documentId}/tags/{tag}")]
        public async Task<ActionResult<DocumentDto>> AddTag(string documentId, string tag, string userId)
        {
            try
            {

                var document = await _documentService.AddTagAsync(userId, documentId, tag);
                if (document == null)
                    return NotFound();

                return Ok(document);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding tag");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{documentId}/tags/{tag}")]
        public async Task<ActionResult<DocumentDto>> RemoveTag(string documentId, string tag, string userId)
        {
            try
            {

                var document = await _documentService.RemoveTagAsync(userId, documentId, tag);
                if (document == null)
                    return NotFound();

                return Ok(document);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing tag");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}