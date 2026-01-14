FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/Service.OutboxWorker/Service.OutboxWorker.csproj", "src/Service.OutboxWorker/"]
COPY ["src/DaprSaga.Shared/DaprSaga.Shared.csproj", "src/DaprSaga.Shared/"]
RUN dotnet restore "src/Service.OutboxWorker/Service.OutboxWorker.csproj"
COPY . .
WORKDIR "/src/src/Service.OutboxWorker"
RUN dotnet build "Service.OutboxWorker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Service.OutboxWorker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Service.OutboxWorker.dll"]
