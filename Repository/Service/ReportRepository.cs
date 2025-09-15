using online_event_booking_system.Data.Entities;
using online_event_booking_system.Data;
using online_event_booking_system.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace online_event_booking_system.Repository.Service
{
    public class ReportRepository : IReportRepository
    {
        private readonly ApplicationDbContext _context;

        public ReportRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Event>> GetEventsAsync(DateTime? from, DateTime? to, string category = null, string status = null)
        {
            var query = _context.Events.AsQueryable();

            if (from.HasValue)
            {
                query = query.Where(e => e.EventDate.Date >= from.Value.Date);
            }

            if (to.HasValue)
            {
                query = query.Where(e => e.EventDate.Date <= to.Value.Date);
            }

            if (!string.IsNullOrEmpty(category) && category != "All Categories")
            {
                // Assuming Category has a name or some identifiable property
                query = query.Where(e => e.Category.Name == category);
            }

            if (!string.IsNullOrEmpty(status) && status != "All Status")
            {
                query = query.Where(e => e.Status == status);
            }

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<ApplicationUser>> GetUsersAsync(DateTime? from, DateTime? to)
        {
            var query = _context.Users.AsQueryable();

            // Assuming you've added a CreatedAt column to ApplicationUser
            if (from.HasValue)
            {
                query = query.Where(u => u.CreatedAt.Date >= from.Value.Date);
            }

            if (to.HasValue)
            {
                query = query.Where(u => u.CreatedAt.Date <= to.Value.Date);
            }

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<ApplicationUser>> GetOrganizersAsync(DateTime? from, DateTime? to)
        {
            // Get users with the "Organizer" role
            var organizerRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Organizer");
            if (organizerRole == null) return new List<ApplicationUser>();

            var userIdsInRole = _context.UserRoles.Where(ur => ur.RoleId == organizerRole.Id).Select(ur => ur.UserId);

            var query = _context.Users.Where(u => userIdsInRole.Contains(u.Id));

            if (from.HasValue)
            {
                query = query.Where(u => u.CreatedAt.Date >= from.Value.Date);
            }

            if (to.HasValue)
            {
                query = query.Where(u => u.CreatedAt.Date <= to.Value.Date);
            }

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<ApplicationUser>> GetCustomersAsync(DateTime? from, DateTime? to)
        {
            // Get users with the "Customer" role
            var customerRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Customer");
            if (customerRole == null) return new List<ApplicationUser>();

            var userIdsInRole = _context.UserRoles.Where(ur => ur.RoleId == customerRole.Id).Select(ur => ur.UserId);

            var query = _context.Users.Where(u => userIdsInRole.Contains(u.Id));

            if (from.HasValue)
            {
                query = query.Where(u => u.CreatedAt.Date >= from.Value.Date);
            }

            if (to.HasValue)
            {
                query = query.Where(u => u.CreatedAt.Date <= to.Value.Date);
            }

            return await query.ToListAsync();
        }
    }
}
