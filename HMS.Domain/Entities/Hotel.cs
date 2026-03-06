using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HMS.Domain.Entities;

public class Hotel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Name { get; set; }
    
    [Required]
    [Range(1,5)]
    public byte Rating { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Country { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string City { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Address { get; set; }
    
    public ICollection<ApplicationUser> Managers { get; set; }
    
    public ICollection<Room> Rooms { get; set; }
}