using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using BusinessObject.Models;
using BusinessObject.SupportModels;
using Microsoft.Extensions.Logging;
using Repository.UnitOfWork;

namespace Service.PrivateCallSessionService
{
    public class PrivateCallSessionService : IPrivateCallSessionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PrivateCallSessionService> _logger;

        public PrivateCallSessionService(IUnitOfWork unitOfWork, ILogger<PrivateCallSessionService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<PrivateCallSession> CreateCallSessionAsync(string callerId, string receiverId, string callType)
        {
            try
            {
                var session = new PrivateCallSession
                {
                    CallerId = callerId,
                    ReceiverId = receiverId,
                    CallType = callType,
                    StartTime = DateTime.UtcNow,
                    Status = "initiated",
                    DeviceInfo = GetDeviceInfo(),
                    NotificationLog = JsonSerializer.Serialize(new[] { new { Event = "initiated", Time = DateTime.UtcNow } })
                };

                await _unitOfWork.PrivateCallSessions.AddAsync(session);
                await _unitOfWork.SaveAsync();

                _logger.LogInformation($"Call session created: {session.CallId}");
                return session;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating call session: {ex.Message}");
                throw;
            }
        }

        public async Task<PrivateCallSession> UpdateCallStatusAsync(string callId, string status, string reason = null)
        {
            var session = await _unitOfWork.PrivateCallSessions.GetByIdAsync(callId);
            if (session == null) throw new NotFoundException("Call session not found");

            session.Status = status;
            if (reason != null) session.DisconnectedReason = reason;

            var notifications = JsonSerializer.Deserialize<List<object>>(session.NotificationLog ?? "[]");
            notifications.Add(new { Event = status, Time = DateTime.UtcNow, Reason = reason });
            session.NotificationLog = JsonSerializer.Serialize(notifications);

            await _unitOfWork.PrivateCallSessions.UpdateAsync(callId, session);
            await _unitOfWork.SaveAsync();

            return session;
        }

        public async Task<PrivateCallSession> EndCallAsync(string callId, string reason)
        {
            var session = await _unitOfWork.PrivateCallSessions.GetByIdAsync(callId);
            if (session == null) throw new NotFoundException("Call session not found");

            session.Status = "ended";
            session.EndTime = DateTime.UtcNow;
            session.DisconnectedReason = reason;
            session.QualityMetrics = await CalculateQualityMetrics(session);

            await _unitOfWork.PrivateCallSessions.UpdateAsync(callId, session);
            await _unitOfWork.SaveAsync();

            return session;
        }

        public async Task<CallStatistics> GetUserCallStatisticsAsync(string userId)
        {
            var allCalls = await _unitOfWork.PrivateCallSessions.GetAllAsync();
            var calls = allCalls.Where
                (c => c.CallerId == userId || c.ReceiverId == userId)
                .ToList();

            return new CallStatistics
            {
                TotalCalls = calls.Count,
                MissedCalls = calls.Count(c => c.Status == "missed"),
                AverageCallDuration = calls
                    .Where(c => c.EndTime.HasValue)
                    .Average(c => (c.EndTime.Value - c.StartTime).TotalMinutes),
                CallsByType = calls
                    .GroupBy(c => c.CallType)
                    .ToDictionary(g => g.Key, g => g.Count())
            };
        }

        private string GetDeviceInfo()
        {
            return JsonSerializer.Serialize(new
            {
                Platform = Environment.OSVersion.Platform,
                Version = Environment.OSVersion.Version.ToString(),
                MachineName = Environment.MachineName
            });
        }

        private async Task<string> CalculateQualityMetrics(PrivateCallSession session)
        {
            return JsonSerializer.Serialize(new
            {
                Duration = (session.EndTime - session.StartTime)?.TotalMinutes,
            });
        }
        public async Task<PrivateCallSession> GetCallSessionAsync(string callId)
        {
            try
            {
                var session = await _unitOfWork.PrivateCallSessions.GetByIdAsync(callId);
                if (session == null)
                {
                    throw new NotFoundException($"Call session with ID {callId} not found");
                }
                return session;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving call session: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<PrivateCallSession>> GetUserCallHistoryAsync(string userId, int limit = 10)
        {
            try
            {
                var allCalls = await _unitOfWork.PrivateCallSessions.GetAllAsync();
                var calls = allCalls.Where(c => c.CallerId == userId || c.ReceiverId == userId)
                    .OrderByDescending(c => c.StartTime)
                    .Take(limit)
                    .ToList();

                return calls;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving call history for user {userId}: {ex.Message}");
                throw;
            }
        }

        public async Task<PrivateCallSession> HandleCallAnswerAsync(string callId, string answer)
        {
            try
            {
                var session = await GetCallSessionAsync(callId);
                session.Answer = answer;
                session.Status = "accepted";

                // Update notification log
                var notifications = JsonSerializer.Deserialize<List<object>>(session.NotificationLog ?? "[]");
                notifications.Add(new { Event = "answered", Time = DateTime.UtcNow });
                session.NotificationLog = JsonSerializer.Serialize(notifications);

                await _unitOfWork.PrivateCallSessions.UpdateAsync(callId, session);
                await _unitOfWork.SaveAsync();

                _logger.LogInformation($"Call {callId} answered successfully");
                return session;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error handling call answer: {ex.Message}");
                throw;
            }
        }

        public async Task<PrivateCallSession> UpdateIceCandidatesAsync(string callId, string iceCandidates)
        {
            try
            {
                var session = await GetCallSessionAsync(callId);
                session.IceCandidates = iceCandidates;

                await _unitOfWork.PrivateCallSessions.UpdateAsync(callId, session);
                await _unitOfWork.SaveAsync();

                _logger.LogInformation($"ICE candidates updated for call {callId}");
                return session;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating ICE candidates: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteCallSessionAsync(string callId)
        {
            try
            {
                var session = await GetCallSessionAsync(callId);
                await _unitOfWork.PrivateCallSessions.DeleteAsync(callId);
                await _unitOfWork.SaveAsync();

                _logger.LogInformation($"Call session {callId} deleted successfully");
                return true;
            }
            catch (NotFoundException)
            {
                _logger.LogWarning($"Attempted to delete non-existent call session {callId}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting call session: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<PrivateCallSession>> GetActiveCallsAsync()
        {
            try
            {
                var allCalls = await _unitOfWork.PrivateCallSessions.GetAllAsync();
                var activeCalls = allCalls.Where(c => c.Status == "accepted" && !c.EndTime.HasValue)
                    .ToList();

                return activeCalls;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving active calls: {ex.Message}");
                throw;
            }
        }
    }
}