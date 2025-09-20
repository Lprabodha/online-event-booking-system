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

        /// <summary>
        /// Initializes a new instance of the DiscountService class.
        /// </summary>
        /// <param name="context"></param>
        public DiscountService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all discounts in the system
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<Discount>> GetAllDiscountsAsync()
        {
            return await _context.Discounts
                .Include(d => d.Event)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Get discounts by organizer
        /// </summary>
        /// <param name="organizerId"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Discount>> GetDiscountsByOrganizerAsync(string organizerId)
        {
            return await _context.Discounts
                .Include(d => d.Event)
                .Where(d => d.Event != null && d.Event.OrganizerId == organizerId)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Get discount by its unique identifier
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Discount?> GetDiscountByIdAsync(Guid id)
        {
            return await _context.Discounts
                .Include(d => d.Event)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        /// <summary>
        /// Get discount by event id
        /// </summary>
        /// <param name="eventId"></param>
        /// <returns></returns>
        public async Task<Discount?> GetDiscountByEventIdAsync(Guid eventId)
        {
            return await _context.Discounts
                .Include(d => d.Event)
                .FirstOrDefaultAsync(d => d.EventId == eventId);
        }

        /// <summary>
        /// Get discount by code
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public async Task<Discount?> GetDiscountByCodeAsync(string code)
        {
            return await _context.Discounts
                .Include(d => d.Event)
                .FirstOrDefaultAsync(d => d.Code == code.ToUpper());
        }

        /// <summary>
        /// Create a new discount
        /// </summary>
        /// <param name="model"></param>
        /// <param name="organizerId"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Update an existing discount
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
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

        /// <summary>
        /// Delete a discount
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<bool> DeleteDiscountAsync(Guid id)
        {
            var discount = await _context.Discounts.FindAsync(id);
            if (discount == null)
                return false;

            _context.Discounts.Remove(discount);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Validate a discount code for an event
        /// </summary>
        /// <param name="code"></param>
        /// <param name="eventId"></param>
        /// <returns></returns>
        public async Task<bool> ValidateDiscountCodeAsync(string code, Guid? eventId = null)
        {
            var existingDiscount = await _context.Discounts
                .FirstOrDefaultAsync(d => d.Code == code.ToUpper());

            if (existingDiscount == null)
                return true; 

            return existingDiscount.Id == eventId;
        }

        /// <summary>
        /// Get available events for discount assignment
        /// </summary>
        /// <param name="organizerId"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Toggle the active status of a discount
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
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
