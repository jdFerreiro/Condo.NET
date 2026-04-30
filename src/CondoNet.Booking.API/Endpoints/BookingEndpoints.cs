using CondoNet.Booking.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace CondoNet.Booking.API.Endpoints
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingEndpoints(IBookingService bookingService) : ControllerBase
    {
        private readonly IBookingService _bookingService = bookingService;

        [HttpPost]
        public async Task<IActionResult> Reserve([FromBody] Core.Entities.Booking booking)
        {
            var result = await _bookingService.ReserveAsync(booking);
            if (!result)
                return Conflict("No se pudo reservar el activo. Puede estar ocupado en ese horario.");
            return Ok(booking);
        }

        [HttpPatch("{bookingId}/confirm")]
        public async Task<IActionResult> Confirm(Guid bookingId)
        {
            var result = await _bookingService.ConfirmBookingAsync(bookingId);
            if (!result)
                return NotFound();
            return Ok();
        }

        [HttpGet("{bookingId}")]
        public async Task<IActionResult> GetById(Guid bookingId)
        {
            var booking = await _bookingService.GetByIdAsync(bookingId);
            if (booking == null)
                return NotFound();
            return Ok(booking);
        }

        [HttpGet]
        public async Task<IActionResult> List([FromQuery] Guid? assetId, [FromQuery] DateTime? start, [FromQuery] DateTime? end)
        {
            if (assetId == null || start == null || end == null)
                return BadRequest("assetId, start y end son requeridos");
            var bookings = await _bookingService.GetByAssetAndPeriodAsync(assetId.Value, start.Value, end.Value);
            return Ok(bookings);
        }

        [HttpDelete("{bookingId}")]
        public async Task<IActionResult> Cancel(Guid bookingId)
        {
            var result = await _bookingService.CancelAsync(bookingId);
            if (!result)
                return NotFound();
            return NoContent();
        }

        [HttpGet("{assetId}/month-status")]
        public async Task<IActionResult> GetMonthStatus(Guid assetId, [FromQuery] int year, [FromQuery] int month)
        {
            if (year < 1 || month < 1 || month > 12)
                return BadRequest("Año o mes inválido");
            var result = await _bookingService.GetMonthStatusAsync(assetId, year, month);
            return Ok(result);
        }
    }
}