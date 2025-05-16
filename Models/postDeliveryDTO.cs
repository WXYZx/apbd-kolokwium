using System.ComponentModel.DataAnnotations;

namespace Kolokwium1.Models;

public class postDeliveryDTO
{
    [Required]public int DeliveryId { get; set; }
    [Required]public int CustomerId { get; set; }
    [Required]public string LicenceNumber { get; set; }
    [Required]public List<Product> Products { get; set; }
}