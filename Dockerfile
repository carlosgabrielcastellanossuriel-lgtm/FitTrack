# Etapa de compilación: usa el SDK para publicar la API en Release.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish FitTrack.Api/FitTrack.Api.csproj -c Release -o /app

# Etapa final: solo el runtime de ASP.NET con los archivos ya publicados.
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "FitTrack.Api.dll"]
