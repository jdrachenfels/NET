using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using ClsLib;

namespace NET.Cli
{
    /// <summary>
    /// gRPC-Service-Implementierung für CommandExecutor.
    /// Nutzt ClsCustomCommands.Execute, welches eine Liste von Strings zurückgibt.
    /// </summary>
    public class CommandExecutorServiceImpl : CommandExecutor.CommandExecutorBase
    {
        private readonly ClsCustomCommands _executor = new ClsCustomCommands();

        /// <summary>
        /// Führt den Command aus und gibt eine Liste von Ergebnissen zurück.
        /// </summary>
        public override Task<CommandReply> Execute(CommandRequest request, ServerCallContext context)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Command))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Command must not be empty."));

            // Execute liefert nun eine Liste von Strings
            List<string> results = _executor.Execute(request.Command);

            // Zusammenpacken in den CommandReply (mit repeated string Output)
            var reply = new CommandReply();
            reply.Output = string.Join(Environment.NewLine, results);
            return Task.FromResult(reply);
        }
    }
}
