using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using online_event_booking_system.Business.Interface;
using online_event_booking_system.Business.Service;
using online_event_booking_system.Data.Entities;
using online_event_booking_system.Models.View_Models;
using online_event_booking_system.Services;
using online_event_booking_system.Business.Interface;

namespace online_event_booking_system.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IVenueService _venueService;
        private readonly IEventService _eventService;
        private readonly IS3Service _s3Service;

        /// <summary>
        /// Constructor for AdminController
        /// </summary>
        /// <param name="adminService"></param>
        /// <param name="userManager"></param>
        /// <param name="venueService"></param>
        public AdminController(IAdminService adminService, UserManager<ApplicationUser> userManager, IVenueService venueService, IS3Service s3Service, IEventService eventService)
        {
            _adminService = adminService;
            _userManager = userManager;
            _venueService = venueService;
            _s3Service = s3Service;
            _eventService = eventService;
        }

        /// <summary>
        /// Admin dashboard displaying all users
        /// </summary>
        /// <returns></returns>
        [HttpGet("admin")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var users = await _adminService.GetAllUsers();

                // User growth (last 6 months)
                var now = DateTime.UtcNow;
                var growthLabels = new List<string>();
                var growthData = new List<int>();
                for (int i = 5; i >= 0; i--)
                {
                    var month = new DateTime(now.Year, now.Month, 1).AddMonths(-i);
                    growthLabels.Add(month.ToString("MMM"));
                    var count = users.Count(u => u.CreatedAt.Year == month.Year && u.CreatedAt.Month == month.Month);
                    growthData.Add(count);
                }
                ViewBag.UserGrowthLabels = growthLabels;
                ViewBag.UserGrowthData = growthData;

                // User activity - daily logins (last 7 days)
                var activityLabels = new List<string>();
                var activityData = new List<int>();
                for (int i = 6; i >= 0; i--)
                {
                    var day = now.Date.AddDays(-i);
                    activityLabels.Add(day.ToString("MMM dd"));
                    var count = users.Count(u => u.LastLogin.HasValue && u.LastLogin.Value.Date == day);
                    activityData.Add(count);
                }
                ViewBag.UserActivityLabels = activityLabels;
                ViewBag.UserActivityData = activityData;

                // Platform performance: events created, tickets sold, revenue (last 30 days)
                var context = HttpContext.RequestServices.GetRequiredService<online_event_booking_system.Data.ApplicationDbContext>();
                var start30 = now.Date.AddDays(-30);
                var perfLabels = new List<string>();
                var eventsData = new List<int>();
                var ticketsData = new List<int>();
                var revenueData = new List<decimal>();
                for (var d = start30; d <= now.Date; d = d.AddDays(1))
                {
                    perfLabels.Add(d.ToString("MMM dd"));
                    eventsData.Add(await context.Events.CountAsync(e => e.CreatedAt.Date == d));
                    var dayTickets = await context.Tickets.CountAsync(t => t.PurchaseDate.Date == d && t.IsPaid);
                    ticketsData.Add(dayTickets);
                    var dayRevenue = await context.Payments.Where(p => p.Status == "Completed" && p.PaidAt.Date == d).SumAsync(p => (decimal?)p.Amount) ?? 0m;
                    revenueData.Add(dayRevenue);
                }
                ViewBag.PerfLabels = perfLabels;
                ViewBag.PerfEvents = eventsData;
                ViewBag.PerfTickets = ticketsData;
                ViewBag.PerfRevenue = revenueData;

                // Recent activity (registrations and logins)
                var recent = new List<object>();
                recent.AddRange(users
                    .OrderByDescending(u => u.CreatedAt)
                    .Take(3)
                    .Select(u => new { Type = "register", Title = "New user registered", Subtitle = u.Email ?? u.UserName, When = (now - u.CreatedAt).TotalMinutes }));

                recent.AddRange(users
                    .Where(u => u.LastLogin.HasValue)
                    .OrderByDescending(u => u.LastLogin!.Value)
                    .Take(2)
                    .Select(u => new { Type = "login", Title = "User logged in", Subtitle = u.Email ?? u.UserName, When = (now - u.LastLogin!.Value).TotalMinutes }));

                ViewBag.RecentActivity = recent
                    .OrderBy(x => ((double)x.GetType().GetProperty("When")!.GetValue(x)!).CompareTo(0))
                    .Take(5)
                    .ToList();

                return View(users);
            }
            catch (Exception ex)
            {
                return View(new List<ApplicationUser>());
            }
        }

        /// <summary>
        /// View details of a specific user by ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }
            
            try
            {
                var user = await _adminService.GetUserById(id);
                if (user == null)
                {
                    return NotFound();
                }
                return View(user);
            }
            catch
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Display form to create a new user
        /// </summary>
        /// <returns></returns>
        
        // GET: Admin/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ApplicationUser user, string password, string role)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.UserName) || string.IsNullOrEmpty(password))
                {
                    ModelState.AddModelError(string.Empty, "Email, Username, and Password are required.");
                    return View(user);
                }

                try
                {
                    var (succeeded, errors) = await _adminService.CreateUser(user, password, role);
                    if (succeeded)
                    {
                        TempData["SuccessMessage"] = "User created successfully.";
                        return RedirectToAction(nameof(Index));
                    }
                    foreach (var error in errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                }
            }
            return View(user);
        }

        // GET: Admin/Edit/5

        /// <summary>
        /// Load the edit organizer modal with user data
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> LoadEditOrganizerModal(string id)
        {
            var user = await _adminService.GetUserById(id);
            if (user == null)
            {
                return NotFound();
            }
            return PartialView("_EditOrganizerModal", user);
        }

        // POST: Admin/Edit/5

        /// <summary>
        /// Handle the submission of edited organizer details
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditOrganizer(ApplicationUser user)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.UserName))
                {
                    ModelState.AddModelError(string.Empty, "Email and Username are required.");
                    return PartialView("_EditOrganizerModal", user);
                }

                try
                {
                    var result = await _adminService.UpdateUser(user);
                    if (result)
                    {
                        TempData["SuccessMessage"] = "Organizer updated successfully.";
                        return Json(new { success = true, message = "Organizer updated successfully." });
                    }
                    ModelState.AddModelError(string.Empty, "Failed to update organizer.");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                }
            }
            return PartialView("_EditOrganizerModal", user);
        }

        // POST: Admin/Delete/5

        /// <summary>
        /// Soft delete a user by ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                var result = await _adminService.SoftDeleteUser(id);
                if (!result)
                {
                    return NotFound();
                }
                TempData["SuccessMessage"] = "User deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Toggle a user's active status
        /// </summary>
        /// <param name="id"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(string id, int? page = null)
        {
            try
            {
                var user = await _adminService.GetUserById(id);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction(nameof(Users), new { page = page ?? 1 });
                }

                var result = await _adminService.ToggleUserStatus(id);
                if (!result)
                {
                    TempData["ErrorMessage"] = "Failed to update user status.";
                    return RedirectToAction(nameof(Users), new { page = page ?? 1 });
                }

                var statusMessage = user.IsActive ? "User deactivated successfully." : "User activated successfully.";
                TempData["SuccessMessage"] = statusMessage;
                return RedirectToAction(nameof(Users), new { page = page ?? 1 });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while updating user status.";
                return RedirectToAction(nameof(Users), new { page = page ?? 1 });
            }
        }

        /// <summary>
        /// List all users with pagination
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>

        [HttpGet("admin/users")]
        public async Task<IActionResult> Users(int page = 1, int pageSize = 10)
        {
            try
            {
                var users = await _adminService.GetAllUsersWithRoles();
                
                // Calculate pagination
                var totalItems = users.Count();
                var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
                var skip = (page - 1) * pageSize;
                var paginatedUsers = users.Skip(skip).Take(pageSize).ToList();
                
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalItems = totalItems;
                ViewBag.StartItem = skip + 1;
                ViewBag.EndItem = Math.Min(skip + pageSize, totalItems);
                
                return View(paginatedUsers);
            }
            catch (Exception ex)
            {
                return View(new List<UserWithRoleViewModel>());
            }
        }

        /// <summary>
        /// Admin events management page
        /// </summary>
        /// <returns></returns>
        [HttpGet("admin/events")]
        public async Task<IActionResult> Events(string? search, string? status, int page = 1, int pageSize = 12)
        {
            try
            {
                // Get all events with related data
                var events = await _adminService.GetAllEventsAsync();
                // Ensure image paths are direct S3/CDN URLs
                foreach (var e in events)
                {
                    if (!string.IsNullOrEmpty(e.Image))
                    {
                        e.Image = _s3Service.GetDirectUrl(e.Image);
                    }
                }
                
                // Apply search filter
                if (!string.IsNullOrEmpty(search))
                {
                    events = events.Where(e => 
                        e.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        e.Description.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        e.Venue.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        e.Organizer.FullName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        e.Category.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
                    );
                }

                // Apply status filter
                if (!string.IsNullOrEmpty(status) && status != "all")
                {
                    events = events.Where(e => e.Status.ToLower() == status.ToLower());
                }

                // Calculate event status based on date
                var now = DateTime.UtcNow;
                var eventsWithStatus = events.Select(e => new
                {
                    Event = e,
                    CalculatedStatus = e.EventDate < now ? "ended" : 
                                     e.EventDate.AddHours(2) > now && e.EventDate <= now ? "live" : 
                                     e.Status == "Cancelled" ? "cancelled" : "upcoming"
                });

                // Apply calculated status filter
                if (!string.IsNullOrEmpty(status) && status != "all" && status != "upcoming" && status != "live" && status != "ended" && status != "cancelled")
                {
                    // Filter by calculated status
                    eventsWithStatus = eventsWithStatus.Where(e => e.CalculatedStatus == status);
                }

                var eventsList = eventsWithStatus.ToList();
                
                // Calculate pagination
                var totalItems = eventsList.Count;
                var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
                var skip = (page - 1) * pageSize;
                var paginatedEvents = eventsList.Skip(skip).Take(pageSize).ToList();

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalItems = totalItems;
                ViewBag.StartItem = skip + 1;
                ViewBag.EndItem = Math.Min(skip + pageSize, totalItems);
                ViewBag.SearchTerm = search;
                ViewBag.SelectedStatus = status;

                return View(paginatedEvents);
            }
            catch (Exception ex)
            {
                return View(new List<object>());
            }
        }

        /// <summary>
        /// Admin view: per-event analytics (tickets sold, buyers, revenue)
        /// </summary>
        [HttpGet("admin/event-analytics/{id}")]
        public async Task<IActionResult> EventAnalytics(Guid id)
        {
            try
            {
                var analytics = await _eventService.GetEventAnalyticsAsync(id);
                if (analytics == null)
                {
                    TempData["ErrorMessage"] = "Event not found.";
                    return RedirectToAction(nameof(Events));
                }
                return View("~/Views/Organizer/EventAnalytics.cshtml", analytics);
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Unable to load analytics.";
                return RedirectToAction(nameof(Events));
            }
        }

        [HttpGet("admin/organizers")]
        public async Task<IActionResult> Organizers(int page = 1, int pageSize = 6)
        {
            var organizers = await _adminService.GetUsersByRole("Organizer");
            
            var totalItems = organizers.Count();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            var skip = (page - 1) * pageSize;
            var paginatedOrganizers = organizers.Skip(skip).Take(pageSize).ToList();
            
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.StartItem = skip + 1;
            ViewBag.EndItem = Math.Min(skip + pageSize, totalItems);
            
            return View("Organizers", paginatedOrganizers);
        }

        /// <summary>
        /// Admin venues management page
        /// </summary>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <returns></returns>
        [HttpGet("admin/venues")]
        public async Task<IActionResult> Venues(int page = 1, int pageSize = 6)
        {
            try
            {
                var allVenues = await _venueService.GetAllVenuesAsync();
                var venuesList = allVenues.ToList();
                
                // Calculate pagination
                var totalItems = venuesList.Count;
                var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
                var skip = (page - 1) * pageSize;
                var paginatedVenues = venuesList.Skip(skip).Take(pageSize).ToList();
                
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalItems = totalItems;
                ViewBag.StartItem = skip + 1;
                ViewBag.EndItem = Math.Min(skip + pageSize, totalItems);
                
                return View(paginatedVenues);
            }
            catch (Exception ex)
            {
                return View(new List<Venue>());
            }
        }

        /// <summary>
        /// Create a new venue
        /// </summary>
        /// <param name="venue"></param>
        /// <returns></returns>
        [HttpPost("admin/venues/create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateVenue(Venue venue)
        {
            // For create operation, ensure Id is set to new Guid
            venue.Id = Guid.NewGuid();

            // Handle empty capacity field
            if (string.IsNullOrEmpty(Request.Form["Capacity"]) || !int.TryParse(Request.Form["Capacity"], out int capacity))
            {
                ModelState.AddModelError("Capacity", "Capacity is required and must be a valid number.");
            }
            else
            {
                venue.Capacity = capacity;
            }

            // Clear any existing capacity and id errors and re-validate
            ModelState.Remove("Capacity");
            ModelState.Remove("Id");
            TryValidateModel(venue);

            if (ModelState.IsValid)
            {
                try
                {
                    var createdVenue = await _venueService.CreateVenueAsync(venue);
                    TempData["SuccessMessage"] = "Venue created successfully!";
                    return RedirectToAction(nameof(Venues));
                }
                catch (ArgumentException ex)
                {
                    TempData["ErrorMessage"] = ex.Message;
                }
                catch (InvalidOperationException ex)
                {
                    TempData["ErrorMessage"] = ex.Message;
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "An error occurred while creating the venue. Please try again.";
                }
            }
            else
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                TempData["ErrorMessage"] = "Validation failed: " + string.Join(", ", errors);
            }

            return RedirectToAction(nameof(Venues));
        }

        /// <summary>
        /// Update an existing venue
        /// </summary>
        /// <param name="id"></param>
        /// <param name="venue"></param>
        /// <returns></returns>
        [HttpPost("admin/venues/edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditVenue(Guid id, Venue venue)
        {
            // For edit operation, set the Id from the route parameter
            venue.Id = id;

            // Handle empty capacity field
            if (string.IsNullOrEmpty(Request.Form["Capacity"]) || !int.TryParse(Request.Form["Capacity"], out int capacity))
            {
                ModelState.AddModelError("Capacity", "Capacity is required and must be a valid number.");
            }
            else
            {
                venue.Capacity = capacity;
            }

            // Clear any existing capacity and id errors and re-validate
            ModelState.Remove("Capacity");
            ModelState.Remove("Id");
            TryValidateModel(venue);

            if (ModelState.IsValid)
            {
                try
                {
                    var updatedVenue = await _venueService.UpdateVenueAsync(id, venue);
                    if (updatedVenue != null)
                    {
                        TempData["SuccessMessage"] = "Venue updated successfully!";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Venue not found.";
                    }
                }
                catch (ArgumentException ex)
                {
                    TempData["ErrorMessage"] = ex.Message;
                }
                catch (InvalidOperationException ex)
                {
                    TempData["ErrorMessage"] = ex.Message;
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "An error occurred while updating the venue. Please try again.";
                }
            }
            else
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                TempData["ErrorMessage"] = "Validation failed: " + string.Join(", ", errors);
            }

            return RedirectToAction(nameof(Venues));
        }

        /// <summary>
        /// Delete a venue
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("admin/venues/delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteVenue(Guid id)
        {
            try
            {
                var result = await _venueService.DeleteVenueAsync(id);
                if (result)
                {
                    TempData["SuccessMessage"] = "Venue deleted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Venue not found.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the venue. Please try again.";
            }

            return RedirectToAction(nameof(Venues));
        }


        [HttpGet("admin/settings")]
        public IActionResult Settings()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetUser(string id)
        {
            var userWithRole = await _adminService.GetUserWithRoleById(id);
            if (userWithRole == null)
            {
                return NotFound();
            }
            return Json(userWithRole);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrganizer(ApplicationUser model)
        {
            if (ModelState.IsValid)
            {
                var (success, errors) = await _adminService.CreateOrganizer(model);
                if (success)
                {
                    TempData["SuccessMessage"] = "Organizer created successfully!";
                    return RedirectToAction(nameof(Organizers), new { page = 1 });
                }

                foreach (var error in errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            var validationErrors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            if (validationErrors.Any())
            {
                TempData["ValidationErrors"] = validationErrors;
            }

            var organizers = await _adminService.GetUsersByRole("Organizer");
            var totalItems = organizers.Count();
            var totalPages = (int)Math.Ceiling((double)totalItems / 6);
            var paginatedOrganizers = organizers.Take(6).ToList();
            
            ViewBag.CurrentPage = 1;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = 6;
            ViewBag.TotalItems = totalItems;
            ViewBag.StartItem = 1;
            ViewBag.EndItem = Math.Min(6, totalItems);
            
            return View("Organizers", paginatedOrganizers);
        }

        [HttpGet]
        public async Task<IActionResult> GetOrganizerData(string id)
        {
            var user = await _adminService.GetUserById(id);
            if (user == null)
            {
                return NotFound();
            }
            return Json(user);
        }
    }
}
