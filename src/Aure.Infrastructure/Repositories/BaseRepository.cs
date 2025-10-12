using Microsoft.EntityFrameworkCore;
using Aure.Infrastructure.Data;
using Aure.Domain.Common;

namespace Aure.Infrastructure.Repositories;

public abstract class BaseRepository<T> where T : BaseEntity
{
    protected readonly AureDbContext _context;
    protected readonly DbSet<T> _dbSet;

    protected BaseRepository(AureDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public virtual async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
    }

    public virtual async Task UpdateAsync(T entity)
    {
        entity.UpdateTimestamp();
        _dbSet.Update(entity);
        await Task.CompletedTask;
    }

    public virtual async Task DeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            entity.MarkAsDeleted();
            await UpdateAsync(entity);
        }
    }

    public virtual async Task<bool> ExistsAsync(Guid id)
    {
        return await _dbSet.AnyAsync(x => x.Id == id);
    }

    protected IQueryable<T> GetQueryable()
    {
        return _dbSet.AsQueryable();
    }

    protected async Task<IEnumerable<T>> GetPagedAsync(IQueryable<T> query, int pageNumber, int pageSize)
    {
        return await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    protected async Task<int> GetCountAsync(IQueryable<T> query)
    {
        return await query.CountAsync();
    }
}