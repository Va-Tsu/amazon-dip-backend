# -------- BUILD STAGE --------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копіюємо solution і всі csproj
COPY AmazonClone.sln ./

COPY AmazonClone.Api/AmazonClone.Api.csproj AmazonClone.Api/
COPY AmazonClone.Application/AmazonClone.Application.csproj AmazonClone.Application/
COPY AmazonClone.Domain/AmazonClone.Domain.csproj AmazonClone.Domain/
COPY AmazonClone.Infrastructure/AmazonClone.Infrastructure.csproj AmazonClone.Infrastructure/

# Restore
RUN dotnet restore AmazonClone.sln

# Копіюємо все інше
COPY . .

# Publish API
RUN dotnet publish AmazonClone.Api/AmazonClone.Api.csproj -c Release -o /app/publish

# -------- RUNTIME STAGE --------
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 80
ENTRYPOINT ["dotnet", "AmazonClone.Api.dll"]
