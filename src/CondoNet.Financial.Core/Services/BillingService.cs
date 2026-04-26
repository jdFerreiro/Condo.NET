using CondoNet.Financial.Core.Interfaces;
using CondoNet.Shared.DTOs.Financial;

namespace CondoNet.Financial.Core.Services;

public class BillingService : IBillingService
{
    // Dependencias típicas: repositorios, servicios de Merkle, etc.
    // Se inyectan por constructor (no implementados aquí para mantener el stub limpio)

    public BillingService()
    {
        // Inyectar dependencias aquí
    }

    public async Task GenerateMonthlyBillAsync(DateTime period, CancellationToken cancellationToken = default)
    {
        // 1. Obtener gastos del mes y subsección
        // 2. Obtener unidades vinculadas a cada subsección
        // 3. Prorratear gastos según participación
        // 4. Crear facturas (Invoice) y sus items
        // 5. Calcular Merkle Root y asociar
        // 6. Guardar en base de datos
        // 7. (Opcional) Anclar Merkle Root en blockchain
        throw new NotImplementedException();
    }

    public async Task GenerateExpenseAsync(ExpenseDto expense, CancellationToken cancellationToken = default)
    {
        // 1. Validar y mapear ExpenseDto a CondoExpense
        // 2. Asociar a la subsección correspondiente
        // 3. Calcular hash del documento si aplica
        // 4. Guardar en base de datos
        throw new NotImplementedException();
    }

    public async Task<string> CalculateMerkleRootAsync(DateTime period, CancellationToken cancellationToken = default)
    {
        // 1. Obtener todos los gastos del periodo
        // 2. Calcular Merkle Root usando IMerkleTreeService
        // 3. Retornar el hash
        throw new NotImplementedException();
    }
}
