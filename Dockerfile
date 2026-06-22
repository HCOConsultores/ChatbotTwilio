# Usa la imagen oficial de .NET como la base para ejecutar la aplicación
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
ENV ASPNETCORE_URLS="http://+:80"
ENV ASPNETCORE_ENVIRONMENT=Production

# Usa la imagen de .NET SDK para construir la aplicación
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["chatBotTwilio.csproj", "./"]
RUN dotnet restore "./chatBotTwilio.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "chatBotTwilio.csproj" -c Release -o /app/build

# Publica la aplicación
FROM build AS publish
RUN dotnet publish "chatBotTwilio.csproj" -c Release -o /app/publish

# Usa la imagen base para ejecutar la aplicación
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "chatBotTwilio.dll"]
