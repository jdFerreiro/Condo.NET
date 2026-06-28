using CondoNet.Shared.Booking.Enums;

namespace CondoNet.Booking.Core.Extensions
{
    public static class BookingExtensions
    {
        public static IEnumerable<(DateTime Start, DateTime End)> GenerateOccurrences(this Entities.Booking booking)
        {
            var occurrences = new List<(DateTime Start, DateTime End)>();

            // Si es una reserva única, solo tiene una ocurrencia
            if (booking.Type == BookingType.Single || booking.RecurrenceType == RecurrenceType.None)
            {
                occurrences.Add((booking.Start, booking.End));
                return occurrences;
            }

            // Si es recurrente, calculamos la duración original del bloque
            TimeSpan duration = booking.End - booking.Start;
            DateTime currentStart = booking.Start;

            // Regla de Negocio: Poner un límite máximo por defecto (ej. 1 año) para evitar bucles infinitos
            DateTime maxLimit = booking.RecurrenceEnd ?? booking.Start.AddYears(1);
            int interval = booking.RecurrenceInterval ?? 1;

            if (interval <= 0) interval = 1;

            while (currentStart <= maxLimit)
            {
                DateTime currentEnd = currentStart + duration;
                occurrences.Add((currentStart, currentEnd));

                // Lógica matemática para avanzar según el patrón de recurrencia
                currentStart = booking.RecurrenceType switch
                {
                    RecurrenceType.Daily => currentStart.AddDays(interval),
                    RecurrenceType.Weekly => currentStart.AddDays(7 * interval),
                    RecurrenceType.Monthly => currentStart.AddMonths(interval),
                    _ => maxLimit.AddDays(1) // Salir del bucle de seguridad
                };
            }

            return occurrences;
        }
    }
}
