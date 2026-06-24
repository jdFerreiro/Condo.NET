using CondoNet.Accounting.Core.Entities;
using CondoNet.Shared;
using CondoNet.Shared.Accounting.DTOs;
using Microsoft.EntityFrameworkCore;

namespace CondoNet.Accounting.Infrastructure.Services
{
    public partial class AccountingService
    {
        // CONSTRUCCIÓN EFICIENTE DEL ÁRBOL EN MEMORIA (SOLO 1 VIAJE A SQL)
        public async Task<Result<List<AccountNodeDto>>> GetAccountTreeAsync()
        {
            var condominiumId = tenantService.GetCondominiumId();

            // 1. Descargar todo el universo de cuentas del condominio ordenadas por código
            var accounts = await context.Set<Account>()
                .Where(a => a.CondominiumId == condominiumId)
                .OrderBy(a => a.Code)
                .ToListAsync();

            // 2. Mapear inicialmente a una lista plana de DTOs inicializando la colección de hijos vacía
            var flatNodes = accounts.Select(a => new AccountNodeDto(
                Id: a.Id,
                Code: a.Code,
                Name: a.Name,
                Type: a.Type.ToString(),
                IsTransactional: a.IsTransactional,
                CurrentBalance: a.CurrentBalance,
                IsActive: a.IsActive,
                Children: [] // Lista mutable preparada para recibir los hijos
            )).ToList();

            // Crear un diccionario rápido por Id para búsquedas instantáneas O(1)
            var nodesDictionary = flatNodes.ToDictionary(n => n.Id);
            var rootNodes = new List<AccountNodeDto>();

            // 3. Emparejar jerarquías vinculando las referencias en memoria
            foreach (var account in accounts)
            {
                var currentNode = nodesDictionary[account.Id];

                if (account.ParentAccountId.HasValue && nodesDictionary.TryGetValue(account.ParentAccountId.Value, out var parentNode))
                {
                    // Si tiene un padre válido en el catálogo, se inyecta directamente en su lista de hijos
                    parentNode.Children.Add(currentNode);
                }
                else
                {
                    // Si no tiene padre, significa que es un nodo raíz del catálogo (Ej: 1. Activo, 2. Pasivo...)
                    rootNodes.Add(currentNode);
                }
            }

            return Result<List<AccountNodeDto>>.Success(rootNodes);
        }
    }
}
