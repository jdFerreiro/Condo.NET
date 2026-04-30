using CondoNet.Booking.Core.DTOs;
using CondoNet.Booking.Core.Entities;
using CondoNet.Booking.Core.Events;
using CondoNet.Booking.Core.Repositories;
using SharedBookingEvents = CondoNet.Shared.Events.Booking;

namespace CondoNet.Booking.Core.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingAvailabilityService _availabilityService;
        private readonly IBookingRepository _bookingRepository;
        private readonly IEventPublisher _eventPublisher;

        public BookingService(
            IBookingAvailabilityService availabilityService,
            IBookingRepository bookingRepository,
            IEventPublisher eventPublisher)
        {
            _availabilityService = availabilityService;
            _bookingRepository = bookingRepository;
            _eventPublisher = eventPublisher;
        }

        public async Task<bool> ReserveAsync(Core.Entities.Booking booking)
        {
            // Intentar adquirir el lock
            if (!await _availabilityService.TryAcquireLockAsync(booking.AssetId, booking.Id))
                return false;
            try
            {
                // Validar que no exista una reserva que se cruce en el tiempo
                if (await _bookingRepository.ExistsAsync(booking.AssetId, booking.Start, booking.End))
                    return false;
                booking.IsConfirmed = false; // Solo se confirma tras el pago
                booking.CreatedAt = DateTime.UtcNow;
                await _bookingRepository.AddAsync(booking);

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
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null)
                return false;
            if (booking.IsConfirmed)
                return true;
            booking.IsConfirmed = true;
            booking.UpdatedAt = DateTime.UtcNow;
            await _bookingRepository.UpdateAsync(booking);
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
            return await _bookingRepository.GetByIdAsync(bookingId);
        }

        public async Task<IEnumerable<Core.Entities.Booking>> GetByAssetAndPeriodAsync(Guid assetId, DateTime start, DateTime end)
        {
            return await _bookingRepository.GetByAssetAndPeriodAsync(assetId, start, end);
        }

        public async Task<bool> CancelAsync(Guid bookingId)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null)
                return false;
            // Aquí podrías marcar como cancelada en vez de eliminar
            // await _bookingRepository.RemoveAsync(booking);
            return true;
        }

        public async Task<BookingMonthStatusDto> GetMonthStatusAsync(Guid assetId, int year, int month)
        {
            var firstDay = new DateTime(year, month, 1);
            var lastDay = firstDay.AddMonths(1).AddDays(-1);
            var bookings = (await _bookingRepository.GetByAssetAndPeriodAsync(assetId, firstDay, lastDay)).ToList();
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
    }
}