using Microsoft.AspNetCore.Identity;
using online_event_booking_system.Data.Entities;
using online_event_booking_system.Models.View_Models;

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
        Task<IEnumerable<UserWithRoleViewModel>> GetAllUsersWithRoles();
        Task<UserWithRoleViewModel> GetUserWithRoleById(string id);
        Task<List<UserWithRoleViewModel>> GetUsersByRole(string roleName);
        Task<(bool success, IEnumerable<IdentityError>? errors)> CreateOrganizer(ApplicationUser user);
    }
}
