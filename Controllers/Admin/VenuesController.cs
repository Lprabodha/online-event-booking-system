using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using online_event_booking_system.Business.Interface;
using online_event_booking_system.Data.Entities;

namespace online_event_booking_system.Controllers.Admin
{
    [ApiController]
    [Route("api/[controller]")]
    public class VenuesController : Controller
    {
        private readonly IVenueService _venueService;
        private readonly ILogger<VenuesController> _logger;

        public VenuesController(IVenueService venueService, ILogger<VenuesController> logger)
        {
            _venueService = venueService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllVenues()
        {
            try
            {
                var venues = await _venueService.GetAllVenuesAsync();
                return Ok(new { success = true, data = venues });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving venues");
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving venues" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetVenueById(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return BadRequest(new { success = false, message = "Invalid venue ID" });
                }

                var venue = await _venueService.GetVenueByIdAsync(id);
                if (venue == null)
                {
                    return NotFound(new { success = false, message = "Venue not found" });
                }
                return Ok(new { success = true, data = venue });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving venue with ID: {VenueId}", id);
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving the venue" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateVenue([FromBody] Venue model)
        {
            try
            {
                if (model == null)
                {
                    return BadRequest(new { success = false, message = "Venue data is required" });
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(new { success = false, message = "Validation failed", errors = errors });
                }

                // Additional business validation
                var validationResult = await ValidateVenueBusinessRules(model);
                if (!validationResult.IsValid)
                {
                    return BadRequest(new { success = false, message = "Validation failed", errors = validationResult.Errors });
                }

                var venue = await _venueService.CreateVenueAsync(model);
                return CreatedAtAction(nameof(GetVenueById), new { id = venue.Id }, 
                    new { success = true, message = "Venue created successfully", data = venue });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error creating venue");
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Business rule violation creating venue");
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating venue");
                return StatusCode(500, new { success = false, message = "An error occurred while creating the venue" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVenue(Guid id, [FromBody] Venue model)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return BadRequest(new { success = false, message = "Invalid venue ID" });
                }

                if (model == null)
                {
                    return BadRequest(new { success = false, message = "Venue data is required" });
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(new { success = false, message = "Validation failed", errors = errors });
                }

                // Additional business validation
                var validationResult = await ValidateVenueBusinessRules(model, id);
                if (!validationResult.IsValid)
                {
                    return BadRequest(new { success = false, message = "Validation failed", errors = validationResult.Errors });
                }

                var venue = await _venueService.UpdateVenueAsync(id, model);
                if (venue == null)
                {
                    return NotFound(new { success = false, message = "Venue not found" });
                }

                return Ok(new { success = true, message = "Venue updated successfully", data = venue });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error updating venue with ID: {VenueId}", id);
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Business rule violation updating venue with ID: {VenueId}", id);
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating venue with ID: {VenueId}", id);
                return StatusCode(500, new { success = false, message = "An error occurred while updating the venue" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVenue(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return BadRequest(new { success = false, message = "Invalid venue ID" });
                }

                var result = await _venueService.DeleteVenueAsync(id);
                if (!result)
                {
                    return NotFound(new { success = false, message = "Venue not found" });
                }

                return Ok(new { success = true, message = "Venue deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting venue with ID: {VenueId}", id);
                return StatusCode(500, new { success = false, message = "An error occurred while deleting the venue" });
            }
        }

        private async Task<(bool IsValid, List<string> Errors)> ValidateVenueBusinessRules(Venue model, Guid? existingId = null)
        {
            var errors = new List<string>();

            try
            {
                // Check if venue name already exists (excluding current venue for updates)
                var existingVenues = await _venueService.GetAllVenuesAsync();
                var duplicateName = existingVenues.Any(v => 
                    v.Name.Equals(model.Name, StringComparison.OrdinalIgnoreCase) && 
                    (!existingId.HasValue || v.Id != existingId.Value));

                if (duplicateName)
                {
                    errors.Add("A venue with this name already exists");
                }

                // Check if location and name combination already exists
                var duplicateLocation = existingVenues.Any(v => 
                    v.Name.Equals(model.Name, StringComparison.OrdinalIgnoreCase) &&
                    v.Location.Equals(model.Location, StringComparison.OrdinalIgnoreCase) &&
                    (!existingId.HasValue || v.Id != existingId.Value));

                if (duplicateLocation)
                {
                    errors.Add("A venue with this name and location combination already exists");
                }

                // Additional capacity validation
                if (model.Capacity <= 0)
                {
                    errors.Add("Capacity must be greater than 0");
                }

                return (errors.Count == 0, errors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating venue business rules");
                errors.Add("An error occurred during validation");
                return (false, errors);
            }
        }
    }
}
