# syntax=docker/dockerfile:1

# ---- build ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar los .csproj primero para cachear el restore (capa estable).
COPY src/FinancieroPersonal.Domain/FinancieroPersonal.Domain.csproj src/FinancieroPersonal.Domain/
COPY src/FinancieroPersonal.Application/FinancieroPersonal.Application.csproj src/FinancieroPersonal.Application/
COPY src/FinancieroPersonal.Infrastructure/FinancieroPersonal.Infrastructure.csproj src/FinancieroPersonal.Infrastructure/
COPY src/FinancieroPersonal.Api/FinancieroPersonal.Api.csproj src/FinancieroPersonal.Api/
RUN dotnet restore src/FinancieroPersonal.Api/FinancieroPersonal.Api.csproj

# Copiar el resto y publicar solo el proyecto Api (arrastra Domain/Application/Infrastructure).
COPY . .
RUN dotnet publish src/FinancieroPersonal.Api/FinancieroPersonal.Api.csproj \
    -c Release -o /app/publish /p:UseAppHost=false

# ---- runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_ENVIRONMENT=Production
# Render inyecta la variable PORT; Program.cs hace el binding a 0.0.0.0:$PORT.
EXPOSE 10000
ENTRYPOINT ["dotnet", "FinancieroPersonal.Api.dll"]
