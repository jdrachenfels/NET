using System;
using System.Threading.Tasks;
using Grpc.Core;
using NET.Cli;  // for CommandExecutor, CommandRequest, CommandReply

namespace TestClient
{
    /// <summary>
    /// Client for CommandExecutor gRPC server supporting both Unix Domain Socket and TCP.
    /// Usage:
    ///   ProgramClient unix <socketPath>
    ///   ProgramClient tcp <host> <port>
    /// </summary>
    class ProgramClient
    {
        public static class GlobalVars
        {
            public static string AppName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name ?? "APP";
        }

        static async Task Main(string[] args)
        {
            bool useTcp = false;
            string socketPath = "Cli.sock:0";
            string host = "localhost";
            int port = 50001;

            // Parse arguments
            if (args.Length >= 2)
            {
                var mode = args[0].ToLowerInvariant();
                if (mode == "tcp" && args.Length >= 3 && int.TryParse(args[2], out var p))
                {
                    useTcp = true;
                    host = args[1];
                    port = p;
                }
                else if (mode == "unix")
                {
                    socketPath = args[1];
                }
                else
                {
                    ShowUsage();
                    return;
                }
            }
            else
            {
                ShowUsage();
                return;
            }

            // Create channel
            Channel channel;
            if (useTcp)
            {
                channel = new Channel(host, port, ChannelCredentials.Insecure);
                Console.WriteLine($"Connecting to TCP server at {host}:{port}");
            }
            else
            {
                channel = new Channel($"unix:{socketPath}", ChannelCredentials.Insecure);
                Console.WriteLine($"Connecting to Unix socket at {socketPath}");
            }

            // Create client and send command
            var client = new CommandExecutor.CommandExecutorClient(channel);
            var commandText = "command option one --option1 \"bla bla bla\"";
            var request = new CommandRequest { Command = commandText };

            try
            {
                var reply = await client.ExecuteAsync(request);
                Console.WriteLine($"Server response: {reply.Output}");
            }
            catch (RpcException e)
            {
                Console.Error.WriteLine($"RPC error: {e.Status.Detail}");
            }

            // Shutdown
            await channel.ShutdownAsync();
        }

        static void ShowUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine($"  {GlobalVars.AppName} unix <socketPath>");
            Console.WriteLine($"  {GlobalVars.AppName} tcp <host> <port>");
        }
    }
}
