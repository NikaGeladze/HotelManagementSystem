using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HMS.Domain.Entities;

public class ReservationRoom
{
    [ForeignKey(nameof(Reservation))]
    public Guid ReservationId { get; set; }

    public Reservation Reservation { get; set; }
    
    [ForeignKey(nameof(Room))]
    public Guid RoomId { get; set; }
    
    public Room Room { get; set; }
}