using Microsoft.AspNetCore.Identity;
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
        Task<string> GetUserRole(string id);
        Task<List<ApplicationUser>> GetUsersInRoleAsync(string roleName);
        Task<IdentityResult> CreateOrganizerAsync(ApplicationUser user, string password);
        Task<IdentityResult> AddToRoleAsync(ApplicationUser user, string role);
    }
}
