using System;
using System.Threading.Tasks;
using Grpc.Core;
using NET.Cli;  // CommandExecutor, CommandRequest, CommandReply

namespace NET.Cli
{
    /// <summary>
    /// Client for the CommandExecutor gRPC Unix Domain Socket server.
    /// Sends a command string and prints the server response.
    /// </summary>
    class ProgramClient
    {
        static async Task Main(string[] args)
        {
            // Read socket path from args at position 0
            if (args.Length < 1)
            {
                Console.Error.WriteLine("Usage: <ProgramClient> <socketPath>");
                return;
            }
            var socketPath = args[0];

            // Create a gRPC channel over UDS (explicitly use Grpc.Core.Channel)
            var channel = new Grpc.Core.Channel($"unix:{socketPath}", ChannelCredentials.Insecure);

            // Create the client stub; fully qualify to avoid ambiguity
            var client = new CommandExecutor.CommandExecutorClient(channel);

            // Prepare the command to send
            var commandText = "command option one --option1 \"bla bla bla\"";
            var request = new CommandRequest { Command = commandText };

            try
            {
                // Send the request and await the response
                var reply = await client.ExecuteAsync(request);
                Console.WriteLine($"Server response: {reply.Output}");
            }
            catch (RpcException e)
            {
                Console.Error.WriteLine($"RPC error: {e.Status.Detail}");
            }

            // Shutdown the channel
            await channel.ShutdownAsync();
        }
    }
}
