# Stage 1 — build & publish
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY SaraDrive.sln ./
COPY src/SaraDrive.Domain/SaraDrive.Domain.csproj                 src/SaraDrive.Domain/
COPY src/SaraDrive.Application/SaraDrive.Application.csproj       src/SaraDrive.Application/
COPY src/SaraDrive.Infrastructure/SaraDrive.Infrastructure.csproj src/SaraDrive.Infrastructure/
COPY src/SaraDrive.Api/SaraDrive.Api.csproj                       src/SaraDrive.Api/

RUN dotnet restore

COPY . .

RUN dotnet publish src/SaraDrive.Api/SaraDrive.Api.csproj \
    -c Release -o /app/publish --no-restore

# Stage 2 — runtime image (smaller, no SDK)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "SaraDrive.Api.dll"]
