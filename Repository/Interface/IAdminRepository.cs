using Microsoft.AspNetCore.Identity;
using online_event_booking_system.Data.Entities;

namespace online_event_booking_system.Repository.Interface
{
    public interface IAdminRepository
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
        Task<bool> CreateUser(ApplicationUser user, string password, string role);
        /// <summary>
        /// Update an existing user's details
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        Task<bool> UpdateUser(ApplicationUser user);
        /// <summary>
        /// Soft delete a user by marking them as inactive
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<bool> SoftDeleteUser(string id);
        /// <summary>
        /// Get the role of a user by their unique identifier
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<string> GetUserRole(string id);
        /// <summary>
        /// Get all users assigned to a specific role
        /// </summary>
        /// <param name="roleName"></param>
        /// <returns></returns>
        Task<List<ApplicationUser>> GetUsersInRoleAsync(string roleName);
        /// <summary>
        /// Create a new organizer user
        /// </summary>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        Task<IdentityResult> CreateOrganizerAsync(ApplicationUser user, string password);
        /// <summary>
        /// Assign a role to a user
        /// </summary>
        /// <param name="user"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        Task<IdentityResult> AddToRoleAsync(ApplicationUser user, string role);
        /// <summary>
        /// Get all events in the system
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<Event>> GetAllEventsAsync();
    }
}
