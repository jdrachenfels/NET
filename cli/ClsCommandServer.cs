using System;
using System.IO;
using System.Threading.Tasks;
using Grpc.Core;
using ClsLib;

namespace NET.Cli
{
    /// <summary>
    /// Enthält den Unix Domain Socket gRPC-Server für CommandExecutor im CLI-Projekt.
    /// </summary>
    public class ClsCommandServer
    {
        private readonly Server _server;
        private readonly string _socketPath;

        /// <summary>
        /// Initialisiert den Server mit dem Socket-Pfad und bindet das CommandExecutor-Service.
        /// </summary>
        /// <param name="socketPath">Filesystem-Pfad für den Unix Domain Socket.</param>
        public ClsCommandServer(string socketPath)
        {
            if (string.IsNullOrWhiteSpace(socketPath))
                throw new ArgumentException("Socket path must not be empty.", nameof(socketPath));

            _socketPath = socketPath;
            var dir = Path.GetDirectoryName(socketPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            if (File.Exists(socketPath))
                File.Delete(socketPath);

            // gRPC-Service registrieren
            var serviceDef = CommandExecutor.BindService(new CommandExecutorServiceImpl());

            // Server auf Unix Domain Socket konfigurieren
            _server = new Server
            {
                Services = { serviceDef },
                Ports = { new ServerPort($"unix:{socketPath}", 0, ServerCredentials.Insecure) }
            };
        }

        /// <summary>
        /// Startet den gRPC-Server.
        /// </summary>
        public void Start()
        {
            if (File.Exists(_socketPath)) File.Delete(_socketPath);
            _server.Start();
            Console.WriteLine($"CommandExecutor gRPC server is running on Unix socket: {_socketPath}");
        }

        /// <summary>
        /// Stoppt den gRPC-Server und entfernt die Socket-Datei.
        /// </summary>
        public async Task StopAsync()
        {
            await _server.ShutdownAsync();
            if (File.Exists(_socketPath)) File.Delete(_socketPath);
            Console.WriteLine("CommandExecutor gRPC server stopped and socket removed.");
        }
    }
}
