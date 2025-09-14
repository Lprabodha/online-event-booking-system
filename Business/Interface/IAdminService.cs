using Microsoft.AspNetCore.Identity;
using online_event_booking_system.Data.Entities;
using online_event_booking_system.Models.View_Models;

namespace online_event_booking_system.Business.Interface
{
    public interface IAdminService
    {
        /// <summary>
        /// Get all users in the system
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<ApplicationUser>> GetAllUsers();

        /// <summary>
        /// Get user by their unique identifier
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<ApplicationUser> GetUserById(string id);
        /// <summary>
        /// Create a new user with the specified role
        /// </summary>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        Task<(bool Succeeded, IEnumerable<IdentityError> Errors)> CreateUser(ApplicationUser user, string password, string role);
        /// <summary>
        /// Update an existing user's details
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        Task<bool> UpdateUser(ApplicationUser user);
        /// <summary>
        /// Soft delete a user by setting their IsActive status to false
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<bool> SoftDeleteUser(string id);
        /// <summary>
        /// Toggle a user's active status
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<bool> ToggleUserStatus(string id);
        /// <summary>
        /// Get all users along with their assigned roles
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<UserWithRoleViewModel>> GetAllUsersWithRoles();
        /// <summary>
        /// Get a user along with their role by user ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<UserWithRoleViewModel?> GetUserWithRoleById(string id);
        /// <summary>
        /// Get users by their assigned role
        /// </summary>
        /// <param name="roleName"></param>
        /// <returns></returns>
        Task<List<UserWithRoleViewModel>> GetUsersByRole(string roleName);
        /// <summary>
        /// Create a new organizer user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        Task<(bool success, IEnumerable<IdentityError>? errors)> CreateOrganizer(ApplicationUser user);
        
        /// <summary>
        /// Get all events with related data for admin management
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<Event>> GetAllEventsAsync();
    }
}
