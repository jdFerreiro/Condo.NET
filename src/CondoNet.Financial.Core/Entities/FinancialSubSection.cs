namespace CondoNet.Financial.Core.Entities
{
    public class FinancialSubSection : BaseEntity
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = string.Empty;

        public Guid? ParentSectionId { get; set; }
        public virtual FinancialSubSection? ParentSection { get; set; }
        public virtual ICollection<FinancialSubSection> ChildSections { get; set; } = new List<FinancialSubSection>();

        public virtual ICollection<UnitAccountSection> UnitAssignments { get; set; } = new List<UnitAccountSection>();
        public virtual ICollection<CondoExpense> Expenses { get; set; } = new List<CondoExpense>();
    }
}
