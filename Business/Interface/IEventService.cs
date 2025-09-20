using online_event_booking_system.Data.Entities;
using online_event_booking_system.Models.View_Models;

namespace online_event_booking_system.Business.Interface
{
    public interface IEventService
    {
        /// <summary>
        /// Create a new event
        /// </summary>
        /// <param name="model"></param>
        /// <param name="organizerId"></param>
        /// <returns></returns>
        Task<Event> CreateEventAsync(CreateEventViewModel model, string organizerId);
        /// <summary>
        /// Get event by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Event?> GetEventByIdAsync(Guid id);
        /// <summary>
        /// Get events by organizer
        /// </summary>
        /// <param name="organizerId"></param>
        /// <returns></returns>
        Task<List<Event>> GetEventsByOrganizerAsync(string organizerId);
        /// <summary>
        /// Get events by organizer with pagination
        /// </summary>
        /// <param name="organizerId"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        Task<(List<Event> Events, int TotalCount, int TotalPages)> GetEventsByOrganizerWithPaginationAsync(string organizerId, int page = 1, int pageSize = 10);
        /// <summary>
        /// Update an existing event
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <param name="organizerId"></param>
        /// <returns></returns>
        Task<Event> UpdateEventAsync(Guid id, CreateEventViewModel model, string organizerId);
        /// <summary>
        /// Delete an event
        /// </summary>
        /// <param name="id"></param>
        /// <param name="organizerId"></param>
        /// <returns></returns>
        Task<bool> DeleteEventAsync(Guid id, string organizerId);
        /// <summary>
        /// Get all events
        /// </summary>
        /// <returns></returns>
        Task<List<Event>> GetAllEventsAsync();
        /// <summary>
        /// Get published events
        /// </summary>
        /// <returns></returns>
        Task<List<Event>> GetPublishedEventsAsync();
        /// <summary>
        /// Publish an event
        /// </summary>
        /// <param name="id"></param>
        /// <param name="organizerId"></param>
        /// <returns></returns>
        Task<bool> PublishEventAsync(Guid id, string organizerId);
        /// <summary>
        /// Unpublish an event
        /// </summary>
        /// <param name="id"></param>
        /// <param name="organizerId"></param>
        /// <returns></returns>
        Task<bool> UnpublishEventAsync(Guid id, string organizerId);
        /// <summary>
        /// Cancel an event
        /// </summary>
        /// <param name="id"></param>
        /// <param name="organizerId"></param>
        /// <returns></returns>
        Task<bool> CancelEventAsync(Guid id, string organizerId);
        /// <summary>
        /// Update Event Status
        /// </summary>
        /// <param name="id"></param>
        /// <param name="organizerId"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        Task<bool> UpdateEventStatusAsync(Guid id, string organizerId, string status);
        /// <summary>
        /// Get all categories  
        /// </summary>
        /// <returns></returns>
        Task<List<Category>> GetCategoriesAsync();
        /// <summary>
        /// Get all venues
        /// </summary>
        /// <returns></returns>
        Task<List<Venue>> GetVenuesAsync();
        /// <summary>
        /// Get CreateEventViewModel with categories and venues
        /// </summary>
        /// <returns></returns>
        Task<CreateEventViewModel> GetCreateEventViewModelAsync();
        /// <summary>
        /// Get upcoming events
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        Task<List<Event>> GetUpcomingEventsAsync(int count = 6);
        /// <summary>
        /// Get latest events
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        Task<List<Event>> GetLatestEventsAsync(int count = 4);
        /// <summary>
        /// Get related events based on category
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        Task<List<Event>> GetRelatedEventsAsync(Guid eventId, int count = 3);
        /// <summary>
        /// Get events happening this week
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        Task<List<Event>> GetEventsThisWeekAsync(int count = 6);
        /// <summary>
        /// Get events happening next week
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        Task<List<Event>> GetEventsNextWeekAsync(int count = 6);
        /// <summary>
        /// Get event analytics
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="restrictToOrganizerId"></param>
        /// <returns></returns>
        Task<EventAnalyticsViewModel?> GetEventAnalyticsAsync(Guid eventId, string? restrictToOrganizerId = null);
    }
}
