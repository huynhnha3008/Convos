using BusinessObjects.Models;

namespace Services.Interfaces
{
    public interface IMemberRoleRepository : IGenericRepository<MemberRole>
    {

        Task<MemberRole> GetByMemberIdAndRoleAsync(Guid memberId, Guid roleId);
        Task<List<MemberRole>> GetMembersByRoleIdAsync(Guid roleId);
        Task<List<MemberRole>> GetAllByMemberId(Guid serverMemberId);
    }
}
