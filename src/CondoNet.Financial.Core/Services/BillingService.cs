using CondoNet.Financial.Core.Interfaces;
using CondoNet.Shared.DTOs.Financial;

namespace CondoNet.Financial.Core.Services;



public class BillingService : IBillingService
{
    private readonly ICondoExpenseRepository _expenseRepo;
    private readonly IFinancialSubSectionRepository _subSectionRepo;
    private readonly IUnitAccountSectionRepository _unitAccountSectionRepo;
    private readonly IUnitAccountRepository _unitAccountRepo;
    private readonly IInvoiceRepository _invoiceRepo;
    private readonly IInvoiceItemRepository _invoiceItemRepo;
    private readonly IMerkleTreeService _merkleTreeService;
    private readonly IBlockchainIntegrationService _blockchainIntegrationService;
    private readonly IAuditLogRepository _auditLogRepository;

    public BillingService(
        ICondoExpenseRepository expenseRepo,
        IFinancialSubSectionRepository subSectionRepo,
        IUnitAccountSectionRepository unitAccountSectionRepo,
        IUnitAccountRepository unitAccountRepo,
        IInvoiceRepository invoiceRepo,
        IInvoiceItemRepository invoiceItemRepo,
        IMerkleTreeService merkleTreeService,
        IBlockchainIntegrationService blockchainIntegrationService,
        IAuditLogRepository auditLogRepository)
    {
        _expenseRepo = expenseRepo;
        _subSectionRepo = subSectionRepo;
        _unitAccountSectionRepo = unitAccountSectionRepo;
        _unitAccountRepo = unitAccountRepo;
        _invoiceRepo = invoiceRepo;
        _invoiceItemRepo = invoiceItemRepo;
        _merkleTreeService = merkleTreeService;
        _blockchainIntegrationService = blockchainIntegrationService;
        _auditLogRepository = auditLogRepository;
    }

    public async Task<List<MonthlyBillDto>> GenerateMonthlyBillAsync(DateTime period, CancellationToken cancellationToken = default)
    {
        // 1. Obtener todos los gastos del mes
        var expenses = await _expenseRepo.GetByPeriodAsync(period.Month, period.Year, cancellationToken);
        var expensesList = expenses.ToList();
        if (!expensesList.Any())
            throw new InvalidOperationException("No hay gastos registrados para el periodo");

        // 2. Obtener todas las subsecciones involucradas
        var subSectionIds = expensesList.Select(e => e.FinancialSubSectionId).Distinct().ToList();
        var allSubSections = new List<Entities.FinancialSubSection>();
        foreach (var subId in subSectionIds)
        {
            var sub = await _subSectionRepo.GetByIdAsync(subId, cancellationToken);
            if (sub != null) allSubSections.Add(sub);
        }

        // 3. Obtener todas las unidades y sus asignaciones
        var allAssignments = new List<Entities.UnitAccountSection>();
        foreach (var subId in subSectionIds)
        {
            var assignments = await _unitAccountSectionRepo.GetByFinancialSubSectionIdAsync(subId, cancellationToken);
            allAssignments.AddRange(assignments);
        }

        var allUnitIds = allAssignments.Select(a => a.UnitAccountId).Distinct().ToList();
        var allUnits = new List<Entities.UnitAccount>();
        foreach (var unitId in allUnitIds)
        {
            var unit = await _unitAccountRepo.GetByIdAsync(unitId, cancellationToken);
            if (unit != null) allUnits.Add(unit);
        }

        // 4. Calcular el monto total y los items por unidad
        var bills = new List<MonthlyBillDto>();
        foreach (var unit in allUnits)
        {
            var bill = new MonthlyBillDto
            {
                UnitAccountId = unit.Id.ToString(),
                OwnerName = unit.OwnerName,
                TotalAmount = 0m,
                Items = new List<MonthlyBillItemDto>()
            };

            // Buscar todas las asignaciones de la unidad
            var unitAssignments = allAssignments.Where(a => a.UnitAccountId == unit.Id).ToList();
            var invoiceItems = new List<Entities.InvoiceItem>();
            decimal totalAmount = 0m;

            foreach (var assignment in unitAssignments)
            {
                // Buscar gastos de la subsección
                var subExpenses = expensesList.Where(e => e.FinancialSubSectionId == assignment.FinancialSubSectionId).ToList();
                var totalPercentage = allAssignments.Where(a => a.FinancialSubSectionId == assignment.FinancialSubSectionId).Sum(a => a.ParticipationPercentage);
                if (totalPercentage == 0) continue;

                foreach (var expense in subExpenses)
                {
                    var amount = Math.Round(expense.Amount * (assignment.ParticipationPercentage / totalPercentage), 2);
                    bill.Items.Add(new MonthlyBillItemDto
                    {
                        ExpenseId = expense.Id.ToString(),
                        Description = expense.Description,
                        Amount = amount,
                        SubSectionId = assignment.FinancialSubSectionId.ToString()
                    });
                    totalAmount += amount;

                    invoiceItems.Add(new Entities.InvoiceItem
                    {
                        Description = expense.Description,
                        Amount = amount,
                        RelatedSubSectionId = assignment.FinancialSubSectionId
                    });
                }
            }

            bill.TotalAmount = totalAmount;
            bills.Add(bill);

            // Crear o actualizar la factura real
            var invoices = await _invoiceRepo.GetByUnitAccountIdAsync(unit.Id, cancellationToken);
            var invoice = invoices.FirstOrDefault(i => i.DueDate.Month == period.Month && i.DueDate.Year == period.Year);
            if (invoice == null)
            {
                invoice = new Entities.Invoice
                {
                    UnitAccountId = unit.Id,
                    UnitAccount = unit,
                    DueDate = new DateTime(period.Year, period.Month, DateTime.DaysInMonth(period.Year, period.Month)),
                    Status = Entities.InvoiceStatus.Pending,
                    TotalAmount = totalAmount,
                    Number = $"{period:yyyyMM}-{unit.Id.ToString().Substring(0, 8)}",
                    FechaEmision = DateTime.UtcNow,
                    IsEmitted = true
                };
                await _invoiceRepo.AddAsync(invoice, cancellationToken);
            }
            else
            {
                invoice.TotalAmount = totalAmount;
                invoice.FechaEmision = DateTime.UtcNow;
                invoice.IsEmitted = true;
                await _invoiceRepo.UpdateAsync(invoice, cancellationToken);
            }

            // Agregar los items a la factura
            foreach (var item in invoiceItems)
            {
                item.InvoiceId = invoice.Id;
                await _invoiceItemRepo.AddAsync(item, cancellationToken);
            }
        }

        // 6. Calcular Merkle Root de los gastos del periodo
        var expenseDtos = expensesList.Select(e => new CondoNet.Shared.DTOs.Financial.ExpenseDto
        {
            ExpenseId = e.Id.ToString(),
            Description = e.Description,
            Amount = e.Amount,
            Date = e.ExpenseDate,
            SubSection = e.FinancialSubSectionId.ToString()
        }).ToList();

        var merkleRoot = await _merkleTreeService.GenerateMerkleRootAsync(expenseDtos, cancellationToken);

        // 7. Anclar el Merkle Root en blockchain
        await _blockchainIntegrationService.AnchorMerkleRootAsync(merkleRoot, period, cancellationToken);

        return bills;
    }

