using Kolokwium1.Models;
namespace Kolokwium1.Services;

public class IDeliveryServices
{
    Task<DeliveryDTO> getDelivery(string visitId);
    Task addNewDelivery(postDeliveryDTO newVisit);
}