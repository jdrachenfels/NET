# Secure Mail Starter Pack

Dieses Paket enthält:
- **SecureMailHandler**: Exim-Pipe-Handler, der E-Mails ins sichere Verzeichnis schreibt und eine Portal-Benachrichtigung sendet.
- **SecurePortal**: Razor Pages App (de/en), Postgres (8.4-kompatibel), vorbereitete Repositories mit Prepared Statements, Syslog.
- **ClsLib**: `ClsSyslog.cs` plus deine bestehende `ClsIniFile.cs`.

## Schnellstart

```bash
git clone https://github.com/jdrachenfels/NET.git
cd NET
# Entpacke dieses Starter Pack hierher (Inhalt des ZIPs in dieses Verzeichnis kopieren)

dotnet restore
dotnet build

# Handler ausführen (liest Rohmail von stdin)
# cat mail.eml | dotnet run --project SecureMailHandler -- drachenfels.de johannes demo1.drachenfels.de 1ABC-123 20250101120000Z

# Portal starten
dotnet run --project SecurePortal/SecurePortal.Web
```

## Konfiguration
Die INI-Dateien werden **nicht eingecheckt** und bei Erststart mit Defaults erzeugt:
- Handler: `/etc/secure-handler.ini`
- Portal:  `/etc/secure-portal.ini`

Du kannst die Pfade via Env überschreiben:
- `SECURE_HANDLER_INI`
- `SECURE_PORTAL_INI`
