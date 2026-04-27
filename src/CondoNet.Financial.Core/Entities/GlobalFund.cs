namespace CondoNet.Financial.Core.Entities;

public class GlobalFund : BaseEntity
{
    public GlobalFundType FundType { get; set; } // Operativo o Reserva
    public decimal Balance { get; set; }
}

public enum GlobalFundType
{
    Operating = 1,
    Reserve = 2,
    Differential = 3,
}
