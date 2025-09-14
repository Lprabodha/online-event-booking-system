using Microsoft.EntityFrameworkCore;
using online_event_booking_system.Data;
using online_event_booking_system.Data.Entities;
using online_event_booking_system.Repository.Interface;

namespace online_event_booking_system.Repository.Service
{
    public class DiscountRepository : IDiscountRepository
    {
        private readonly ApplicationDbContext _context;

        public DiscountRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Discount>> GetAllAsync()
        {
            return await _context.Discounts
                .Include(d => d.Event)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Discount>> GetByOrganizerAsync(string organizerId)
        {
            return await _context.Discounts
                .Include(d => d.Event)
                .Where(d => d.Event != null && d.Event.OrganizerId == organizerId)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        public async Task<Discount?> GetByIdAsync(Guid id)
        {
            return await _context.Discounts
                .Include(d => d.Event)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<Discount?> GetByCodeAsync(string code)
        {
            return await _context.Discounts
                .Include(d => d.Event)
                .FirstOrDefaultAsync(d => d.Code == code.ToUpper());
        }

        public async Task<Discount> CreateAsync(Discount discount)
        {
            _context.Discounts.Add(discount);
            await _context.SaveChangesAsync();
            return discount;
        }

        public async Task<Discount> UpdateAsync(Discount discount)
        {
            _context.Discounts.Update(discount);
            await _context.SaveChangesAsync();
            return discount;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var discount = await _context.Discounts.FindAsync(id);
            if (discount == null)
                return false;

            _context.Discounts.Remove(discount);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(string code, Guid? excludeId = null)
        {
            var query = _context.Discounts.Where(d => d.Code == code.ToUpper());
            
            if (excludeId.HasValue)
            {
                query = query.Where(d => d.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }
    }
}
