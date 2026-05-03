FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5003

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["LeaveService/LeaveService.csproj", "LeaveService/"]
COPY ["Shared.Events/Shared.Events.csproj", "Shared.Events/"]
RUN dotnet restore "LeaveService/LeaveService.csproj"
COPY . .
WORKDIR "/src/LeaveService"
RUN dotnet publish "LeaveService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "LeaveService.dll"]