    public async Task<ProratedExpenseDto> GenerateExpenseAsync(ExpenseDto expense, CancellationToken cancellationToken = default)
    {
        // 1. Validar subsección
        if (!Guid.TryParse(expense.SubSection, out var subSectionId))
            throw new ArgumentException("SubSection debe ser un GUID válido");

        var subSection = await _subSectionRepo.GetByIdAsync(subSectionId, cancellationToken);
        if (subSection == null)
            throw new InvalidOperationException("La subsección financiera no existe");

        // 2. Crear el gasto
        var condoExpense = new Entities.CondoExpense
        {
            Description = expense.Description,
            Amount = expense.Amount,
            ExpenseDate = expense.Date,
            Month = expense.Date.Month,
            Year = expense.Date.Year,
            FinancialSubSectionId = subSectionId,
            FinancialSubSection = subSection,
            DocumentUrl = string.Empty, // Asignar si aplica
            DocumentHash = string.Empty // Calcular si aplica
        };
        await _expenseRepo.AddAsync(condoExpense, cancellationToken);

        // 3. Obtener unidades vinculadas a la subsección
        var assignments = (await _unitAccountSectionRepo.GetByFinancialSubSectionIdAsync(subSectionId, cancellationToken)).ToList();
        if (!assignments.Any())
            throw new InvalidOperationException("No hay unidades vinculadas a la subsección para prorratear el gasto");

        // 4. Calcular el monto prorrateado para cada unidad
        var totalPercentage = assignments.Sum(u => u.ParticipationPercentage);
        if (totalPercentage == 0)
            throw new InvalidOperationException("La suma de los porcentajes de participación es 0");

        var proratedList = new List<ProratedExpenseUnitDto>();
        foreach (var unit in assignments)
        {
            var amount = Math.Round(condoExpense.Amount * (unit.ParticipationPercentage / totalPercentage), 2);

            // Obtener la unidad
            var unitAccount = await _unitAccountRepo.GetByIdAsync(unit.UnitAccountId, cancellationToken);
            if (unitAccount == null)
                continue;

            // Crear o buscar la factura del periodo
            var invoices = await _invoiceRepo.GetByUnitAccountIdAsync(unit.UnitAccountId, cancellationToken);
            var invoice = invoices.FirstOrDefault(i => i.DueDate.Month == expense.Date.Month && i.DueDate.Year == expense.Date.Year);
            if (invoice == null)
            {
                invoice = new Entities.Invoice
                {
                    UnitAccountId = unit.UnitAccountId,
                    UnitAccount = unitAccount,
                    DueDate = new DateTime(expense.Date.Year, expense.Date.Month, DateTime.DaysInMonth(expense.Date.Year, expense.Date.Month)),
                    Status = Entities.InvoiceStatus.Pending,
                    TotalAmount = 0m,
                    Number = $"{expense.Date:yyyyMM}-{unit.UnitAccountId.ToString().Substring(0, 8)}"
                };
                await _invoiceRepo.AddAsync(invoice, cancellationToken);
            }

            // Crear el InvoiceItem
            var item = new Entities.InvoiceItem
            {
                InvoiceId = invoice.Id,
                Description = expense.Description,
                Amount = amount,
                RelatedSubSectionId = subSectionId
            };
            await _invoiceItemRepo.AddAsync(item, cancellationToken);

            // Actualizar el total de la factura
            invoice.TotalAmount += amount;
            await _invoiceRepo.UpdateAsync(invoice, cancellationToken);

            // Actualizar la deuda de la unidad
            unitAccount.CurrentDebt += amount;
            await _unitAccountRepo.UpdateAsync(unitAccount, cancellationToken);

            proratedList.Add(new ProratedExpenseUnitDto
            {
                UnitAccountId = unit.UnitAccountId.ToString(),
                ParticipationPercentage = unit.ParticipationPercentage,
                ProratedAmount = amount
            });
        }

        // Retornar el resultado del prorrateo
        return new ProratedExpenseDto
        {
            ExpenseId = condoExpense.Id.ToString(),
            SubSectionId = subSectionId.ToString(),
            Description = expense.Description,
            TotalAmount = expense.Amount,
            ExpenseDate = expense.Date,
            Units = proratedList
        };
    }

