using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace HMS.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    [Required]
    [MaxLength(50)]
    public string FirstName { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string LastName { get; set; }
    
    [Required]
    [MaxLength(11)]
    [MinLength(11)]
    public string PersonalNumber { get; set; }

    [ForeignKey(nameof(ManagedHotel))]
    public Guid? HotelId { get; set; }

    public Hotel? ManagedHotel { get; set; }
    
    public ICollection<Reservation> GuestReservations { get; set; }
}