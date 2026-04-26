using System;

namespace CondoNet.Financial.Core.Entities
{
    public class ReportedPayment : BaseEntity
    {
        public Guid UnitAccountId { get; set; }
        public UnitAccount UnitAccount { get; set; } = null!;
        public string ReferenceNumber { get; set; } = string.Empty;
        public decimal AmountVES { get; set; }
        public string BankCode { get; set; } = string.Empty;
        public DateTime ReportedDate { get; set; }
        public bool IsVerified { get; set; }
        public Guid? BankTransactionId { get; set; }
        public BankTransaction? BankTransaction { get; set; }
    }
}
