FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS base
RUN apt-get update && apt-get install -y libgssapi-krb5-2 && rm -rf /var/lib/apt/lists/*
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copy all csproj files and restore
COPY src/ClaimFlow.Api/ClaimFlow.Api.csproj src/ClaimFlow.Api/
COPY src/ClaimFlow.Application/ClaimFlow.Application.csproj src/ClaimFlow.Application/
COPY src/ClaimFlow.Infrastructure/ClaimFlow.Infrastructure.csproj src/ClaimFlow.Infrastructure/
COPY src/ClaimFlow.Domain/ClaimFlow.Domain.csproj src/ClaimFlow.Domain/
RUN dotnet restore src/ClaimFlow.Api/ClaimFlow.Api.csproj

# Copy everything and publish
COPY src/ src/
RUN dotnet publish src/ClaimFlow.Api/ClaimFlow.Api.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ClaimFlow.Api.dll"]
