using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using System.Numerics;

namespace CondoNet.BlockChain.Infrastructure.ContractDefinition;

[Function("registrarPago")]
public class RegistrarPagoFunction : FunctionMessage
{
    [Parameter("string", "unitId", 1)]
    public string UnitId { get; set; } = string.Empty;

    [Parameter("uint256", "monto", 2)]
    public BigInteger Monto { get; set; }

    [Parameter("string", "referencia", 3)]
    public string Referencia { get; set; } = string.Empty;
}
