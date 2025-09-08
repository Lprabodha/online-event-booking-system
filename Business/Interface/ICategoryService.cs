using online_event_booking_system.Data.Entities;

namespace online_event_booking_system.Business.Interface
{
    public interface ICategoryService
    {
        Task<IEnumerable<Category>> GetAllCategoriesAsync();
        Task<Category?> GetCategoryByIdAsync(Guid id);
        Task<(bool Success, string Message, Category? Category)> CreateCategoryAsync(Category category);
        Task<(bool Success, string Message, Category? Category)> UpdateCategoryAsync(Category category);
        Task<(bool Success, string Message)> DeleteCategoryAsync(Guid id);
        Task<bool> CategoryExistsAsync(Guid id);
        Task<IEnumerable<Category>> GetActiveCategoriesAsync();
        Task<bool> IsCategoryNameUniqueAsync(string name, Guid? excludeId = null);
    }
}
