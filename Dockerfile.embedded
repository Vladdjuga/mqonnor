FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/mqonnor.API/mqonnor.API.csproj", "src/mqonnor.API/"]
COPY ["src/mqonnor.Application/mqonnor.Application.csproj", "src/mqonnor.Application/"]
COPY ["src/mqonnor.Domain/mqonnor.Domain.csproj", "src/mqonnor.Domain/"]
COPY ["src/mqonnor.Infra/mqonnor.Infra.csproj", "src/mqonnor.Infra/"]
RUN dotnet restore "src/mqonnor.API/mqonnor.API.csproj"

COPY . .
RUN dotnet publish "src/mqonnor.API/mqonnor.API.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

# Install MongoDB and supervisord
RUN apt-get update && apt-get install -y --no-install-recommends \
    gnupg \
    curl \
    supervisor \
    && curl -fsSL https://www.mongodb.org/static/pgp/server-8.0.asc | gpg -o /usr/share/keyrings/mongodb-server-8.0.gpg --dearmor \
    && echo "deb [ signed-by=/usr/share/keyrings/mongodb-server-8.0.gpg ] https://repo.mongodb.org/apt/debian bookworm/mongodb-org/8.0 main" \
       > /etc/apt/sources.list.d/mongodb-org-8.0.list \
    && apt-get update && apt-get install -y --no-install-recommends mongodb-org \
    && rm -rf /var/lib/apt/lists/* \
    && mkdir -p /data/db

WORKDIR /app
COPY --from=build /app/publish .
COPY supervisord.conf /etc/supervisor/conf.d/supervisord.conf

EXPOSE 8080

ENV ConnectionStrings__MongoDB=mongodb://localhost:27017
ENV MongoDB__Database=mqonnor
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["/usr/bin/supervisord", "-c", "/etc/supervisor/conf.d/supervisord.conf"]
