using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessObject.Models;
using BusinessObject.RequestObjects;
using BusinessObject.SupportModels;
using Microsoft.AspNetCore.Mvc;
using Service.PrivateCallSessionService;

namespace ConvosNoSQLWebApi.Controllers
{
    [ApiController]
    [Route("api/private-call-sessions")]
    public class PrivateCallSessionController : ControllerBase
    {
        private readonly IPrivateCallSessionService _callSessionService;
        private readonly ILogger<PrivateCallSessionController> _logger;

        public PrivateCallSessionController(IPrivateCallSessionService callSessionService, ILogger<PrivateCallSessionController> logger)
        {
            _callSessionService = callSessionService;
            _logger = logger;
        }

        [HttpPost("initiate")]
        public async Task<ActionResult<PrivateCallSession>> InitiateCall([FromBody] InitiateCallRequest request)
        {
            try
            {
                var session = await _callSessionService.CreateCallSessionAsync(
                    request.CallerId,
                    request.ReceiverId,
                    request.CallType);
                return Ok(session);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error initiating call: {ex.Message}");
                return StatusCode(500, "Error initiating call");
            }
        }

        [HttpPut("{callId}/status")]
        public async Task<ActionResult<PrivateCallSession>> UpdateCallStatus(
            string callId,
            [FromBody] UpdateCallStatusRequest request)
        {
            try
            {
                var session = await _callSessionService.UpdateCallStatusAsync(
                    callId,
                    request.Status,
                    request.Reason);
                return Ok(session);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating call status: {ex.Message}");
                return StatusCode(500, "Error updating call status");
            }
        }

        [HttpPost("{callId}/end")]
        public async Task<ActionResult<PrivateCallSession>> EndCall(
            string callId,
            [FromBody] EndCallRequest request)
        {
            try
            {
                var session = await _callSessionService.EndCallAsync(callId, request.Reason);
                return Ok(session);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error ending call: {ex.Message}");
                return StatusCode(500, "Error ending call");
            }
        }

        [HttpGet("user/{userId}/statistics")]
        public async Task<ActionResult<CallStatistics>> GetUserCallStatistics(string userId)
        {
            try
            {
                var statistics = await _callSessionService.GetUserCallStatisticsAsync(userId);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting call statistics: {ex.Message}");
                return StatusCode(500, "Error getting call statistics");
            }
        }
        [HttpGet("{callId}")]
        public async Task<ActionResult<PrivateCallSession>> GetCallSession(string callId)
        {
            try
            {
                var session = await _callSessionService.GetCallSessionAsync(callId);
                return Ok(session);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving call session: {ex.Message}");
                return StatusCode(500, "Error retrieving call session");
            }
        }

        [HttpGet("user/{userId}/history")]
        public async Task<ActionResult<IEnumerable<PrivateCallSession>>> GetUserCallHistory(
            string userId,
            [FromQuery] int limit = 10)
        {
            try
            {
                var history = await _callSessionService.GetUserCallHistoryAsync(userId, limit);
                return Ok(history);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving call history: {ex.Message}");
                return StatusCode(500, "Error retrieving call history");
            }
        }

        [HttpPost("{callId}/answer")]
        public async Task<ActionResult<PrivateCallSession>> HandleCallAnswer(
            string callId,
            [FromBody] string answer)
        {
            try
            {
                var session = await _callSessionService.HandleCallAnswerAsync(callId, answer);
                return Ok(session);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error handling call answer: {ex.Message}");
                return StatusCode(500, "Error handling call answer");
            }
        }

        [HttpPut("{callId}/ice-candidates")]
        public async Task<ActionResult<PrivateCallSession>> UpdateIceCandidates(
            string callId,
            [FromBody] string iceCandidates)
        {
            try
            {
                var session = await _callSessionService.UpdateIceCandidatesAsync(callId, iceCandidates);
                return Ok(session);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating ICE candidates: {ex.Message}");
                return StatusCode(500, "Error updating ICE candidates");
            }
        }

        [HttpDelete("{callId}")]
        public async Task<ActionResult> DeleteCallSession(string callId)
        {
            try
            {
                var result = await _callSessionService.DeleteCallSessionAsync(callId);
                if (result)
                {
                    return NoContent();
                }
                return NotFound($"Call session {callId} not found");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting call session: {ex.Message}");
                return StatusCode(500, "Error deleting call session");
            }
        }

        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<PrivateCallSession>>> GetActiveCalls()
        {
            try
            {
                var activeCalls = await _callSessionService.GetActiveCallsAsync();
                return Ok(activeCalls);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving active calls: {ex.Message}");
                return StatusCode(500, "Error retrieving active calls");
            }
        }
    }
}