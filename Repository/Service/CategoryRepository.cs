using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using online_event_booking_system.Data;
using online_event_booking_system.Data.Entities;
using online_event_booking_system.Repository.Interface;

namespace online_event_booking_system.Repository.Service
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CategoryRepository> _logger;

        public CategoryRepository(ApplicationDbContext context, ILogger<CategoryRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all categories in the system
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            try
            {
                var categories = await _context.Categories
                    .Include(c => c.Events)
                    .OrderBy(c => c.Name)
                    .ToListAsync();
                
                return categories;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all categories");
                throw;
            }
        }

        /// <summary>
        /// Get category by its unique identifier
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Category?> GetCategoryByIdAsync(Guid id)
        {
            try
            {
                var category = await _context.Categories
                    .Include(c => c.Events)
                    .ThenInclude(e => e.Venue)
                    .FirstOrDefaultAsync(c => c.Id == id);
                
                return category;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving category with ID: {CategoryId}", id);
                throw;
            }
        }

        /// <summary>
        /// Create a new category
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public async Task<Category> CreateCategoryAsync(Category category)
        {
            try
            {
                
                category.CreatedAt = DateTime.UtcNow;
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
                
                return category;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating category: {CategoryName}", category.Name);
                throw;
            }
        }

        /// <summary>
        /// Update an existing category
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public async Task<Category> UpdateCategoryAsync(Category category)
        {
            try
            {
                
                category.UpdatedAt = DateTime.UtcNow;
                _context.Categories.Update(category);
                await _context.SaveChangesAsync();
                
                return category;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating category with ID: {CategoryId}", category.Id);
                throw;
            }
        }

        /// <summary>
        /// Delete a category by its ID. If the category has associated events, it will be marked as inactive instead of being deleted.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<bool> DeleteCategoryAsync(Guid id)
        {
            try
            {
                
                var category = await _context.Categories.FindAsync(id);
                if (category == null)
                {
                    _logger.LogWarning("Category with ID {CategoryId} not found for deletion", id);
                    return false;
                }

                // Check if category has associated events
                var hasEvents = await _context.Events.AnyAsync(e => e.CategoryId == id);
                if (hasEvents)
                {
                    // Soft delete - mark as inactive instead of hard delete
                    category.IsActive = false;
                    category.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    return true;
                }

                // Hard delete if no associated events
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting category with ID: {CategoryId}", id);
                throw;
            }
        }

        /// <summary>
        /// Check if a category exists by its unique identifier
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<bool> CategoryExistsAsync(Guid id)
        {
            try
            {
                _logger.LogDebug("Checking if category exists with ID: {CategoryId}", id);
                var exists = await _context.Categories.AnyAsync(c => c.Id == id);
                _logger.LogDebug("Category with ID {CategoryId} exists: {Exists}", id, exists);
                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking if category exists with ID: {CategoryId}", id);
                throw;
            }
        }

        /// <summary>
        /// Get all active categories
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<Category>> GetActiveCategoriesAsync()
        {
            try
            {
                var categories = await _context.Categories
                    .Where(c => c.IsActive)
                    .Include(c => c.Events)
                    .OrderBy(c => c.Name)
                    .ToListAsync();
                
                return categories;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving active categories");
                throw;
            }
        }

        /// <summary>
        /// Check if a category name is unique, optionally excluding a specific category ID (useful for updates)
        /// </summary>
        /// <param name="name"></param>
        /// <param name="excludeId"></param>
        /// <returns></returns>
        public async Task<bool> IsCategoryNameUniqueAsync(string name, Guid? excludeId = null)
        {
            try
            {
                _logger.LogDebug("Checking if category name '{CategoryName}' is unique (excluding ID: {ExcludeId})", name, excludeId);
                
                var query = _context.Categories.Where(c => c.Name.ToLower() == name.ToLower());
                
                if (excludeId.HasValue)
                {
                    query = query.Where(c => c.Id != excludeId.Value);
                }

                var isUnique = !await query.AnyAsync();
                _logger.LogDebug("Category name '{CategoryName}' is unique: {IsUnique}", name, isUnique);
                return isUnique;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking if category name '{CategoryName}' is unique", name);
                throw;
            }
        }
    }
}
