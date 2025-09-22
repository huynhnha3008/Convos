using BusinessObjects.DTOs;
using BusinessObjects.Models;
using BusinessObjects.QueryObject;

namespace Services.Interfaces
{
    public interface ICategoryService
    {
        Task<List<CategoryDetailResponse>> GetAllAsync(Guid serverId, QueryCategory query);

        Task<CategoryDetailResponse> GetByIdAsync(Guid id);
        Task<Category> CreateAsync(CategoryCreateRequest category,Guid userId, int serverType);
        Task<string> UpdateAsync(Guid id,Guid userId, CategoryUpdateRequest request);

        Task<List<CategoryDetailResponse>> SearchAsync(Guid serverId, QueryCategory query);

        Task<string> DeleteAsync(Guid id, Guid userId);
        Task<string> ChangePositionAsync(Guid id, int newPosition, Guid userId);
    }
}
