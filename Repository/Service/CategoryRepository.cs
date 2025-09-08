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

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            try
            {
                _logger.LogInformation("Retrieving all categories");
                var categories = await _context.Categories
                    .Include(c => c.Events)
                    .OrderBy(c => c.Name)
                    .ToListAsync();
                
                _logger.LogInformation("Successfully retrieved {Count} categories", categories.Count);
                return categories;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all categories");
                throw;
            }
        }

        public async Task<Category?> GetCategoryByIdAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Retrieving category with ID: {CategoryId}", id);
                var category = await _context.Categories
                    .Include(c => c.Events)
                    .ThenInclude(e => e.Venue)
                    .FirstOrDefaultAsync(c => c.Id == id);
                
                if (category == null)
                {
                    _logger.LogWarning("Category with ID {CategoryId} not found", id);
                }
                else
                {
                    _logger.LogInformation("Successfully retrieved category: {CategoryName}", category.Name);
                }
                
                return category;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving category with ID: {CategoryId}", id);
                throw;
            }
        }

        public async Task<Category> CreateCategoryAsync(Category category)
        {
            try
            {
                _logger.LogInformation("Creating new category: {CategoryName}", category.Name);
                
                category.CreatedAt = DateTime.UtcNow;
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Successfully created category with ID: {CategoryId}", category.Id);
                return category;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating category: {CategoryName}", category.Name);
                throw;
            }
        }

        public async Task<Category> UpdateCategoryAsync(Category category)
        {
            try
            {
                _logger.LogInformation("Updating category with ID: {CategoryId}, Name: {CategoryName}", category.Id, category.Name);
                
                category.UpdatedAt = DateTime.UtcNow;
                _context.Categories.Update(category);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Successfully updated category with ID: {CategoryId}", category.Id);
                return category;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating category with ID: {CategoryId}", category.Id);
                throw;
            }
        }

        public async Task<bool> DeleteCategoryAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Attempting to delete category with ID: {CategoryId}", id);
                
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
                    _logger.LogInformation("Category {CategoryName} has associated events, performing soft delete", category.Name);
                    category.IsActive = false;
                    category.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Successfully performed soft delete on category with ID: {CategoryId}", id);
                    return true;
                }

                // Hard delete if no associated events
                _logger.LogInformation("Category {CategoryName} has no associated events, performing hard delete", category.Name);
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully performed hard delete on category with ID: {CategoryId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting category with ID: {CategoryId}", id);
                throw;
            }
        }

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

        public async Task<IEnumerable<Category>> GetActiveCategoriesAsync()
        {
            try
            {
                _logger.LogInformation("Retrieving active categories");
                var categories = await _context.Categories
                    .Where(c => c.IsActive)
                    .Include(c => c.Events)
                    .OrderBy(c => c.Name)
                    .ToListAsync();
                
                _logger.LogInformation("Successfully retrieved {Count} active categories", categories.Count);
                return categories;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving active categories");
                throw;
            }
        }

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
