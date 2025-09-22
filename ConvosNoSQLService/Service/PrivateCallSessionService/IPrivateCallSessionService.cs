using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessObject.Models;
using BusinessObject.SupportModels;

namespace Service.PrivateCallSessionService
{
    public interface IPrivateCallSessionService
    {
        Task<PrivateCallSession> CreateCallSessionAsync(string callerId, string receiverId, string callType);
        Task<PrivateCallSession> GetCallSessionAsync(string callId);
        Task<IEnumerable<PrivateCallSession>> GetUserCallHistoryAsync(string userId, int limit = 10);
        Task<PrivateCallSession> UpdateCallStatusAsync(string callId, string status, string reason = null);
        Task<PrivateCallSession> HandleCallAnswerAsync(string callId, string answer);
        Task<PrivateCallSession> UpdateIceCandidatesAsync(string callId, string iceCandidates);
        Task<PrivateCallSession> EndCallAsync(string callId, string reason);
        Task<bool> DeleteCallSessionAsync(string callId);
        Task<IEnumerable<PrivateCallSession>> GetActiveCallsAsync();
        Task<CallStatistics> GetUserCallStatisticsAsync(string userId);
    }
}