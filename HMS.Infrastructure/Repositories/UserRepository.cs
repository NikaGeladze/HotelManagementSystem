using HMS.Application.Interfaces.Repositories;
using HMS.Domain.Entities;
using HMS.Infrastructure.Data;

namespace HMS.Infrastructure.Repositories;

public class UserRepository : RepositoryBase<ApplicationUser,ApplicationDbContext> , IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }
}