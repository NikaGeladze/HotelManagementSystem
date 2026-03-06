using HMS.Application.Interfaces.Repositories;
using HMS.Domain.Entities;
using HMS.Infrastructure.Data;

namespace HMS.Infrastructure.Repositories;

public class HotelRepository : RepositoryBase<Hotel,ApplicationDbContext> , IHotelRepository
{
    public HotelRepository(ApplicationDbContext context) : base(context)
    {
        
    }
}