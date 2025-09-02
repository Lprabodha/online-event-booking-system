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
            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                if (await _roleManager.RoleExistsAsync(role))
                {
                    await _userManager.AddToRoleAsync(user, role);
                    return true;
                }
            }
            return false;
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
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }
    }
}
