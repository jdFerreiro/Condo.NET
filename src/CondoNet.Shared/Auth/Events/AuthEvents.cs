namespace CondoNet.Shared.Auth.Events
{
    public record UserLoggedInEvents(
        Guid UserId,
        Guid? CurrentCondoId,
        string FullName,
        DateTime LoginDate
    );

    public record UserLoggedOutEvent(
        Guid UserId,
        string RefreshToken, // Útil para identificar qué sesión específica se cerró
        DateTime LogoutDate
    );

    public record UserActiveCondoChangeEvent(
        Guid UserId,
        Guid? CurrentCondoId,
        string FullName,
        DateTime CondoChangeDate
    );

}
