using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;

namespace Repositories.impl
{

    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly ConvosDbContext _context;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(ConvosDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {



            return await _dbSet.ToListAsync();

        }


        public async Task<T> GetByIdAsync(Guid id)
        {


            return await _dbSet.FindAsync(id) as T;

        }



        public async Task<T> CreateAsync(T entity)
        {
            _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<T> UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }


        public async Task<T> DeleteAsync(T entity)
        {



            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
            return entity;



        }







        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }




    }
}

