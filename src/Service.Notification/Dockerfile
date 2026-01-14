FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/Service.Notification/Service.Notification.csproj", "src/Service.Notification/"]
COPY ["src/DaprSaga.Shared/DaprSaga.Shared.csproj", "src/DaprSaga.Shared/"]
RUN dotnet restore "src/Service.Notification/Service.Notification.csproj"
COPY . .
WORKDIR "/src/src/Service.Notification"
RUN dotnet publish "Service.Notification.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Service.Notification.dll"]
