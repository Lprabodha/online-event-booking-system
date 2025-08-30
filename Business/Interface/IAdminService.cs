using Microsoft.AspNetCore.Identity;
using online_event_booking_system.Data.Entities;

namespace online_event_booking_system.Business.Interface
{
    public interface IAdminService
    {
        Task<IEnumerable<ApplicationUser>> GetAllUsers();
        Task<ApplicationUser> GetUserById(string id);
        Task<(bool Succeeded, IEnumerable<IdentityError> Errors)> CreateUser(ApplicationUser user, string password, string role);
        Task<bool> UpdateUser(ApplicationUser user);
        Task<bool> SoftDeleteUser(string id);
        Task<bool> ToggleUserStatus(string id);
    }
}
