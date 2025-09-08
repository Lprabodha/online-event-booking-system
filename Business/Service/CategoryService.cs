using Microsoft.Extensions.Logging;
using online_event_booking_system.Business.Interface;
using online_event_booking_system.Data.Entities;
using online_event_booking_system.Repository.Interface;

namespace online_event_booking_system.Business.Service
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(ICategoryRepository categoryRepository, ILogger<CategoryService> logger)
        {
            _categoryRepository = categoryRepository;
            _logger = logger;
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
                _logger.LogInformation("Creating new category: {CategoryName}", category.Name);

                // Business validation
                if (string.IsNullOrWhiteSpace(category.Name))
                {
                    _logger.LogWarning("Category creation failed: Name is required");
                    return (false, "Category name is required.", null);
                }

                if (category.Name.Length > 100)
                {
                    _logger.LogWarning("Category creation failed: Name too long");
                    return (false, "Category name cannot exceed 100 characters.", null);
                }

                if (!string.IsNullOrEmpty(category.Description) && category.Description.Length > 500)
                {
                    _logger.LogWarning("Category creation failed: Description too long");
                    return (false, "Category description cannot exceed 500 characters.", null);
                }

                // Validate category name uniqueness
                if (!await _categoryRepository.IsCategoryNameUniqueAsync(category.Name))
                {
                    _logger.LogWarning("Category creation failed: Name already exists - {CategoryName}", category.Name);
                    return (false, "A category with this name already exists.", null);
                }

                var createdCategory = await _categoryRepository.CreateCategoryAsync(category);
                _logger.LogInformation("Successfully created category with ID: {CategoryId}", createdCategory.Id);
                return (true, "Category created successfully.", createdCategory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating category: {CategoryName}", category.Name);
                return (false, $"An error occurred while creating the category: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Message, Category? Category)> UpdateCategoryAsync(Category category)
        {
            try
            {
                _logger.LogInformation("Updating category with ID: {CategoryId}, Name: {CategoryName}", category.Id, category.Name);

                // Business validation
                if (string.IsNullOrWhiteSpace(category.Name))
                {
                    _logger.LogWarning("Category update failed: Name is required");
                    return (false, "Category name is required.", null);
                }

                if (category.Name.Length > 100)
                {
                    _logger.LogWarning("Category update failed: Name too long");
                    return (false, "Category name cannot exceed 100 characters.", null);
                }

                if (!string.IsNullOrEmpty(category.Description) && category.Description.Length > 500)
                {
                    _logger.LogWarning("Category update failed: Description too long");
                    return (false, "Category description cannot exceed 500 characters.", null);
                }

                // Check if category exists
                if (!await _categoryRepository.CategoryExistsAsync(category.Id))
                {
                    _logger.LogWarning("Category update failed: Category not found with ID: {CategoryId}", category.Id);
                    return (false, "Category not found.", null);
                }

                // Validate category name uniqueness (excluding current category)
                if (!await _categoryRepository.IsCategoryNameUniqueAsync(category.Name, category.Id))
                {
                    _logger.LogWarning("Category update failed: Name already exists - {CategoryName}", category.Name);
                    return (false, "A category with this name already exists.", null);
                }

                var updatedCategory = await _categoryRepository.UpdateCategoryAsync(category);
                _logger.LogInformation("Successfully updated category with ID: {CategoryId}", category.Id);
                return (true, "Category updated successfully.", updatedCategory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating category with ID: {CategoryId}", category.Id);
                return (false, $"An error occurred while updating the category: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Message)> DeleteCategoryAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Attempting to delete category with ID: {CategoryId}", id);

                // Check if category exists
                if (!await _categoryRepository.CategoryExistsAsync(id))
                {
                    _logger.LogWarning("Category deletion failed: Category not found with ID: {CategoryId}", id);
                    return (false, "Category not found.");
                }

                // Get category details for logging
                var category = await _categoryRepository.GetCategoryByIdAsync(id);
                var categoryName = category?.Name ?? "Unknown";

                var result = await _categoryRepository.DeleteCategoryAsync(id);
                if (result)
                {
                    var message = category?.Events?.Count > 0 
                        ? $"Category '{categoryName}' has been marked as inactive due to associated events."
                        : $"Category '{categoryName}' has been deleted successfully.";
                    
                    _logger.LogInformation("Successfully deleted category with ID: {CategoryId}", id);
                    return (true, message);
                }
                else
                {
                    _logger.LogError("Failed to delete category with ID: {CategoryId}", id);
                    return (false, "Failed to delete category.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting category with ID: {CategoryId}", id);
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
