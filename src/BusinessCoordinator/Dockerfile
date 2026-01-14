FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/BusinessCoordinator/BusinessCoordinator.csproj", "src/BusinessCoordinator/"]
COPY ["src/DaprSaga.Shared/DaprSaga.Shared.csproj", "src/DaprSaga.Shared/"]
RUN dotnet restore "src/BusinessCoordinator/BusinessCoordinator.csproj"
COPY . .
WORKDIR "/src/src/BusinessCoordinator"
RUN dotnet publish "BusinessCoordinator.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "BusinessCoordinator.dll"]
