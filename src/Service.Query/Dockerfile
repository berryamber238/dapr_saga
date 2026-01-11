FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/Service.Query/Service.Query.csproj", "src/Service.Query/"]
COPY ["src/DaprDemo.Shared/DaprDemo.Shared.csproj", "src/DaprDemo.Shared/"]
RUN dotnet restore "src/Service.Query/Service.Query.csproj"
COPY . .
WORKDIR "/src/src/Service.Query"
RUN dotnet publish "Service.Query.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Service.Query.dll"]
