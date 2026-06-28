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
using Microsoft.Extensions.Logging;

namespace CondoNet.Booking.Infrastructure.Services
{
    public class BookingApplicationService(
        IBookingRepository bookingRepository,
        ITenantService tenantService,
        IValidator<CreateBookingRequest> validator,
        IMessageScheduler scheduler,
        ILogger<BookingApplicationService> logger,
        IEventBus eventBus)
    {
        public async Task<BookingResponse> CreateBookingAsync(CreateBookingRequest request, Guid userId)
        {
            Guid condoId = tenantService.GetCondominiumId();

            // LOG TRACE INICIAL: Intento de transacción recibido
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Iniciando procesamiento de reserva para el Usuario: {UserId} en el Condominio: {CondoId} para el Activo: {AssetId}",
                    userId, condoId, request.AssetId);
            }

            var validationResult = await validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                if (logger.IsEnabled(LogLevel.Warning))
                {
                    logger.LogWarning("Intento de reserva fallido por Errores de Validación Estructural. Usuario: {UserId}, ErroresCount: {ErrorsCount}",
                    userId, validationResult.Errors.Count);
                }
                throw new ValidationException(validationResult.Errors);
            }

            var asset = await bookingRepository.GetAssetByIdAsync(request.AssetId, condoId);
            if (asset == null || !asset.IsActive)
            {
                if (logger.IsEnabled(LogLevel.Warning))
                {
                    logger.LogWarning("Intento de reserva rechazado: Activo Inexistente o Inactivo. AssetId: {AssetId}, CondoId: {CondoId}",
                    request.AssetId, condoId);
                }
                throw new InvalidOperationException("El área común no existe o está inactiva.");
            }

            // VALIDACIÓN: Verificación de Bloqueos Técnicos / Mantenimiento
            var temporaryBooking = request.ToEntity(condoId, userId, 0);
            var projectedOccurrences = temporaryBooking.GenerateOccurrences();

            if (request.Type != BookingType.MaintenanceBlock)
            {
                bool isAreaClosed = await bookingRepository.HasActiveMaintenanceBlockAsync(condoId, request.AssetId, projectedOccurrences);
                if (isAreaClosed)
                {
                    if (logger.IsEnabled(LogLevel.Warning))
                    {
                        logger.LogWarning("Reserva rechazada por Bloqueo de Mantenimiento / Día Festivo vigente. AssetId: {AssetId}, SolicitudStart: {Start}",
                        request.AssetId, request.Start);
                    }
                    throw new InvalidOperationException("El área común se encontrará cerrada por mantenimiento.");
                }

                // VALIDACIÓN: Verificación de Deudas Morosas
                bool isBlocked = await bookingRepository.IsUserBlockedByDebtAsync(condoId, userId);
                if (isBlocked)
                {
                    if (logger.IsEnabled(LogLevel.Warning))
                    {
                        logger.LogWarning("Intento de reserva BLOQUEADO por Morosidad en Accounting. Usuario: {UserId}, CondoId: {CondoId}, AssetId: {AssetId}",
                        userId, condoId, request.AssetId);
                    }
                    throw new InvalidOperationException("El usuario presenta deudas vencidas.");
                }
            }

            decimal hourlyRate = 15.00m;
            var durationInHours = (decimal)(request.End - request.Start).TotalHours;
            decimal calculatedPrice = request.Type == BookingType.MaintenanceBlock ? 0.00m : Math.Round(durationInHours * hourlyRate, 2);

            var newBooking = request.ToEntity(condoId, userId, calculatedPrice);

            // VALIDACIÓN: Cruces de horarios de copropietarios
            bool hasCollision = await bookingRepository.HasAnyOverlappingOccurrencesAsync(condoId, newBooking.AssetId, projectedOccurrences);
            if (hasCollision)
            {
                if (logger.IsEnabled(LogLevel.Warning))
                {
                    logger.LogWarning("Conflicto de Horarios detectado. El activo {AssetId} ya se encuentra reservado en las fechas solicitadas. Solicitante: {UserId}",
                    newBooking.AssetId, userId);
                }
                throw new InvalidOperationException("El área común ya se encuentra ocupada.");
            }

            // PERSISTENCIA EXITOSA
            await bookingRepository.AddAsync(newBooking);

            // LOG DE ÉXITO DE OPERACIÓN
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Reserva CREADA exitosamente. BookingId: {BookingId}, Tipo: {BookingType}, TotalPrice: {Price}, IsConfirmed: {IsConfirmed}",
                newBooking.Id, newBooking.Type, newBooking.Price, newBooking.IsConfirmed);
            }

            if (newBooking.Price > 0 && newBooking.Type != BookingType.MaintenanceBlock)
            {
                decimal requiredDeposit = Math.Round(newBooking.Price * (asset.BookingRequiredPercentage / 100m), 2);

                // Planificación del Scheduler y Outbox...
                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation("Programando alerta de caducidad para la reserva: {BookingId}. Plazo: {Hours} horas. Depósito mínimo requerido: ${Deposit}",
                    newBooking.Id, asset.ExpirationHours, requiredDeposit);
                }
            }

            return newBooking.ToResponse(assetName: asset.Name);
        }
        public async Task CancelBookingAsync(Guid bookingId, Guid userId)
        {
            Guid condoId = tenantService.GetCondominiumId();

            // LOG TRACE INICIAL: Intento de cancelación
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Iniciando solicitud de cancelación para la Reserva: {BookingId}. Usuario solicitante: {UserId}, Condominio: {CondoId}",
                bookingId, userId, condoId);
            }

            var booking = await bookingRepository.GetByIdAsync(bookingId, condoId);
            if (booking == null)
            {
                if (logger.IsEnabled(LogLevel.Warning))
                {
                    logger.LogWarning("Intento de cancelación fallido: La reserva {BookingId} no existe en el Condominio {CondoId}",
                    bookingId, condoId);
                }
                throw new KeyNotFoundException("La reserva no existe.");
            }

            if (booking.UserId != userId)
            {
                if (logger.IsEnabled(LogLevel.Warning))
                {
                    logger.LogWarning("Intento de violación de seguridad: El Usuario {UserId} intentó cancelar la Reserva {BookingId} que le pertenece al Usuario {OwnerId}",
                    userId, bookingId, booking.UserId);
                }
                throw new UnauthorizedAccessException("No tienes permisos.");
            }

            var hoursUntilStart = (booking.Start - DateTime.UtcNow).TotalHours;
            if (hoursUntilStart < 48)
            {
                if (logger.IsEnabled(LogLevel.Warning))
                {
                    logger.LogWarning("Cancelación rechazada por regla de negocio (menos de 48 horas). Reserva: {BookingId}, Horas restantes: {HoursRemaining:F1}",
                    bookingId, hoursUntilStart);
                }
                throw new InvalidOperationException("Requiere 48 horas de anticipación.");
            }

            booking.IsConfirmed = false;
            booking.UpdatedAt = DateTime.UtcNow;

            // 1. Persistencia del cambio de estado
            await bookingRepository.UpdateAsync(booking);

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("La Reserva {BookingId} ha sido dada de baja lógicamente con éxito en la base de datos.", bookingId);
            }

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

                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation("Publicando evento de integración por cancelación hacia Accounting. Reserva: {BookingId}, Monto a reversar: ${Price}",
                    booking.Id, booking.Price);
                }
                await eventBus.PublishAsync(cancellationEvent);
            }
        }

        public async Task<bool> ConfirmBookingAsync(Guid bookingId)
        {
            Guid condoId = tenantService.GetCondominiumId();

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Recibida solicitud de confirmación de pago para la Reserva: {BookingId} en el Condominio: {CondoId}",
                bookingId, condoId);
            }

            var booking = await bookingRepository.GetByIdAsync(bookingId, condoId);
            if (booking == null)
            {
                if (logger.IsEnabled(LogLevel.Warning))
                {
                    logger.LogWarning("No se pudo confirmar la reserva: El ID {BookingId} no existe para el Condominio {CondoId}",
                    bookingId, condoId);
                }
                return false;
            }

            // Si ya estaba confirmada, registramos el evento y salimos de forma segura (Idempotencia)
            if (booking.IsConfirmed)
            {
                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation("La Reserva {BookingId} ya se encontraba previamente en estado Confirmada. Operación omitida.", bookingId);
                }
                return true;
            }

            // Se marca como confirmado (salvándolo del Consumer de Expiración)
            booking.IsConfirmed = true;
            booking.UpdatedAt = DateTime.UtcNow;

            await bookingRepository.UpdateAsync(booking);

            // LOG DE ÉXITO: La reserva queda asegurada
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("La Reserva {BookingId} ha sido CONFIRMADA exitosamente por haber cubierto el pago. Queda protegida de la caducidad horaria.",
                bookingId);
            }

            return true;
        }

    }
}
