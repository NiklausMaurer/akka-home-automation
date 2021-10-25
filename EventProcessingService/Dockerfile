FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["EventProcessingService/EventProcessingService.csproj", "EventProcessingService/"]
RUN dotnet restore "EventProcessingService/EventProcessingService.csproj"
COPY . .
WORKDIR "/src/EventProcessingService"
RUN dotnet build "EventProcessingService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EventProcessingService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EventProcessingService.dll"]
