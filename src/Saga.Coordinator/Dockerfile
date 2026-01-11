FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/Saga.Coordinator/Saga.Coordinator.csproj", "src/Saga.Coordinator/"]
COPY ["src/DaprDemo.Shared/DaprDemo.Shared.csproj", "src/DaprDemo.Shared/"]
RUN dotnet restore "src/Saga.Coordinator/Saga.Coordinator.csproj"
COPY . .
WORKDIR "/src/src/Saga.Coordinator"
RUN dotnet publish "Saga.Coordinator.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Saga.Coordinator.dll"]
