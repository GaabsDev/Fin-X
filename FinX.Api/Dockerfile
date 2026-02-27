# Dockerfile para FinX.Api
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 5006
EXPOSE 5007

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copiar arquivo de projeto e restaurar dependências
COPY ["FinX.Api/FinX.Api.csproj", "FinX.Api/"]
RUN dotnet restore "FinX.Api/FinX.Api.csproj"

# Copiar código fonte e compilar
COPY . .
WORKDIR "/src/FinX.Api"
RUN dotnet build "FinX.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "FinX.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Variáveis de ambiente para produção
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:5006

# Comando de inicialização
ENTRYPOINT ["dotnet", "FinX.Api.dll"]