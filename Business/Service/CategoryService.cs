using online_event_booking_system.Business.Interface;
using online_event_booking_system.Data.Entities;
using online_event_booking_system.Repository.Interface;

namespace online_event_booking_system.Business.Service
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _categoryRepository.GetAllCategoriesAsync();
        }

        public async Task<Category?> GetCategoryByIdAsync(Guid id)
        {
            return await _categoryRepository.GetCategoryByIdAsync(id);
        }

        public async Task<(bool Success, string Message, Category? Category)> CreateCategoryAsync(Category category)
        {
            try
            {
                // Validate category name uniqueness
                if (!await _categoryRepository.IsCategoryNameUniqueAsync(category.Name))
                {
                    return (false, "A category with this name already exists.", null);
                }

                var createdCategory = await _categoryRepository.CreateCategoryAsync(category);
                return (true, "Category created successfully.", createdCategory);
            }
            catch (Exception ex)
            {
                return (false, $"An error occurred while creating the category: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Message, Category? Category)> UpdateCategoryAsync(Category category)
        {
            try
            {
                // Check if category exists
                if (!await _categoryRepository.CategoryExistsAsync(category.Id))
                {
                    return (false, "Category not found.", null);
                }

                // Validate category name uniqueness (excluding current category)
                if (!await _categoryRepository.IsCategoryNameUniqueAsync(category.Name, category.Id))
                {
                    return (false, "A category with this name already exists.", null);
                }

                var updatedCategory = await _categoryRepository.UpdateCategoryAsync(category);
                return (true, "Category updated successfully.", updatedCategory);
            }
            catch (Exception ex)
            {
                return (false, $"An error occurred while updating the category: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Message)> DeleteCategoryAsync(Guid id)
        {
            try
            {
                // Check if category exists
                if (!await _categoryRepository.CategoryExistsAsync(id))
                {
                    return (false, "Category not found.");
                }

                var result = await _categoryRepository.DeleteCategoryAsync(id);
                if (result)
                {
                    return (true, "Category deleted successfully.");
                }
                else
                {
                    return (false, "Failed to delete category.");
                }
            }
            catch (Exception ex)
            {
                return (false, $"An error occurred while deleting the category: {ex.Message}");
            }
        }

        public async Task<bool> CategoryExistsAsync(Guid id)
        {
            return await _categoryRepository.CategoryExistsAsync(id);
        }

        public async Task<IEnumerable<Category>> GetActiveCategoriesAsync()
        {
            return await _categoryRepository.GetActiveCategoriesAsync();
        }

        public async Task<bool> IsCategoryNameUniqueAsync(string name, Guid? excludeId = null)
        {
            return await _categoryRepository.IsCategoryNameUniqueAsync(name, excludeId);
        }
    }
}
