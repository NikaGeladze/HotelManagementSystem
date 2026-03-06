using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace HMS.Application.Interfaces.Repositories;

public interface IRepositoryBase<T, TContext>
    where T : class
    where TContext : DbContext
{
    Task AddAsync(T entity);
    void Remove(T entity);
    void RemoveRange(IEnumerable<T> entitites);
    void Update(T entity);
    Task<int> SaveAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
    Task<(List<T> Items, int TotalCount)> GetAllAsync(
        Expression<Func<T, bool>> filter = null,
        int? pageNumber = null,
        int? pageSize = null,
        string orderBy = null,
        bool ascending = true,
        Func<IQueryable<T>, IQueryable<T>> includes = null,
        bool tracking = true);
    Task<T> GetAsync(
        Expression<Func<T, bool>> filter,
        Func<IQueryable<T>, IQueryable<T>> includes = null,
        bool tracking = true);
}