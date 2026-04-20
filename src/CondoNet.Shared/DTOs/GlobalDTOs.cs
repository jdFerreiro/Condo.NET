namespace CondoNet.Shared.DTOs
{
    // En CondoNet.Shared
    public record Result<T>(bool IsSuccess, T? Value, string? Error = null)
    {
        public static Result<T> Success(T value) => new(true, value);
        public static Result<T> Failure(string error) => new(false, default, error);
    }

    public record SelectContextRequest(Guid ContextId);
}
