using HMS.Application.Interfaces.Repositories;
using HMS.Domain.Entities;
using HMS.Infrastructure.Data;

namespace HMS.Infrastructure.Repositories;

public class RoomRepository : RepositoryBase<Room,ApplicationDbContext> , IRoomRepository
{
    public RoomRepository(ApplicationDbContext context) : base(context)
    {
    }
}