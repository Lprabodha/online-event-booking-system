using online_event_booking_system.Data.Entities;

namespace online_event_booking_system.Repository.Interface
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> GetAllCategoriesAsync();
        Task<Category?> GetCategoryByIdAsync(Guid id);
        Task<Category> CreateCategoryAsync(Category category);
        Task<Category> UpdateCategoryAsync(Category category);
        Task<bool> DeleteCategoryAsync(Guid id);
        Task<bool> CategoryExistsAsync(Guid id);
        Task<IEnumerable<Category>> GetActiveCategoriesAsync();
        Task<bool> IsCategoryNameUniqueAsync(string name, Guid? excludeId = null);
    }
}
