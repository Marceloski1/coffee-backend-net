# Use the official .NET 8 runtime as a parent image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Use the SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY ["src/Coffee.Api/Coffee.Api.csproj", "src/Coffee.Api/"]
COPY ["src/Coffee.Application/Coffee.Application.csproj", "src/Coffee.Application/"]
COPY ["src/Coffee.Domain/Coffee.Domain.csproj", "src/Coffee.Domain/"]
COPY ["src/Coffee.Persistence/Coffee.Persistence.csproj", "src/Coffee.Persistence/"]
RUN dotnet restore "src/Coffee.Api/Coffee.Api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/src/Coffee.Api"
RUN dotnet build "Coffee.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Coffee.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Build runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create logs directory
RUN mkdir -p /app/logs

# Set environment variables
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "Coffee.Api.dll"]