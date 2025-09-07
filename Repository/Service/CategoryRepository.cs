using Microsoft.EntityFrameworkCore;
using online_event_booking_system.Data;
using online_event_booking_system.Data.Entities;
using online_event_booking_system.Repository.Interface;

namespace online_event_booking_system.Repository.Service
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ApplicationDbContext _context;

        public CategoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _context.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<Category?> GetCategoryByIdAsync(Guid id)
        {
            return await _context.Categories
                .Include(c => c.Events)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Category> CreateCategoryAsync(Category category)
        {
            category.CreatedAt = DateTime.UtcNow;
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<Category> UpdateCategoryAsync(Category category)
        {
            category.UpdatedAt = DateTime.UtcNow;
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<bool> DeleteCategoryAsync(Guid id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return false;

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

        public async Task<bool> CategoryExistsAsync(Guid id)
        {
            return await _context.Categories.AnyAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Category>> GetActiveCategoriesAsync()
        {
            return await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<bool> IsCategoryNameUniqueAsync(string name, Guid? excludeId = null)
        {
            var query = _context.Categories.Where(c => c.Name.ToLower() == name.ToLower());
            
            if (excludeId.HasValue)
            {
                query = query.Where(c => c.Id != excludeId.Value);
            }

            return !await query.AnyAsync();
        }
    }
}
