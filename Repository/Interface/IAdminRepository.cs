using online_event_booking_system.Data.Entities;

namespace online_event_booking_system.Repository.Interface
{
    public interface IAdminRepository
    {
        Task<IEnumerable<ApplicationUser>> GetAllUsers();
        Task<ApplicationUser> GetUserById(string id);
        Task<bool> CreateUser(ApplicationUser user, string password, string role);
        Task<bool> UpdateUser(ApplicationUser user);
        Task<bool> SoftDeleteUser(string id);
    }
}
