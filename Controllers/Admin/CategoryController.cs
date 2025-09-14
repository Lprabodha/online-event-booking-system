using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using online_event_booking_system.Business.Interface;
using online_event_booking_system.Data.Entities;
using online_event_booking_system.Models;

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
                var viewModels = categories.Select(c => new CategoryViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    IsActive = c.IsActive,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    EventsCount = c.Events?.Count ?? 0
                }).ToList();
                
                return View("~/Views/Admin/Category/Index.cshtml", viewModels);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading categories: {ex.Message}";
                return View("~/Views/Admin/Category/Index.cshtml", new List<CategoryViewModel>());
            }
        }

        /// <summary>
        /// Display the form to create a new category
        /// </summary>
        /// <returns></returns>

        // GET: Admin/Category/Create
        [HttpGet("admin/categories/create")]
        public IActionResult Create()
        {
            return View("~/Views/Admin/Category/Create.cshtml", new CategoryCreateViewModel());
        }

        /// <summary>
        /// Handle the submission of the new category form
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>

        // POST: Admin/Category/Create
        [HttpPost("admin/categories/create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryCreateViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Check if category name is unique
                    var isUnique = await _categoryService.IsCategoryNameUniqueAsync(viewModel.Name);
                    if (!isUnique)
                    {
                        ModelState.AddModelError(nameof(viewModel.Name), "A category with this name already exists.");
                        return View("~/Views/Admin/Category/Create.cshtml", viewModel);
                    }

                    var category = new Category
                    {
                        Name = viewModel.Name,
                        Description = viewModel.Description,
                        IsActive = viewModel.IsActive
                    };

                    var (success, message, createdCategory) = await _categoryService.CreateCategoryAsync(category);
                    if (success)
                    {
                        TempData["SuccessMessage"] = message;
                        return Redirect("/admin/categories");
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
            
            return View("~/Views/Admin/Category/Create.cshtml", viewModel);
        }

        /// <summary>
        /// Display the form to edit an existing category
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>

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

                var viewModel = new CategoryEditViewModel
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description,
                    IsActive = category.IsActive,
                    CreatedAt = category.CreatedAt
                };

                return View("~/Views/Admin/Category/Edit.cshtml", viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading category: {ex.Message}";
                return Redirect("/admin/categories");
            }
        }

        /// <summary>
        /// Handle the submission of the edit category form
        /// </summary>
        /// <param name="id"></param>
        /// <param name="viewModel"></param>
        /// <returns></returns>

        // POST: Admin/Category/Edit/5
        [HttpPost("admin/categories/edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, CategoryEditViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Check if category name is unique (excluding current category)
                    var isUnique = await _categoryService.IsCategoryNameUniqueAsync(viewModel.Name, viewModel.Id);
                    if (!isUnique)
                    {
                        ModelState.AddModelError(nameof(viewModel.Name), "A category with this name already exists.");
                        return View("~/Views/Admin/Category/Edit.cshtml", viewModel);
                    }

                    var category = new Category
                    {
                        Id = viewModel.Id,
                        Name = viewModel.Name,
                        Description = viewModel.Description,
                        IsActive = viewModel.IsActive,
                        CreatedAt = viewModel.CreatedAt
                    };

                    var (success, message, updatedCategory) = await _categoryService.UpdateCategoryAsync(category);
                    if (success)
                    {
                        TempData["SuccessMessage"] = message;
                        return Redirect("/admin/categories");
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
            
            return View("~/Views/Admin/Category/Edit.cshtml", viewModel);
        }


        /// <summary>
        /// Handle the deletion of a category
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        
        // POST: Admin/Category/Delete/5
        [HttpPost("admin/categories/delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (id == Guid.Empty)
            {
                TempData["ErrorMessage"] = "Invalid category ID.";
                return Redirect("/admin/categories");
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

            return Redirect("/admin/categories");
        }

        /// <summary>
        /// Get statistics about categories
        /// </summary>
        /// <returns></returns>

        [HttpGet("admin/categories/statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            try
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                var activeCategories = await _categoryService.GetActiveCategoriesAsync();
                
                var statistics = new
                {
                    TotalCategories = categories.Count(),
                    ActiveCategories = activeCategories.Count(),
                    InactiveCategories = categories.Count() - activeCategories.Count(),
                    TotalEvents = categories.Sum(c => c.Events?.Count ?? 0)
                };

                return Json(statistics);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }
    }
}
