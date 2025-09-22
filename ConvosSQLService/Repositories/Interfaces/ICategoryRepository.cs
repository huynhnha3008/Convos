using BusinessObjects.Models;

namespace Services.Interfaces
{
    public interface ICategoryRepository : IGenericRepository<Category>
    {

        Task<List<Category>> GetAllAsync(Guid serverId);
    }

}
