// CondoNet.Booking.Core/Services/BookingApplicationService.cs
using CondoNet.Booking.Core.Events; // <--- Importamos los eventos
using CondoNet.Booking.Core.Extensions;
using CondoNet.Booking.Core.Repositories;
using CondoNet.Shared.Booking.DTOs;
using CondoNet.Shared.Booking.Enums;
using CondoNet.Shared.Booking.Events;
using CondoNet.Shared.Interfaces;
using FluentValidation;
using MassTransit;

namespace CondoNet.Booking.Infrastructure.Services
{
    public class BookingApplicationService(
        IBookingRepository bookingRepository,
        ITenantService tenantService,
        IValidator<CreateBookingRequest> validator,
        IMessageScheduler scheduler,
        IEventBus eventBus)
    {
        public async Task CancelBookingAsync(Guid bookingId, Guid userId)
        {
            Guid condoId = tenantService.GetCondominiumId();

            var booking = await bookingRepository.GetByIdAsync(bookingId, condoId) ?? throw new KeyNotFoundException("La reserva no existe.");
            if (booking.UserId != userId) throw new UnauthorizedAccessException("No tienes permisos.");

            var hoursUntilStart = (booking.Start - DateTime.UtcNow).TotalHours;
            if (hoursUntilStart < 48) throw new InvalidOperationException("Requiere 48 horas de anticipación.");

            booking.IsConfirmed = false;
            booking.UpdatedAt = DateTime.UtcNow;

            // 1. Persistencia del cambio de estado
            await bookingRepository.UpdateAsync(booking);

            // 2. PUBLICACIÓN ASÍNCRONA: Notificar a Accounting para liberar o reversar el saldo
            if (booking.Price > 0)
            {
                var cancellationEvent = new BookingCancelledIntegrationEvent(
                    EventId: Guid.NewGuid(),
                    OccurredOn: DateTime.UtcNow,
                    BookingId: booking.Id,
                    CondoId: booking.CondoId,
                    UserId: booking.UserId,
                    Price: booking.Price
                );

                await eventBus.PublishAsync(cancellationEvent);
            }
        }
        // Añadir esta lógica en tu BookingApplicationService.cs
        public async Task<bool> ConfirmBookingAsync(Guid bookingId)
        {
            Guid condoId = tenantService.GetCondominiumId();

            var booking = await bookingRepository.GetByIdAsync(bookingId, condoId);
            if (booking == null) return false;

            // Se marca como confirmado (salvándolo del Consumer de Expiración)
            booking.IsConfirmed = true;
            booking.UpdatedAt = DateTime.UtcNow;

            await bookingRepository.UpdateAsync(booking);
            return true;
        }
        // CondoNet.Booking.Core/Services/BookingApplicationService.cs
        public async Task<BookingResponse> CreateBookingAsync(CreateBookingRequest request, Guid userId)
        {
            var validationResult = await validator.ValidateAsync(request);
            if (!validationResult.IsValid) throw new ValidationException(validationResult.Errors);

            Guid condoId = tenantService.GetCondominiumId();

            var asset = await bookingRepository.GetAssetByIdAsync(request.AssetId, condoId);
            if (asset == null || !asset.IsActive) throw new InvalidOperationException("El área común no existe o está inactiva.");

            // 1. Proyectar matemáticamente las ocurrencias de la solicitud actual
            // Creamos una entidad temporal para usar nuestro método de extensión GenerateOccurrences()
            var temporaryBooking = request.ToEntity(condoId, userId, 0);
            var projectedOccurrences = temporaryBooking.GenerateOccurrences();

            // ====================================================================
            // REGLA DE NEGOCIO: Validar Bloqueos de Mantenimiento / Días Festivos
            // ====================================================================
            // Si la petición NO es un bloqueo administrativo, validamos que el área esté abierta
            if (request.Type != BookingType.MaintenanceBlock)
            {
                bool isAreaClosed = await bookingRepository.HasActiveMaintenanceBlockAsync(condoId, request.AssetId, projectedOccurrences);
                if (isAreaClosed)
                {
                    throw new InvalidOperationException("El área común seleccionada se encontrará cerrada por mantenimiento o día festivo en el horario solicitado.");
                }

                // Validar morosidad (Solo aplica a copropietarios)
                bool isBlocked = await bookingRepository.IsUserBlockedByDebtAsync(condoId, userId);
                if (isBlocked) throw new InvalidOperationException("El usuario presenta deudas vencidas.");
            }

            // 2. Calcular precios y tarifas...
            decimal hourlyRate = 15.00m;
            var durationInHours = (decimal)(request.End - request.Start).TotalHours;
            decimal calculatedPrice = request.Type == BookingType.MaintenanceBlock ? 0.00m : Math.Round(durationInHours * hourlyRate, 2);

            // 3. Mapear e insertar la entidad definitiva
            var newBooking = request.ToEntity(condoId, userId, calculatedPrice);

            // Si es un bloqueo de mantenimiento, nace auto-confirmado por la administración
            if (newBooking.Type == BookingType.MaintenanceBlock)
            {
                newBooking.IsConfirmed = true;
            }

            // 4. Validar colisiones contra otras reservas ordinarias de usuarios
            bool hasCollision = await bookingRepository.HasAnyOverlappingOccurrencesAsync(condoId, newBooking.AssetId, projectedOccurrences);
            if (hasCollision) throw new InvalidOperationException("El área común ya se encuentra ocupada en los horarios solicitados.");

            await bookingRepository.AddAsync(newBooking);

            // 5. Publicación y políticas de vencimiento (Solo si requiere pago y no es bloqueo)
            if (newBooking.Price > 0 && newBooking.Type != BookingType.MaintenanceBlock)
            {
                decimal requiredDeposit = Math.Round(newBooking.Price * (asset.BookingRequiredPercentage / 100m), 2);

                await eventBus.PublishAsync(new BookingCreatedIntegrationEvent(
                    Guid.NewGuid(), DateTime.UtcNow, newBooking.Id, newBooking.CondoId, newBooking.UserId, newBooking.AssetId, newBooking.Price, requiredDeposit, newBooking.Start, newBooking.End
                ));

                await scheduler.SchedulePublish(
                    delay: TimeSpan.FromHours(asset.ExpirationHours),
                    message: new BookingExpirationReminder(newBooking.Id)
                );
            }

            return newBooking.ToResponse(assetName: asset.Name);
        }

    }
}