    public async Task<string> CalculateMerkleRootAsync(DateTime period, CancellationToken cancellationToken = default)
    {
        // 1. Obtener todos los gastos del periodo
        // 2. Calcular Merkle Root usando IMerkleTreeService
        // 3. Retornar el hash
        throw new NotImplementedException();
    }

    /// <summary>
    /// Actualiza un gasto solo si el Merkle Root del periodo no está anclado en blockchain.
    /// Si está anclado, lanza excepción y dispara alerta de auditoría.
    /// </summary>
    public async Task UpdateExpenseAsync(Guid expenseId, ExpenseDto updatedExpense, CancellationToken cancellationToken = default)
    {
        var expense = await _expenseRepo.GetByIdAsync(expenseId, cancellationToken);
        if (expense == null)
            throw new InvalidOperationException("El gasto no existe.");

        // Verifica si el Merkle Root del periodo está anclado
        var period = new DateTime(expense.Year, expense.Month, 1);
        var expenses = await _expenseRepo.GetByPeriodAsync(expense.Month, expense.Year, cancellationToken);
        var expenseDtos = expenses.Select(e => new ExpenseDto
        {
            ExpenseId = e.Id.ToString(),
            Description = e.Description,
            Amount = e.Amount,
            Date = e.ExpenseDate,
            SubSection = e.FinancialSubSectionId.ToString()
        }).ToList();
        var merkleRoot = await _merkleTreeService.GenerateMerkleRootAsync(expenseDtos, cancellationToken);
        var isAnchored = await _blockchainIntegrationService.ValidateMerkleRootAsync(merkleRoot, period, cancellationToken);
        if (isAnchored)
        {
            // Auditoría real: registrar el intento
            await _auditLogRepository.AddAsync(new Entities.AuditLog
            {
                Action = "Intento de modificación de gasto anclado",
                EntityName = nameof(Entities.CondoExpense),
                EntityId = expenseId.ToString(),
                UserName = "system", // Reemplazar por usuario real si está disponible
                Details = $"Intento de modificar gasto anclado en blockchain. Descripción nueva: {updatedExpense.Description}, Monto nuevo: {updatedExpense.Amount}",
                Timestamp = DateTime.UtcNow
            }, cancellationToken);
            throw new InvalidOperationException("No se puede modificar un gasto ya anclado en blockchain. Se ha registrado una alerta de auditoría.");
        }

        // Actualiza el gasto
        expense.Description = updatedExpense.Description;
        expense.Amount = updatedExpense.Amount;
        expense.ExpenseDate = updatedExpense.Date;
        expense.FinancialSubSectionId = Guid.Parse(updatedExpense.SubSection);
        await _expenseRepo.UpdateAsync(expense, cancellationToken);
    }
}
