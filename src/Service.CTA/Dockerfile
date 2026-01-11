FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/Service.CTA/Service.CTA.csproj", "src/Service.CTA/"]
COPY ["src/DaprDemo.Shared/DaprDemo.Shared.csproj", "src/DaprDemo.Shared/"]
RUN dotnet restore "src/Service.CTA/Service.CTA.csproj"
COPY . .
WORKDIR "/src/src/Service.CTA"
RUN dotnet publish "Service.CTA.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Service.CTA.dll"]
