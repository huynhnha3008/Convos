using BusinessObjects.DTOs;
using BusinessObjects.Models;
namespace Services.Interfaces
{
    public interface IInviteUsageService
    {
        Task<string> JoinServerByInviteCodeAsync(InviteUsageCreateRequest request, Guid userId);
    }
}
