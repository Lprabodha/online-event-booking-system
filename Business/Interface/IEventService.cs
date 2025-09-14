using online_event_booking_system.Data.Entities;
using online_event_booking_system.Models.View_Models;

namespace online_event_booking_system.Business.Interface
{
    public interface IEventService
    {
        Task<Event> CreateEventAsync(CreateEventViewModel model, string organizerId);
        Task<Event?> GetEventByIdAsync(Guid id);
        Task<List<Event>> GetEventsByOrganizerAsync(string organizerId);
        Task<(List<Event> Events, int TotalCount, int TotalPages)> GetEventsByOrganizerWithPaginationAsync(string organizerId, int page = 1, int pageSize = 10);
        Task<Event> UpdateEventAsync(Guid id, CreateEventViewModel model, string organizerId);
        Task<bool> DeleteEventAsync(Guid id, string organizerId);
        Task<List<Event>> GetAllEventsAsync();
        Task<List<Event>> GetPublishedEventsAsync();
        Task<bool> PublishEventAsync(Guid id, string organizerId);
        Task<bool> UnpublishEventAsync(Guid id, string organizerId);
        Task<bool> CancelEventAsync(Guid id, string organizerId);
        Task<bool> UpdateEventStatusAsync(Guid id, string organizerId, string status);
        Task<List<Category>> GetCategoriesAsync();
        Task<List<Venue>> GetVenuesAsync();
        Task<CreateEventViewModel> GetCreateEventViewModelAsync();
    }
}
