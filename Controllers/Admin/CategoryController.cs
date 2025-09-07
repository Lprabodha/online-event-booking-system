using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using online_event_booking_system.Business.Interface;
using online_event_booking_system.Data.Entities;

namespace online_event_booking_system.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // GET: Admin/Category
        [HttpGet("admin/categories")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                return View(categories);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading categories: {ex.Message}";
                return View(new List<Category>());
            }
        }

        // GET: Admin/Category/Details/5
        [HttpGet("admin/categories/details/{id}")]
        public async Task<IActionResult> Details(Guid id)
        {
            if (id == Guid.Empty)
            {
                return NotFound();
            }

            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(id);
                if (category == null)
                {
                    return NotFound();
                }
                return View(category);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading category details: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Admin/Category/Create
        [HttpGet("admin/categories/create")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Category/Create
        [HttpPost("admin/categories/create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var (success, message, createdCategory) = await _categoryService.CreateCategoryAsync(category);
                    if (success)
                    {
                        TempData["SuccessMessage"] = message;
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, message);
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                }
            }
            return View(category);
        }

        // GET: Admin/Category/Edit/5
        [HttpGet("admin/categories/edit/{id}")]
        public async Task<IActionResult> Edit(Guid id)
        {
            if (id == Guid.Empty)
            {
                return NotFound();
            }

            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(id);
                if (category == null)
                {
                    return NotFound();
                }
                return View(category);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading category: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Admin/Category/Edit/5
        [HttpPost("admin/categories/edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, Category category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var (success, message, updatedCategory) = await _categoryService.UpdateCategoryAsync(category);
                    if (success)
                    {
                        TempData["SuccessMessage"] = message;
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, message);
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                }
            }
            return View(category);
        }

        // GET: Admin/Category/Delete/5
        [HttpGet("admin/categories/delete/{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (id == Guid.Empty)
            {
                return NotFound();
            }

            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(id);
                if (category == null)
                {
                    return NotFound();
                }
                return View(category);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading category: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Admin/Category/Delete/5
        [HttpPost("admin/categories/delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (id == Guid.Empty)
            {
                return NotFound();
            }

            try
            {
                var (success, message) = await _categoryService.DeleteCategoryAsync(id);
                if (success)
                {
                    TempData["SuccessMessage"] = message;
                }
                else
                {
                    TempData["ErrorMessage"] = message;
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while deleting category: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // AJAX endpoint to check if category name is unique
        [HttpGet("admin/categories/check-name")]
        public async Task<IActionResult> CheckNameUnique(string name, Guid? excludeId = null)
        {
            try
            {
                var isUnique = await _categoryService.IsCategoryNameUniqueAsync(name, excludeId);
                return Json(new { isUnique });
            }
            catch
            {
                return Json(new { isUnique = false });
            }
        }
    }
}
