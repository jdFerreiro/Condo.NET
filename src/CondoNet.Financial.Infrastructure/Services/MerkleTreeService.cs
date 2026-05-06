using System.Security.Cryptography;
using System.Text;
using CondoNet.Financial.Core.Services;
using CondoNet.Shared.DTOs.Financial;

namespace CondoNet.Financial.Infrastructure.Services;

public class MerkleTreeService : IMerkleTreeService
{
    public async Task<string> GenerateMerkleRootAsync(IEnumerable<ExpenseDto> expenses, CancellationToken cancellationToken = default)
    {
        // Ordenar y serializar los gastos para asegurar consistencia
        var leaves = expenses
            .OrderBy(e => e.ExpenseId)
            .Select(e => ComputeSha256Hash($"{e.ExpenseId}|{e.Description}|{e.Amount}|{e.Date:O}|{e.SubSection}"))
            .ToList();

        if (!leaves.Any())
            return string.Empty;
        if (leaves.Count == 1)
            return leaves[0];

        // Construir el árbol de Merkle
        while (leaves.Count > 1)
        {
            var nextLevel = new List<string>();
            for (int i = 0; i < leaves.Count; i += 2)
            {
                if (i + 1 < leaves.Count)
                {
                    nextLevel.Add(ComputeSha256Hash(leaves[i] + leaves[i + 1]));
                }
                else
                {
                    // Si es impar, duplicar la última hoja
                    nextLevel.Add(ComputeSha256Hash(leaves[i] + leaves[i]));
                }
            }
            leaves = nextLevel;
        }
        return leaves[0];
    }

    public bool ValidateMerkleRoot(IEnumerable<ExpenseDto> expenses, string merkleRoot)
    {
        var calculated = GenerateMerkleRootAsync(expenses).GetAwaiter().GetResult();
        return calculated == merkleRoot;
    }

    private static string ComputeSha256Hash(string rawData)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
        var sb = new StringBuilder();
        foreach (var b in bytes)
            sb.Append(b.ToString("x2"));
        return sb.ToString();
    }
}
