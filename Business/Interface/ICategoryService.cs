using online_event_booking_system.Data.Entities;

namespace online_event_booking_system.Business.Interface
{
    public interface ICategoryService
    {
        /// <summary>
        /// Get all categories in the system
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<Category>> GetAllCategoriesAsync();
        /// <summary>
        /// Get category by its unique identifier
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Category?> GetCategoryByIdAsync(Guid id);
        /// <summary>
        /// Create a new category
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        Task<(bool Success, string Message, Category? Category)> CreateCategoryAsync(Category category);
        /// <summary>
        /// Update an existing category
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        Task<(bool Success, string Message, Category? Category)> UpdateCategoryAsync(Category category);
        /// <summary>
        /// Soft delete a category by setting its IsActive status to false
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<(bool Success, string Message)> DeleteCategoryAsync(Guid id);
        /// <summary>
        /// Check if a category exists by its unique identifier
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<bool> CategoryExistsAsync(Guid id);
        /// <summary>
        /// Get all active categories
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<Category>> GetActiveCategoriesAsync();
        /// <summary>
        /// Check if a category name is unique, optionally excluding a specific category by ID
        /// </summary>
        /// <param name="name"></param>
        /// <param name="excludeId"></param>
        /// <returns></returns>
        Task<bool> IsCategoryNameUniqueAsync(string name, Guid? excludeId = null);
    }
}
