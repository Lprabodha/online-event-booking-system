using Microsoft.AspNetCore.Identity;
using online_event_booking_system.Business.Interface;
using online_event_booking_system.Data.Entities;
using online_event_booking_system.Models.View_Models;
using online_event_booking_system.Repository.Interface;

namespace online_event_booking_system.Business.Service
{
    public class AdminService : IAdminService
    {
        private readonly IAdminRepository _adminRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminService(IAdminRepository adminRepository, UserManager<ApplicationUser> userManager)
        {
            _adminRepository = adminRepository;
            _userManager = userManager;
        }

        public async Task<(bool Succeeded, IEnumerable<IdentityError> Errors)> CreateUser(ApplicationUser user, string password, string role)
        {
            try
            {
                var created = await _adminRepository.CreateUser(user, password, role);
                if (created)
                {
                    return (true, new List<IdentityError>());
                }
                else
                {
                    return (false, new List<IdentityError> 
                    { 
                        new IdentityError { Description = "Failed to create user. Please check the provided information." } 
                    });
                }
            }
            catch (Exception ex)
            {
                return (false, new List<IdentityError> 
                { 
                    new IdentityError { Description = $"An error occurred: {ex.Message}" } 
                });
            }
        }

        public async Task<IEnumerable<ApplicationUser>> GetAllUsers()
        {
            return await _adminRepository.GetAllUsers();
        }

        public async Task<IEnumerable<UserWithRoleViewModel>> GetAllUsersWithRoles()
        {
            var users = await _adminRepository.GetAllUsers();
            var usersWithRoles = new List<UserWithRoleViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                usersWithRoles.Add(new UserWithRoleViewModel
                {
                    User = user,
                    Role = roles.FirstOrDefault()
                });
            }

            return usersWithRoles;
        }

        public async Task<ApplicationUser> GetUserById(string id)
        {
            return await _adminRepository.GetUserById(id);
        }

        public async Task<bool> SoftDeleteUser(string id)
        {
            return await _adminRepository.SoftDeleteUser(id);
        }

        //public async Task<bool> ToggleUserStatus(string id)
        //{
        //    var user = await _adminRepository.GetUserById(id);
        //    if (user == null)
        //    {
        //        return false;
        //    }
        //    user.IsActive = !user.IsActive;
        //    return await _adminRepository.UpdateUser(user);
        //}

        public async Task<bool> UpdateUser(ApplicationUser user)
        {
            return await _adminRepository.UpdateUser(user);
        }
    }
}
