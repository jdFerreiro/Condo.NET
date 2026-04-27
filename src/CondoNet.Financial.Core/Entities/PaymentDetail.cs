using System;
using CondoNet.Financial.Core.Entities;

namespace CondoNet.Financial.Core.Entities;

public class PaymentDetail : BaseEntity
{
    public Guid PaymentId { get; set; }
    public Payment Payment { get; set; } = null!;
    public string Currency { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal RateAtMoment { get; set; }
    public string Method { get; set; } = string.Empty;
}
