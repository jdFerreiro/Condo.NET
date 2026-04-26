namespace CondoNet.Financial.Core.Entities;

public class FinancialCondominiumConfiguration : BaseEntity
{
    public decimal DebtLimit { get; set; }
    public bool BlockOnDebt { get; set; }
    public BlockType BlockType { get; set; } = BlockType.Access; // Ej: Access, Services, Voting
    public decimal ReserveFundPercentage { get; set; }
    public decimal LateFeeInterest { get; set; }
    // Otros parámetros financieros configurables...
}

public enum BlockType
{
    Access = 1,
    Services = 2,
    Voting = 3,
    PartyRoom = 4,
}
