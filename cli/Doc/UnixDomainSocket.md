# Installing and Managing the CommandExecutor Daemon via systemd

This guide covers how to run your .NET CLI gRPC server over a Unix Domain Socket as a managed systemd service on Linux.

---

## 1. Create the systemd Unit File

Create a file at `/etc/systemd/system/command-executor.service` with the following contents:

```ini
[Unit]
Description=CommandExecutor gRPC Daemon
After=network.target

[Service]
Type=notify
ExecStart=/usr/bin/dotnet /path/to/NET.Cli.dll
WorkingDirectory=/path/to
KillSignal=SIGINT
TimeoutStopSec=30
Restart=on-failure

[Install]
WantedBy=multi-user.target
```

* **Type=notify** (or `simple`) – service startup notification.
* **ExecStart** – path to the published .NET executable or DLL.
* **WorkingDirectory** – directory containing your application.
* **KillSignal=SIGINT** – instructs systemd to send SIGINT on `stop`, triggering graceful shutdown.
* **TimeoutStopSec=30** – wait up to 30 s for clean shutdown before forcing termination.
* **Restart=on-failure** – automatically restart on unexpected exits.

---

## 2. Enable and Start the Service

Reload systemd to pick up the new unit file, then enable and start your service:

```bash
sudo systemctl daemon-reload
sudo systemctl enable command-executor
sudo systemctl start command-executor
```

* **enable**: starts on boot.
* **start**: begins the service now.

---

## 3. Stopping and Checking Status

To stop the service gracefully:

```bash
sudo systemctl stop command-executor
```

Systemd will send SIGINT, your app’s shutdown handlers fire, and the socket is removed.

To verify status and logs:

```bash
sudo systemctl status command-executor
journalctl -u command-executor
```

---

Your `ClsCommandServer` will now run as a proper Linux daemon, automatically managed by systemd, handling both startup and clean shutdown via SIGINT.
