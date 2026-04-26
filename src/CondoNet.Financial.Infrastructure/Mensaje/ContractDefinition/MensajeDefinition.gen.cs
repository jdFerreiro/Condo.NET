using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace CondoNet.Financial.Infrastructure.Mensaje.ContractDefinition
{


    public partial class MensajeDeployment : MensajeDeploymentBase
    {
        public MensajeDeployment() : base(BYTECODE) { }
        public MensajeDeployment(string byteCode) : base(byteCode) { }
    }

    public class MensajeDeploymentBase : ContractDeploymentMessage
    {
        public static string BYTECODE = "";
        public MensajeDeploymentBase() : base(BYTECODE) { }
        public MensajeDeploymentBase(string byteCode) : base(byteCode) { }
        [Parameter("string", "mensajeInicial", 1)]
        public virtual string MensajeInicial { get; set; } = null!;
    }

    public partial class GuardarMensajeFunction : GuardarMensajeFunctionBase { }

    [Function("guardarMensaje")]
    public class GuardarMensajeFunctionBase : FunctionMessage
    {
        [Parameter("string", "nuevoMensaje", 1)]
        public virtual string NuevoMensaje { get; set; } = null!;
    }

    public partial class LeerMensajeFunction : LeerMensajeFunctionBase { }

    [Function("leerMensaje", "string")]
    public class LeerMensajeFunctionBase : FunctionMessage
    {

    }

    public partial class OwnerFunction : OwnerFunctionBase { }

    [Function("owner", "address")]
    public class OwnerFunctionBase : FunctionMessage
    {

    }



    public partial class LeerMensajeOutputDTO : LeerMensajeOutputDTOBase { }

    [FunctionOutput]
    public class LeerMensajeOutputDTOBase : IFunctionOutputDTO
    {
        [Parameter("string", "", 1)]
        public virtual string ReturnValue1 { get; set; } = null!;
    }

    public partial class OwnerOutputDTO : OwnerOutputDTOBase { }

    [FunctionOutput]
    public class OwnerOutputDTOBase : IFunctionOutputDTO
    {
        [Parameter("address", "", 1)]
        public virtual string ReturnValue1 { get; set; } = null!;
    }
}
