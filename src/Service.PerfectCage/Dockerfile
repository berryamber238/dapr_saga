FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/Service.PerfectCage/Service.PerfectCage.csproj", "src/Service.PerfectCage/"]
COPY ["src/DaprDemo.Shared/DaprDemo.Shared.csproj", "src/DaprDemo.Shared/"]
RUN dotnet restore "src/Service.PerfectCage/Service.PerfectCage.csproj"
COPY . .
WORKDIR "/src/src/Service.PerfectCage"
RUN dotnet publish "Service.PerfectCage.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Service.PerfectCage.dll"]
