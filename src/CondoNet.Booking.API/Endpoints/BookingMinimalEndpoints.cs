using CondoNet.Booking.Core.Services;

namespace CondoNet.Booking.API.Endpoints
{
    public static class BookingMinimalEndpoints
    {
        public static IEndpointRouteBuilder MapBookingEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/booking", async (Core.Entities.Booking booking, IBookingService service) =>
            {
                var result = await service.ReserveAsync(booking);
                if (!result)
                    return Results.Conflict("No se pudo reservar el activo. Puede estar ocupado en ese horario.");
                return Results.Ok(booking);
            });

            app.MapPatch("/api/booking/{bookingId}/confirm", async (Guid bookingId, IBookingService service) =>
            {
                var result = await service.ConfirmBookingAsync(bookingId);
                if (!result)
                    return Results.NotFound();
                return Results.Ok();
            });

            app.MapGet("/api/booking/{bookingId}", async (Guid bookingId, IBookingService service) =>
            {
                var booking = await service.GetByIdAsync(bookingId);
                if (booking == null)
                    return Results.NotFound();
                return Results.Ok(booking);
            });

            app.MapGet("/api/booking", async (Guid assetId, DateTime start, DateTime end, IBookingService service) =>
            {
                var bookings = await service.GetByAssetAndPeriodAsync(assetId, start, end);
                return Results.Ok(bookings);
            });

            app.MapDelete("/api/booking/{bookingId}", async (Guid bookingId, IBookingService service) =>
            {
                var result = await service.CancelAsync(bookingId);
                if (!result)
                    return Results.NotFound();
                return Results.NoContent();
            });

            app.MapGet("/api/booking/{assetId}/month-status", async (Guid assetId, int year, int month, IBookingService service) =>
            {
                if (year < 1 || month < 1 || month > 12)
                    return Results.BadRequest("Año o mes inválido");
                var result = await service.GetMonthStatusAsync(assetId, year, month);
                return Results.Ok(result);
            });

            return app;
        }
    }
}
