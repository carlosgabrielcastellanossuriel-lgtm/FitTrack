# Etapa de compilación: usa el SDK para publicar la API y el Enviador en Release.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish FitTrack.Api/FitTrack.Api.csproj -c Release -o /app/api
RUN dotnet publish FitTrack.Enviador/FitTrack.Enviador.csproj -c Release -o /app/enviador

# Etapa final "api": solo el runtime de ASP.NET con los archivos ya publicados.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS api
WORKDIR /app
COPY --from=build /app/api .
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "FitTrack.Api.dll"]

# Etapa final "enviador": solo el runtime de .NET con los archivos ya publicados.
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS enviador
WORKDIR /app
COPY --from=build /app/enviador .
