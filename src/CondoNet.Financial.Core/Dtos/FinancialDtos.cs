namespace CondoNet.Financial.Core.Dtos
{
    public record CreateTransactionDto(int AccountId, decimal Amount, int Type, string Description);
    public record TransactionDto(int Id, decimal Amount, DateTime Date, string Description, string Type);
    public record InvoiceDto(int Id, string InvoiceNumber, decimal TotalAmount, DateTime DueDate, bool IsPaid);
    public record TransactionResultDto(bool Success, string Message);

}
