using Microsoft.EntityFrameworkCore;
using online_event_booking_system.Business.Interface;
using online_event_booking_system.Data;
using online_event_booking_system.Data.Entities;
using online_event_booking_system.Models;

namespace online_event_booking_system.Business.Service
{
    public class DiscountService : IDiscountService
    {
        private readonly ApplicationDbContext _context;

        public DiscountService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Discount>> GetAllDiscountsAsync()
        {
            return await _context.Discounts
                .Include(d => d.Event)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Discount>> GetDiscountsByOrganizerAsync(string organizerId)
        {
            return await _context.Discounts
                .Include(d => d.Event)
                .Where(d => d.Event != null && d.Event.OrganizerId == organizerId)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        public async Task<Discount?> GetDiscountByIdAsync(Guid id)
        {
            return await _context.Discounts
                .Include(d => d.Event)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<Discount?> GetDiscountByEventIdAsync(Guid eventId)
        {
            return await _context.Discounts
                .Include(d => d.Event)
                .FirstOrDefaultAsync(d => d.EventId == eventId);
        }

        public async Task<Discount?> GetDiscountByCodeAsync(string code)
        {
            return await _context.Discounts
                .Include(d => d.Event)
                .FirstOrDefaultAsync(d => d.Code == code.ToUpper());
        }

        public async Task<Discount> CreateDiscountAsync(DiscountViewModel model, string organizerId)
        {
            var discount = new Discount
            {
                Code = model.Code.ToUpper(),
                Type = model.Type == DiscountType.Percentage ? "Percent" : "Amount",
                Value = model.Value,
                ValidFrom = DateTime.UtcNow,
                ValidTo = model.ExpiryDate ?? DateTime.UtcNow.AddMonths(1),
                IsActive = model.IsActive,
                Description = model.Description,
                UsageLimit = model.UsageLimit,
                UsedCount = 0,
                CreatedAt = DateTime.UtcNow
            };

            // Event is now always required
            if (model.EventId.HasValue)
            {
                var eventEntity = await _context.Events
                    .FirstOrDefaultAsync(e => e.Id == model.EventId && e.OrganizerId == organizerId);
                
                if (eventEntity != null)
                {
                    discount.EventId = model.EventId;
                }
            }

            _context.Discounts.Add(discount);
            await _context.SaveChangesAsync();

            return discount;
        }

        public async Task<Discount> UpdateDiscountAsync(Guid id, DiscountViewModel model)
        {
            var discount = await _context.Discounts.FindAsync(id);
            if (discount == null)
                throw new ArgumentException("Discount not found");

            discount.Code = model.Code.ToUpper();
            discount.Type = model.Type == DiscountType.Percentage ? "Percent" : "Amount";
            discount.Value = model.Value;
            discount.ValidTo = model.ExpiryDate ?? discount.ValidTo;
            discount.IsActive = model.IsActive;
            discount.Description = model.Description;
            discount.UsageLimit = model.UsageLimit;
            discount.UpdatedAt = DateTime.UtcNow;

            // Event is now always required
            if (model.EventId.HasValue)
            {
                discount.EventId = model.EventId;
            }

            await _context.SaveChangesAsync();
            return discount;
        }

        public async Task<bool> DeleteDiscountAsync(Guid id)
        {
            var discount = await _context.Discounts.FindAsync(id);
            if (discount == null)
                return false;

            _context.Discounts.Remove(discount);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ValidateDiscountCodeAsync(string code, Guid? eventId = null)
        {
            var existingDiscount = await _context.Discounts
                .FirstOrDefaultAsync(d => d.Code == code.ToUpper());

            if (existingDiscount == null)
                return true; 

            return existingDiscount.Id == eventId;
        }

        public async Task<IEnumerable<EventOption>> GetAvailableEventsAsync(string organizerId)
        {
            var events = await _context.Events
                .Where(e => e.OrganizerId == organizerId && e.EventDate > DateTime.UtcNow)
                .OrderBy(e => e.EventDate)
                .Select(e => new EventOption
                {
                    Id = e.Id,
                    Name = e.Title,
                    EventDate = e.EventDate
                })
                .ToListAsync();

            return events;
        }

        public async Task<bool> ToggleDiscountStatusAsync(Guid id)
        {
            var discount = await _context.Discounts.FindAsync(id);
            if (discount == null)
                return false;

            discount.IsActive = !discount.IsActive;
            discount.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
