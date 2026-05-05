# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
EXPOSE 8080
EXPOSE 8081

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/CondoNet.Engagement.Api/CondoNet.Engagement.Api.csproj", "src/CondoNet.Engagement.Api/"]
RUN dotnet restore "./src/CondoNet.Engagement.Api/CondoNet.Engagement.Api.csproj"
COPY . .
WORKDIR "/src/src/CondoNet.Engagement.Api"
RUN dotnet build "./CondoNet.Engagement.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build	
# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CondoNet.Engagement.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false
# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
# ... después del COPY --from=publish ...

# Volvemos a root para asegurar la copia con permisos
USER root

# Copiamos con el permiso de ejecución ya asignado y limpiamos el formato Windows
COPY --chmod=755 wait-for-it.sh ./wait-for-it.sh
RUN sed -i 's/\r$//' ./wait-for-it.sh


# Crear directorio para certificados y copiar el certificado (el volumen lo monta el host)
RUN mkdir -p /https

# Cambiamos al usuario de la aplicación
USER $APP_UID

# Importante: Usa el nombre correcto del servicio RabbitMQ (según tu imagen anterior es condonet_rabbitmq)
ENTRYPOINT ["./wait-for-it.sh", "condonet_rabbitmq:5672", "--", "dotnet", "CondoNet.Engagement.Api.dll"]
