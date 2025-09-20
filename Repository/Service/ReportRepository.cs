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

        /// <summary>
        /// Fetch events based on filters.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="category"></param>
        /// <param name="organizer"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Event>> GetEventsAsync(DateTime? from, DateTime? to, string category = null, string organizer = null)
        {
            var query = _context.Events
                .Include(e => e.Category)
                .Include(e => e.Organizer)
                .Include(e => e.Venue)
                .AsQueryable();

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
                query = query.Where(e => e.Category.Name == category);
            }

            if (!string.IsNullOrEmpty(organizer) && organizer != "All Organizers")
            {
                query = query.Where(e => e.OrganizerId == organizer);
            }

            return await query.ToListAsync();
        }

        /// <summary>
        /// Fetch users based on filters.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        public async Task<IEnumerable<ApplicationUser>> GetUsersAsync(DateTime? from, DateTime? to, string role = null)
        {
            var query = _context.Users.AsQueryable();

            if (from.HasValue)
            {
                query = query.Where(u => u.CreatedAt.Date >= from.Value.Date);
            }

            if (to.HasValue)
            {
                query = query.Where(u => u.CreatedAt.Date <= to.Value.Date);
            }

            if (!string.IsNullOrEmpty(role) && role != "All Roles")
            {
                var roleEntity = await _context.Roles.FirstOrDefaultAsync(r => r.Name == role);
                if (roleEntity != null)
                {
                    var userIdsInRole = _context.UserRoles.Where(ur => ur.RoleId == roleEntity.Id).Select(ur => ur.UserId);
                    query = query.Where(u => userIdsInRole.Contains(u.Id));
                }
            }

            return await query.OrderByDescending(u => u.CreatedAt).ToListAsync();
        }

        /// <summary>
        /// Fetch organizers based on filters.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Fetch customers based on filters.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
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
