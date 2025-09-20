using online_event_booking_system.Data.Entities;

namespace online_event_booking_system.Repository.Interface
{
    public interface IDiscountRepository
    {
        /// <summary>
        /// Get all discounts in the system
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<Discount>> GetAllAsync();
        /// <summary>
        /// Get discounts by organizer
        /// </summary>
        /// <param name="organizerId"></param>
        /// <returns></returns>
        Task<IEnumerable<Discount>> GetByOrganizerAsync(string organizerId);
        /// <summary>
        /// Get discount by its unique identifier
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Discount?> GetByIdAsync(Guid id);
        /// <summary>
        /// Get discount by its unique code
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        Task<Discount?> GetByCodeAsync(string code);
        /// <summary>
        /// Get discount by event id
        /// </summary>
        /// <param name="discount"></param>
        /// <returns></returns>
        Task<Discount> CreateAsync(Discount discount);
        /// <summary>
        /// Update an existing discount
        /// </summary>
        /// <param name="discount"></param>
        /// <returns></returns>
        Task<Discount> UpdateAsync(Discount discount);
        /// <summary>
        /// Delete a discount
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<bool> DeleteAsync(Guid id);
        /// <summary>
        /// Check if a discount code exists, excluding a specific discount by id (useful for updates)
        /// </summary>
        /// <param name="code"></param>
        /// <param name="excludeId"></param>
        /// <returns></returns>
        Task<bool> ExistsAsync(string code, Guid? excludeId = null);
    }
}
