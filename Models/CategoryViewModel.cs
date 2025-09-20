using System.ComponentModel.DataAnnotations;

namespace online_event_booking_system.Models
{
    /// <summary>
    /// ViewModel for representing a category in the event booking system.
    /// </summary>
    public class CategoryViewModel
    {
        public Guid Id { get; set; }
        
        [Required(ErrorMessage = "Category name is required")]
        [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters")]
        [Display(Name = "Category Name")]
        public string Name { get; set; } = default!;
        
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        [Display(Name = "Description")]
        public string? Description { get; set; }
        
        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
        
        [Display(Name = "Created Date")]
        public DateTime CreatedAt { get; set; }
        
        [Display(Name = "Last Updated")]
        public DateTime? UpdatedAt { get; set; }
        
        [Display(Name = "Events Count")]
        public int EventsCount { get; set; }
    }
    /// <summary>
    /// ViewModel for creating a new category.
    /// </summary>

    public class CategoryCreateViewModel
    {
        [Required(ErrorMessage = "Category name is required")]
        [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters")]
        [Display(Name = "Category Name")]
        public string Name { get; set; } = default!;
        
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        [Display(Name = "Description")]
        public string? Description { get; set; }
        
        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// ViewModel for editing an existing category.
    /// </summary>
    public class CategoryEditViewModel
    {
        public Guid Id { get; set; }
        
        [Required(ErrorMessage = "Category name is required")]
        [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters")]
        [Display(Name = "Category Name")]
        public string Name { get; set; } = default!;
        
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        [Display(Name = "Description")]
        public string? Description { get; set; }
        
        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
        
        [Display(Name = "Created Date")]
        public DateTime CreatedAt { get; set; }
    }


}
