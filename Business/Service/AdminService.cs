using Microsoft.AspNetCore.Identity;
using online_event_booking_system.Business.Interface;
using online_event_booking_system.Data.Entities;
using online_event_booking_system.Helper;
using online_event_booking_system.Models.View_Models;
using online_event_booking_system.Repository.Interface;
using online_event_booking_system.Services;
using System.Security.Cryptography;
using System.Text;

namespace online_event_booking_system.Business.Service
{
    /// <summary>
    /// Service class for admin-related operations, implementing IAdminService interface.
    /// </summary>
    public class AdminService : IAdminService
    {
        /// <summary>
        /// Repository for admin-related data operations.
        /// </summary>
        private readonly IAdminRepository _adminRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;

        public AdminService(IAdminRepository adminRepository, UserManager<ApplicationUser> userManager, IEmailService emailService)
        {
            _adminRepository = adminRepository;
            _userManager = userManager;
            _emailService = emailService;
        }

        /// <summary>
        /// Create a new user with the specified role.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <param name="role"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Get all users in the system.
        /// </summary>
        /// <returns></returns>

        public async Task<IEnumerable<ApplicationUser>> GetAllUsers()
        {
            return await _adminRepository.GetAllUsers();
        }

        /// <summary>
        /// Get all users along with their assigned roles.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Get user by their unique identifier.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ApplicationUser> GetUserById(string id)
        {
            return await _adminRepository.GetUserById(id);
        }

        /// <summary>
        /// Soft delete a user by setting their IsActive status to false.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<bool> SoftDeleteUser(string id)
        {
            return await _adminRepository.SoftDeleteUser(id);
        }

        /// <summary>
        /// Toggle a user's active status.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<bool> ToggleUserStatus(string id)
        {
            var user = await _adminRepository.GetUserById(id);
            if (user == null)
            {
                return false;
            }
            user.IsActive = !user.IsActive;
            return await _adminRepository.UpdateUser(user);
        }

        /// <summary>
        /// Update an existing user's details.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<bool> UpdateUser(ApplicationUser user)
        {
            return await _adminRepository.UpdateUser(user);
        }

        /// <summary>
        /// Get a user along with their role by user ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<UserWithRoleViewModel> GetUserWithRoleById(string id)
        {
            var user = await _adminRepository.GetUserById(id);
            if (user == null)
            {
                return null;
            }
            var role = await _adminRepository.GetUserRole(id);
            return new UserWithRoleViewModel
            {
                User = user,
                Role = role
            };
        }

        /// <summary>
        /// Get users by their assigned role.
        /// </summary>
        /// <param name="roleName"></param>
        /// <returns></returns>
        public async Task<List<UserWithRoleViewModel>> GetUsersByRole(string roleName)
        {
            var usersInRole = await _adminRepository.GetUsersInRoleAsync(roleName);
            var usersWithRoles = new List<UserWithRoleViewModel>();

            foreach (var user in usersInRole)
            {
                var userWithRole = new UserWithRoleViewModel
                {
                    User = user,
                    Role = roleName
                };
                usersWithRoles.Add(userWithRole);
            }
            return usersWithRoles;
        }

        /// <summary>
        /// Create a new event organizer with a generated password and send credentials via email.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<(bool success, IEnumerable<IdentityError>? errors)> CreateOrganizer(ApplicationUser model)
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(model.Email))
            {
                return (false, new List<IdentityError> { new IdentityError { Description = "Email is required." } });
            }

            if (string.IsNullOrWhiteSpace(model.FullName))
            {
                return (false, new List<IdentityError> { new IdentityError { Description = "Full Name is required." } });
            }

            if (string.IsNullOrWhiteSpace(model.ContactNumber))
            {
                return (false, new List<IdentityError> { new IdentityError { Description = "Contact Number is required." } });
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                ContactNumber = model.ContactNumber,
                NIC = model.NIC,
                Address = model.Address,
                OrganizationName = model.OrganizationName,
                IsActive = true,
            };

            var password = GenerateRandomPassword();

            var result = await _adminRepository.CreateOrganizerAsync(user, password);
            if (!result.Succeeded)
            {
                return (false, result.Errors);
            }

            var roleResult = await _adminRepository.AddToRoleAsync(user, "Organizer");
            if (!roleResult.Succeeded)
            {
                return (false, roleResult.Errors);
            }

            Task.Run(async () =>
            {
                var emailSubject = "Your New Event Organizer Account Credentials";
                var emailBody = EmailTemplates.GetOrganizerAccountCreationTemplate(
                    user.FullName,
                    user.Email,
                    password
                );

                await _emailService.SendEmailAsync(user.Email, emailSubject, emailBody);
            });

            return (true, null);
        }

        /// <summary>
        /// Generate a secure random password.
        /// </summary>
        /// <returns></returns>
        private string GenerateRandomPassword()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()_+-=[]{}|;:,.<>/?";
            var sb = new StringBuilder();
            var random = RandomNumberGenerator.Create();
            var data = new byte[20];
            random.GetBytes(data);

            foreach (var b in data)
            {
                sb.Append(chars[b % chars.Length]);
            }

            return sb.ToString();
        }
    }
}
