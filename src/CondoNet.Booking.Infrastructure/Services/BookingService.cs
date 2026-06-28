using CondoNet.Booking.Core.DTOs;
using CondoNet.Booking.Core.Events;
using CondoNet.Booking.Core.Extensions;
using CondoNet.Booking.Core.Repositories;
using CondoNet.Booking.Core.Services;
using CondoNet.Shared.Booking.Enums;
using CondoNet.Shared.Interfaces;
using SharedBookingEvents = CondoNet.Shared.Events.Booking;

namespace CondoNet.Booking.Infrastructure.Services
{
    public class BookingService(
        IBookingAvailabilityService availabilityService,
        IBookingRepository bookingRepository,
        ITenantService tenantService,
        IEventBus eventPublisher) : IBookingService
    {
        private readonly IBookingAvailabilityService _availabilityService = availabilityService;
        private readonly IBookingRepository bookingRepository = bookingRepository;
        private readonly ITenantService tenantService = tenantService;
        private readonly IEventBus _eventPublisher = eventPublisher;

        public async Task<bool> ReserveAsync(Core.Entities.Booking booking)
        {
            // Intentar adquirir el lock
            if (!await _availabilityService.TryAcquireLockAsync(booking.AssetId, booking.Id))
                return false;
            try
            {
                // Validar que no exista una reserva que se cruce en el tiempo
                var condoId = tenantService.GetCondominiumId();
                if (await bookingRepository.ExistsAsync(booking.Id, condoId))
                    return false;
                booking.IsConfirmed = false; // Solo se confirma tras el pago
                booking.CreatedAt = DateTime.UtcNow;
                await bookingRepository.AddAsync(booking);

                // Publicar evento BookingCreated
                var evt = new SharedBookingEvents.BookingCreatedEvent
                {
                    BookingId = booking.Id,
                    AssetId = booking.AssetId,
                    UserId = booking.UserId,
                    Start = booking.Start,
                    End = booking.End,
                    Price = booking.Price,
                    Status = "PendingPayment"
                };
                await _eventPublisher.PublishAsync(evt);

                return true;
            }
            finally
            {
                await _availabilityService.ReleaseLockAsync(booking.AssetId, booking.Id);
            }
        }

        public async Task<bool> ReserveRecurringAsync(Core.Entities.Booking booking, int recurrenceCount)
        {
            if (booking.RecurrenceType == RecurrenceType.None || recurrenceCount <= 1)
                return await ReserveAsync(booking);
            DateTime currentStart = booking.Start;
            DateTime currentEnd = booking.End;
            int interval = booking.RecurrenceInterval ?? 1;
            for (int i = 0; i < recurrenceCount; i++)
            {
                var instance = new Core.Entities.Booking
                {
                    Id = Guid.NewGuid(),
                    AssetId = booking.AssetId,
                    UserId = booking.UserId,
                    Type = BookingType.Recurring,
                    Start = currentStart,
                    End = currentEnd,
                    Price = booking.Price,
                    IsConfirmed = false,
                    CreatedAt = DateTime.UtcNow,
                    RecurrenceType = booking.RecurrenceType,
                    RecurrenceInterval = interval,
                    RecurrenceEnd = booking.RecurrenceEnd
                };
                if (!await ReserveAsync(instance))
                    return false;
                switch (booking.RecurrenceType)
                {
                    case RecurrenceType.Daily:
                        currentStart = currentStart.AddDays(interval);
                        currentEnd = currentEnd.AddDays(interval);
                        break;
                    case RecurrenceType.Weekly:
                        currentStart = currentStart.AddDays(7 * interval);
                        currentEnd = currentEnd.AddDays(7 * interval);
                        break;
                    case RecurrenceType.Monthly:
                        currentStart = currentStart.AddMonths(interval);
                        currentEnd = currentEnd.AddMonths(interval);
                        break;
                }
                if (booking.RecurrenceEnd.HasValue && currentStart > booking.RecurrenceEnd.Value)
                    break;
            }
            return true;
        }

        public async Task<bool> ConfirmBookingAsync(Guid bookingId)
        {
            var condoId = tenantService.GetCondominiumId();
            var booking = await bookingRepository.GetByIdAsync(bookingId, condoId);
            if (booking == null)
                return false;
            if (booking.IsConfirmed)
                return true;
            booking.IsConfirmed = true;
            booking.UpdatedAt = DateTime.UtcNow;
            await bookingRepository.UpdateAsync(booking);
            var evt = new SharedBookingEvents.BookingConfirmedEvent
            {
                BookingId = booking.Id,
                AssetId = booking.AssetId,
                UserId = booking.UserId,
                Start = booking.Start,
                End = booking.End,
                Price = booking.Price,
                Status = "Confirmed"
            };
            await _eventPublisher.PublishAsync(evt);
            return true;
        }

        public async Task<Core.Entities.Booking?> GetByIdAsync(Guid bookingId)
        {
            var condoId = tenantService.GetCondominiumId();
            return await bookingRepository.GetByIdAsync(bookingId, condoId);
        }

        public async Task<IEnumerable<Core.Entities.Booking>> GetByAssetAndPeriodAsync(Guid assetId, DateTime start, DateTime end)
        {
            var condoId = tenantService.GetCondominiumId();
            return await bookingRepository.GetByAssetAndPeriodAsync(condoId, assetId, start, end);
        }

        public async Task<bool> CancelAsync(Guid bookingId)
        {
            var condoId = tenantService.GetCondominiumId();
            var booking = await bookingRepository.GetByIdAsync(bookingId, condoId);
            if (booking == null)
                return false;
            // Aquí podrías marcar como cancelada en vez de eliminar
            // await bookingRepository.RemoveAsync(booking);
            return true;
        }

        public async Task<BookingMonthStatusDto> GetMonthStatusAsync(Guid assetId, int year, int month)
        {
            var firstDay = new DateTime(year, month, 1);
            var lastDay = firstDay.AddMonths(1).AddDays(-1);
            var condoId = tenantService.GetCondominiumId();
            var bookings = (await bookingRepository.GetByAssetAndPeriodAsync(condoId, assetId, firstDay, lastDay)).ToList();
            var confirmed = bookings.Where(b => b.IsConfirmed).ToList();
            var pending = bookings.Where(b => !b.IsConfirmed).ToList();
            var reservedDays = new HashSet<int>();
            foreach (var b in bookings)
            {
                var start = b.Start < firstDay ? firstDay : b.Start;
                var end = b.End > lastDay ? lastDay : b.End;
                for (var d = start.Date; d <= end.Date; d = d.AddDays(1))
                    reservedDays.Add(d.Day);
            }
            var freeDays = new List<DateTime>();
            for (int day = 1; day <= lastDay.Day; day++)
            {
                if (!reservedDays.Contains(day))
                    freeDays.Add(new DateTime(year, month, day));
            }
            return new BookingMonthStatusDto
            {
                FreeDays = freeDays,
                Confirmed = confirmed,
                Pending = pending
            };
        }
        // Fragmento lógico del Service que unifica todo:
        public async Task<bool> ProcessNewBookingAsync(Core.Entities.Booking newBooking)
        {
            // 1. Generar la proyección matemática de fechas
            var projectedOccurrences = newBooking.GenerateOccurrences();

            // 2. Ejecutar la validación masiva en persistencia
            bool isOccupied = await bookingRepository.HasAnyOverlappingOccurrencesAsync(
                newBooking.CondoId,
                newBooking.AssetId,
                projectedOccurrences
            );

            if (isOccupied)
            {
                throw new InvalidOperationException("El recurso no está disponible en una o más fechas solicitadas para la serie.");
            }

            // 3. Guardar la reserva cabecera si pasa la validación
            await bookingRepository.AddAsync(newBooking);
            return true;
        }
    }
}