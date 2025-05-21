using System;
using System.Threading.Tasks;
using Grpc.Core;

namespace NET.Cli
{
    /// <summary>
    /// Provides a gRPC server over TCP for the CommandExecutor service.
    /// </summary>
    public class ClsCommandServerTcp
    {
        private readonly Server _server;

        /// <summary>
        /// Initializes a new TCP-based gRPC server.
        /// </summary>
        /// <param name="host">The host to bind (e.g., "localhost" or "0.0.0.0").</param>
        /// <param name="port">The TCP port to listen on.</param>
        public ClsCommandServerTcp(string host, int port)
        {
            if (string.IsNullOrWhiteSpace(host))
                throw new ArgumentException("Host must not be empty.", nameof(host));
            if (port <= 0 || port > 65535)
                throw new ArgumentOutOfRangeException(nameof(port), "Port must be between 1 and 65535.");

            // Register the gRPC service
            var serviceDef = CommandExecutor.BindService(new CommandExecutorServiceImpl());

            // Configure the server to listen on TCP
            _server = new Server
            {
                Services = { serviceDef },
                Ports = { new ServerPort(host, port, ServerCredentials.Insecure) }
            };
        }

        /// <summary>
        /// Starts the gRPC server.
        /// </summary>
        public void Start()
        {
            _server.Start();

            // Display bound ports
            foreach (var p in _server.Ports)
            {
                Console.WriteLine($"CommandExecutor gRPC TCP server is listening on {p.Host}:{p.Port}");
            }
        }

        /// <summary>
        /// Stops the gRPC server.
        /// </summary>
        public async Task StopAsync()
        {
            await _server.ShutdownAsync();
            Console.WriteLine("CommandExecutor gRPC TCP server stopped.");
        }
    }
}
