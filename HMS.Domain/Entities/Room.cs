using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HMS.Domain.Entities;

public class Room
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; }
    
    [Required]
    [Range(1,Int32.MaxValue)]
    public decimal Price { get; set; }

    [ForeignKey(nameof(Hotel))]
    public Guid HotelId { get; set; }
    
    public Hotel Hotel { get; set; }
    
    public ICollection<ReservationRoom> ReservationRooms { get; set; }
}