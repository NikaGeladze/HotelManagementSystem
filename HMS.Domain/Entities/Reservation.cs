using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HMS.Domain.Entities;

public class Reservation
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    
    [Required]
    public DateOnly CheckInDate { get; set; }
    
    [Required]
    public DateOnly CheckOutDate { get; set; }
    
    [ForeignKey(nameof(Guest))]
    public string GuestId { get; set; }
    public ApplicationUser Guest { get; set; }
    
    public ICollection<ReservationRoom> ReservationRooms { get; set; }
}