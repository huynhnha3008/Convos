namespace Repositories.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> GetByIdAsync(Guid id);
        Task<T> CreateAsync(T entity);



        Task<T> UpdateAsync(T entity);
        Task<T> DeleteAsync(T entity);
        Task SaveAsync();
    }
}
