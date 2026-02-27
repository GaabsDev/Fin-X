# Dockerfile para FinX.Script
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copiar arquivo de projeto e restaurar dependências
COPY ["FinX.Script/scripts.csproj", "FinX.Script/"]
RUN dotnet restore "FinX.Script/scripts.csproj"

# Copiar código fonte dos scripts
COPY ["FinX.Script/", "FinX.Script/"]

# Copiar modelos compartilhados da API
COPY ["FinX.Api/Models/", "FinX.Api/Models/"]

WORKDIR "/src/FinX.Script"
RUN dotnet build "scripts.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "scripts.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Variáveis de ambiente
ENV DOTNET_ENVIRONMENT=Production

# Comando padrão - mostra help
ENTRYPOINT ["dotnet", "scripts.dll", "help"]