using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using online_event_booking_system.Data.Entities;
using online_event_booking_system.Repository.Interface;

namespace online_event_booking_system.Repository.Service
{
    public class AdminRepository : IAdminRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminRepository(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<bool> CreateUser(ApplicationUser user, string password, string role)
        {
            try
            {
                // Check if user already exists
                var existingUser = await _userManager.FindByEmailAsync(user.Email);
                if (existingUser != null)
                {
                    return false;
                }

                var result = await _userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(role) && await _roleManager.RoleExistsAsync(role))
                    {
                        await _userManager.AddToRoleAsync(user, role);
                    }
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IEnumerable<ApplicationUser>> GetAllUsers()
        {
            return await _userManager.Users.Where(u => u.IsActive == true).ToListAsync();
        }

        public async Task<ApplicationUser> GetUserById(string id)
        {
            return await _userManager.FindByIdAsync(id);
        }

        public async Task<bool> SoftDeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return false;
            }
            user.IsActive = false;

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> UpdateUser(ApplicationUser user)
        {
            // Get the existing user from the database to avoid tracking conflicts
            var existingUser = await _userManager.FindByIdAsync(user.Id);
            if (existingUser == null)
            {
                return false;
            }

            // Update the properties of the existing user
            existingUser.FullName = user.FullName;
            existingUser.UserName = user.UserName;
            existingUser.Email = user.Email;
            existingUser.ContactNumber = user.ContactNumber;
            existingUser.NIC = user.NIC;
            existingUser.Address = user.Address;
            existingUser.OrganizationName = user.OrganizationName;
            existingUser.IsActive = user.IsActive;

            var result = await _userManager.UpdateAsync(existingUser);
            return result.Succeeded;
        }
    }
}
