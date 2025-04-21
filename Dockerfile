# Etapa de compilacion
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar solucion y proyecto
COPY Notificaciones.sln .
COPY NotificacionesService/ NotificacionesService/

# Restaurar dependencias
WORKDIR /src/NotificacionesService
RUN dotnet restore

# Publicar la app
RUN dotnet publish -c Release -o /app/publish

# Etapa de ejecucion
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Expone el puerto interno estandar
EXPOSE 8080

ENTRYPOINT ["dotnet", "NotificacionesService.dll"]


