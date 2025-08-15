# syntax=docker/dockerfile:1

### Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Lokale Rebex-DLLs VOR Restore/Pub bereitstellen (Pfad passt zu HintPath ..\libs\Rebex\)
COPY libs/ libs/

# Nur das Nötige kopieren
COPY ClsLib/ ClsLib/
COPY SecureMailHandler/ SecureMailHandler/

RUN dotnet restore SecureMailHandler/SecureMailHandler.csproj
RUN dotnet publish SecureMailHandler/SecureMailHandler.csproj -c Release -o /out \
    -r linux-x64 --self-contained true \
    /p:PublishSingleFile=true \
    /p:IncludeNativeLibrariesForSelfExtract=true \
    /p:DebugType=None \
    /p:UseAppHost=true

### Runtime stage (glibc aus Debian)
FROM debian:bookworm-slim AS runtime
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1

RUN apt-get update && apt-get install -y --no-install-recommends \
      ca-certificates tzdata \
    && rm -rf /var/lib/apt/lists/*

# Unprivileged User
RUN useradd -u 10001 -r -m -d /home/securemail -s /usr/sbin/nologin securemail

WORKDIR /app
COPY --from=build /out/ /app/

VOLUME ["/workdir/workspace/secure_mail", "/etc"]
USER securemail
ENTRYPOINT ["/app/SecureMailHandler"]
