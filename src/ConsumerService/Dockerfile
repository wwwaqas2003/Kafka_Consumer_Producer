# Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8081

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ConsumerService.csproj", "./"]
RUN dotnet restore "ConsumerService.csproj"
COPY . .
RUN dotnet build "ConsumerService.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "ConsumerService.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ConsumerService.dll"]
