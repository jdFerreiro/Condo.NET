namespace CondoNet.Financial.Core.Entities
{
    public class Transaction
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public TransactionType Type { get; set; }
    }

    public enum TransactionType
    {
        Credit = 1, // Ingreso
        Debit = 2   // Egreso (Pago/Gasto)
    }
}
