using online_event_booking_system.Data.Entities;
using online_event_booking_system.Models;

namespace online_event_booking_system.Business.Interface
{
    public interface IDiscountService
    {
        Task<IEnumerable<Discount>> GetAllDiscountsAsync();
        Task<IEnumerable<Discount>> GetDiscountsByOrganizerAsync(string organizerId);
        Task<Discount?> GetDiscountByIdAsync(Guid id);
        Task<Discount?> GetDiscountByEventIdAsync(Guid eventId);
        Task<Discount?> GetDiscountByCodeAsync(string code);
        Task<Discount> CreateDiscountAsync(DiscountViewModel model, string organizerId);
        Task<Discount> UpdateDiscountAsync(Guid id, DiscountViewModel model);
        Task<bool> DeleteDiscountAsync(Guid id);
        Task<bool> ValidateDiscountCodeAsync(string code, Guid? eventId = null);
        Task<IEnumerable<EventOption>> GetAvailableEventsAsync(string organizerId);
        Task<bool> ToggleDiscountStatusAsync(Guid id);
    }
}
