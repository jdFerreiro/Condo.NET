// CondoNet.Booking.API/Endpoints/BookingEndpoints.cs
using CondoNet.Booking.Infrastructure.Services;
using CondoNet.Shared.Booking.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CondoNet.Booking.API.Endpoints;

public static class BookingEndpoints
{
    public static void MapBookingEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/bookings")
                          .RequireAuthorization() // Protección Multi-Tenant: Valida JWT yClaims obligatorios
                          .WithTags("Bookings");

        // 1. Reserve (POST /api/bookings)
        group.MapPost("/", async ([FromBody] CreateBookingRequest request, BookingApplicationService bookingApplicationService, ClaimsPrincipal user) =>
        {
            var userId = GetUserIdFromClaims(user);

            // El servicio procesa el Record, valida con FluentValidation y ejecuta el Mapper .ToEntity()
            var response = await bookingApplicationService.CreateBookingAsync(request, userId);

            return Results.CreatedAtRoute("GetById", new { bookingId = response.Id }, response);
        })
        .WithName("Reserve")
        .Produces<BookingResponse>(StatusCodes.Status201Created)
        .Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);

        // 2. Confirm (PATCH /api/bookings/{bookingId}/confirm)
        group.MapPatch("/{bookingId:guid}/confirm", async (Guid bookingId, BookingApplicationService bookingApplicationService) =>
        {
            // Nota: Aquí invocas el método de confirmación de tu BookingApplicationService
            return Results.Ok();
        })
        .WithName("Confirm");

        // 3. GetById (GET /api/bookings/{bookingId})
        group.MapGet("/{bookingId:guid}", async (Guid bookingId, BookingService bookingService) =>
        {
            var response = await bookingService.GetByIdAsync(bookingId);
            if (response == null)
            {
                return Results.NotFound();
            }
            return Results.Ok(response);
        })
        .WithName("GetById")
        .Produces<BookingResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        // 4. List (GET /api/bookings)
        group.MapGet("/", async ([FromQuery] Guid? assetId, [FromQuery] DateTime? start, [FromQuery] DateTime? end, BookingService bookingService) =>
        {
            if (assetId == null || start == null || end == null)
            {
                return Results.BadRequest("assetId, start y end son requeridos");
            }

            var response = await bookingService.GetByAssetAndPeriodAsync(assetId.Value, start.Value, end.Value);
            return Results.Ok(response);
        })
        .WithName("List")
        .Produces<IEnumerable<BookingResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);

        // 5. Cancel (DELETE /api/bookings/{bookingId})
        group.MapDelete("/{bookingId:guid}", async (Guid bookingId, BookingApplicationService bookingService, ClaimsPrincipal user) =>
        {
            var userId = GetUserIdFromClaims(user);

            // El servicio valida la regla de negocio de las 48 horas de anticipación
            await bookingService.CancelBookingAsync(bookingId, userId);

            return Results.NoContent();
        })
        .WithName("Cancel")
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        // 6. GetMonthStatus (GET /api/bookings/{assetId}/month-status)
        group.MapGet("/{assetId:guid}/month-status", async (Guid assetId, [FromQuery] int year, [FromQuery] int month, BookingService bookingService) =>
        {
            if (year < 1 || month < 1 || month > 12)
            {
                return Results.BadRequest("Año o mes inválido");
            }

            var response = await bookingService.GetMonthStatusAsync(assetId, year, month);
            return Results.Ok(response);
        })
        .WithName("GetMonthStatus")
        .Produces<MonthStatusResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);
    }

    // Helper centralizado para mitigar la suplantación de identidad en los endpoints
    private static Guid GetUserIdFromClaims(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? user.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim))
        {
            throw new UnauthorizedAccessException("El token de identidad no contiene un ID de usuario válido.");
        }

        return Guid.Parse(userIdClaim);
    }
}
