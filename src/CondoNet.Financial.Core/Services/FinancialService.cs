using CondoNet.Financial.Core.Entities;
using CondoNet.Financial.Core.Interfaces;
using CondoNet.Shared.DTOs.Financial;

namespace CondoNet.Financial.Core.Services;

public class FinancialService : IFinancialService
{
    // Dependencias típicas: repositorios, servicios de blockchain, etc.
    // Se inyectan por constructor (no implementados aquí para mantener el stub limpio)

    public FinancialService()
    {
        // Inyectar dependencias aquí
    }

    public async Task RegisterPaymentAsync(RegisterPaymentDto payment, CancellationToken cancellationToken = default)
    {
        // 1. Validar el pago y la referencia
        // 2. Buscar la unidad y actualizar saldos
        // 3. Aplicar split automático (operativo/reserva)
        // 4. Registrar la transacción en blockchain
        // 5. Guardar cambios en base de datos
        throw new NotImplementedException();
    }

    public async Task<AccountStatusDto> GetAccountStatusAsync(string unitId, CancellationToken cancellationToken = default)
    {
        // 1. Buscar la unidad y calcular el estado de cuenta
        // 2. Consultar blockchain si es necesario
        // 3. Mapear a AccountStatusDto
        throw new NotImplementedException();
    }

    public async Task SplitFundsAsync(RegisterPaymentDto payment, CancellationToken cancellationToken = default)
    {
        // 1. Calcular el split según configuración (ej: 90% operativo, 10% reserva)
        // 2. Actualizar los saldos correspondientes
        throw new NotImplementedException();
    }
}
