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

WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "mqonnor.API.dll"]
