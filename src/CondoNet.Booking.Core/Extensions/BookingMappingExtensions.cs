using CondoNet.Shared.Booking.DTOs;
using CondoNet.Shared.Booking.Enums;

namespace CondoNet.Booking.Core.Extensions
{
    public static class BookingMappingExtensions
    {
        public static Entities.Booking ToEntity(this CreateBookingRequest request, Guid condoId, Guid userId, decimal calculatedPrice)
        {
            return new Entities.Booking
            {
                Id = Guid.NewGuid(),
                CondoId = condoId,
                AssetId = request.AssetId,
                UserId = userId,
                Type = request.Type,
                Start = request.Start,
                End = request.End,
                Price = calculatedPrice,
                IsConfirmed = false, // Inicia desconfirmada hasta validar pago
                CreatedAt = DateTime.UtcNow,
                RecurrenceType = request.RecurrenceType,
                RecurrenceInterval = request.RecurrenceInterval,
                RecurrenceEnd = request.RecurrenceEnd
            };
        }

        public static BookingResponse ToResponse(this Entities.Booking entity, string assetName)
        {
            var recurrenceDto = entity.Type == BookingType.Recurring
                ? new RecurrenceDetailsDto(entity.RecurrenceType, entity.RecurrenceInterval ?? 1, entity.RecurrenceEnd)
                : null;

            return new BookingResponse(
                Id: entity.Id,
                AssetId: entity.AssetId,
                AssetName: assetName,
                UserId: entity.UserId,
                Type: entity.Type,
                Start: entity.Start,
                End: entity.End,
                Price: entity.Price,
                IsConfirmed: entity.IsConfirmed,
                Recurrence: recurrenceDto
            );
        }
    }
}
