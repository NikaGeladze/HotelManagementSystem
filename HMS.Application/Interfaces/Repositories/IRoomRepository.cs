using HMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HMS.Application.Interfaces.Repositories;

public interface IRoomRepository : IRepositoryBase<Room,DbContext>
{
    
}