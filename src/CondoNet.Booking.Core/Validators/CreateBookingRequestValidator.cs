// CondoNet.Booking.Core/Validators/CreateBookingRequestValidator.cs
using CondoNet.Shared.Booking.DTOs;
using CondoNet.Shared.Booking.Enums;
using FluentValidation;

namespace CondoNet.Booking.Core.Validators;

public class CreateBookingRequestValidator : AbstractValidator<CreateBookingRequest>
{
    public CreateBookingRequestValidator()
    {
        RuleFor(x => x.AssetId)
            .NotEmpty().WithMessage("El AssetId es obligatorio.");

        RuleFor(x => x.Start)
            .GreaterThan(DateTime.UtcNow).WithMessage("La fecha de inicio debe ser en el futuro.");

        RuleFor(x => x.End)
            .GreaterThan(x => x.Start).WithMessage("La fecha de finalización debe ser posterior a la de inicio.");

        // Reglas condicionales si la reserva es recurrente
        When(x => x.Type == BookingType.Recurring, () =>
        {
            RuleFor(x => x.RecurrenceType)
                .NotEqual(RecurrenceType.None).WithMessage("Debe especificar un tipo de recurrencia válido para reservas recurrentes.");

            RuleFor(x => x.RecurrenceInterval)
                .NotNull().WithMessage("El intervalo de recurrencia es obligatorio.")
                .GreaterThan(0).WithMessage("El intervalo de recurrencia debe ser mayor a cero.");

            RuleFor(x => x.RecurrenceEnd)
                .NotNull().WithMessage("La fecha de finalización de la recurrencia es obligatoria.")
                .GreaterThan(x => x.End).WithMessage("El fin de la recurrencia debe ser posterior al fin de la primera reserva.");
        });

        // Reglas condicionales si la reserva es única
        When(x => x.Type == BookingType.Single, () =>
        {
            RuleFor(x => x.RecurrenceType)
                .Equal(RecurrenceType.None).WithMessage("Una reserva única no puede tener tipo de recurrencia.");
        });
    }
}
