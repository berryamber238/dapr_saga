FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/Service.Genesis/Service.Genesis.csproj", "src/Service.Genesis/"]
COPY ["src/DaprSaga.Shared/DaprSaga.Shared.csproj", "src/DaprSaga.Shared/"]
RUN dotnet restore "src/Service.Genesis/Service.Genesis.csproj"
COPY . .
WORKDIR "/src/src/Service.Genesis"
RUN dotnet publish "Service.Genesis.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Service.Genesis.dll"]
