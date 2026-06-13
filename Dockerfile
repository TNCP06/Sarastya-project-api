# Stage 1 — build & publish
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ProjektaskApi.sln ./
COPY src/Projektask.Domain/Projektask.Domain.csproj          src/Projektask.Domain/
COPY src/Projektask.Application/Projektask.Application.csproj src/Projektask.Application/
COPY src/Projektask.Infrastructure/Projektask.Infrastructure.csproj src/Projektask.Infrastructure/
COPY src/Projektask.Api/Projektask.Api.csproj                src/Projektask.Api/

RUN dotnet restore

COPY . .

RUN dotnet publish src/Projektask.Api/Projektask.Api.csproj \
    -c Release -o /app/publish --no-restore

# Stage 2 — runtime image (lebih kecil, tanpa SDK)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Projektask.Api.dll"]
