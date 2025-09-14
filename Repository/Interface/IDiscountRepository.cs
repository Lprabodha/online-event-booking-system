using online_event_booking_system.Data.Entities;

namespace online_event_booking_system.Repository.Interface
{
    public interface IDiscountRepository
    {
        Task<IEnumerable<Discount>> GetAllAsync();
        Task<IEnumerable<Discount>> GetByOrganizerAsync(string organizerId);
        Task<Discount?> GetByIdAsync(Guid id);
        Task<Discount?> GetByCodeAsync(string code);
        Task<Discount> CreateAsync(Discount discount);
        Task<Discount> UpdateAsync(Discount discount);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ExistsAsync(string code, Guid? excludeId = null);
    }
}
