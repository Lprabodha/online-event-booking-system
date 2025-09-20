using online_event_booking_system.Data.Entities;
using online_event_booking_system.Models;

namespace online_event_booking_system.Business.Interface
{
    public interface IDiscountService
    {
        /// <summary>
        /// Get all discounts
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<Discount>> GetAllDiscountsAsync();
        /// <summary>
        /// Get discounts by organizer
        /// </summary>
        /// <param name="organizerId"></param>
        /// <returns></returns>
        Task<IEnumerable<Discount>> GetDiscountsByOrganizerAsync(string organizerId);
        /// <summary>
        /// Get discount by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Discount?> GetDiscountByIdAsync(Guid id);
        /// <summary>
        /// Get discount by event id
        /// </summary>
        /// <param name="eventId"></param>
        /// <returns></returns>
        Task<Discount?> GetDiscountByEventIdAsync(Guid eventId);
        /// <summary>
        /// Get discount by code
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        Task<Discount?> GetDiscountByCodeAsync(string code);
        /// <summary>
        /// Create a new discount
        /// </summary>
        /// <param name="model"></param>
        /// <param name="organizerId"></param>
        /// <returns></returns>
        Task<Discount> CreateDiscountAsync(DiscountViewModel model, string organizerId);
        /// <summary>
        /// Update an existing discount
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<Discount> UpdateDiscountAsync(Guid id, DiscountViewModel model);
        /// <summary>
        /// Delete a discount
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<bool> DeleteDiscountAsync(Guid id);
        /// <summary>
        /// Validate a discount code for an event
        /// </summary>
        /// <param name="code"></param>
        /// <param name="eventId"></param>
        /// <returns></returns>
        Task<bool> ValidateDiscountCodeAsync(string code, Guid? eventId = null);
        /// <summary>
        /// Get available events for discount creation by organizer
        /// </summary>
        /// <param name="organizerId"></param>
        /// <returns></returns>
        Task<IEnumerable<EventOption>> GetAvailableEventsAsync(string organizerId);
        /// <summary>
        /// Toggle discount active status
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<bool> ToggleDiscountStatusAsync(Guid id);
    }
}
