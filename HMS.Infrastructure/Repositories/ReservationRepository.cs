using HMS.Application.Interfaces.Repositories;
using HMS.Domain.Entities;
using HMS.Infrastructure.Data;

namespace HMS.Infrastructure.Repositories;

public class ReservationRepository : RepositoryBase<Reservation,ApplicationDbContext> , IReservationRepository
{
    public ReservationRepository(ApplicationDbContext context) : base(context)
    {
    }
}