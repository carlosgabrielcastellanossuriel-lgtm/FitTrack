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

# Etapa de compilación del frontend: instala dependencias y genera la carpeta dist.
FROM node:22-alpine AS web-build
WORKDIR /web
COPY FitTrack.Web/package*.json ./
RUN npm ci
COPY FitTrack.Web/ .
ARG VITE_API_URL
ENV VITE_API_URL=$VITE_API_URL
RUN npm run build

# Etapa final "web": sirve la carpeta dist del frontend con Nginx.
FROM nginx:alpine AS web
COPY FitTrack.Web/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=web-build /web/dist /usr/share/nginx/html
EXPOSE 80
