using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using online_event_booking_system.Business.Interface;
using online_event_booking_system.Data.Entities;

namespace online_event_booking_system.Controllers.Venue
{
    [ApiController]
    [Route("api/[controller]")]
    public class VenuesController : Controller
    {

        private readonly IVenueService _venueService;

        public VenuesController(IVenueService venueService)
        {
            _venueService = venueService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllVenues()
        {
            var venues = await _venueService.GetAllVenuesAsync();
            return Ok(venues);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetVenueById(Guid id)
        {
            var venue = await _venueService.GetVenueByIdAsync(id);
            if (venue == null)
            {
                return NotFound();
            }
            return Ok(venue);
        }

        [HttpPost]
        public async Task<IActionResult> CreateVenue([FromBody] Data.Entities.Venue model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var venue = await _venueService.CreateVenueAsync(model);
            return CreatedAtAction(nameof(GetVenueById), new { id = venue.Id }, venue);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVenue(Guid id, [FromBody] Data.Entities.Venue model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var venue = await _venueService.UpdateVenueAsync(id, model);
            if (venue == null)
            {
                return NotFound();
            }
            return Ok(venue);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVenue(Guid id)
        {
            var result = await _venueService.DeleteVenueAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}
