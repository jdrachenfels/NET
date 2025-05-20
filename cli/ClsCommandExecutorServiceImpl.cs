using System;
using System.Threading.Tasks;
using Grpc.Core;
using ClsLib;

namespace NET.Cli
{
    /// <summary>
    /// gRPC-Service-Implementierung für CommandExecutor im CLI-Projekt.
    /// Nutzt ClsCustomCommands zur Ausführung des übergebenen Commands.
    /// </summary>
    public class CommandExecutorServiceImpl : CommandExecutor.CommandExecutorBase
    {
        private readonly ClsCustomCommands _executor = new ClsCustomCommands();

        /// <summary>
        /// Führt den Command aus und gibt das Ergebnis als String zurück.
        /// </summary>
        public override Task<CommandReply> Execute(CommandRequest request, ServerCallContext context)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Command))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Command darf nicht leer sein."));

            string result = ""; //_executor.Execute(request.Command);
            var reply = new CommandReply { Output = result };
            return Task.FromResult(reply);
        }
    }
}
