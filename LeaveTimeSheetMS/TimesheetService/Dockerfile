FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5004

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["TimesheetService/TimesheetService.csproj", "TimesheetService/"]
COPY ["Shared.Events/Shared.Events.csproj", "Shared.Events/"]
RUN dotnet restore "TimesheetService/TimesheetService.csproj"
COPY . .
WORKDIR "/src/TimesheetService"
RUN dotnet publish "TimesheetService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TimesheetService.dll"]