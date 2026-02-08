## 1. Етап збірки (SDK 8.0)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 2. Попереднє відновлення залежностей
COPY ["AmazonClone.sln", "./"]
COPY ["AmazonClone.Api/AmazonClone.Api.csproj", "AmazonClone.Api/"]
COPY ["AmazonClone.Application/AmazonClone.Application.csproj", "AmazonClone.Application/"]
COPY ["AmazonClone.Domain/AmazonClone.Domain.csproj", "AmazonClone.Domain/"]
COPY ["AmazonClone.Infrastructure/AmazonClone.Infrastructure.csproj", "AmazonClone.Infrastructure/"]

RUN dotnet restore "AmazonClone.sln"

# 3. Копіюємо ВЕСЬ код
COPY . .

# --- Крок-милиця (Вносимо правки ПІСЛЯ копіювання коду) ---

# Додаємо пакет EF Core потрібної версії
RUN dotnet add "AmazonClone.Application/AmazonClone.Application.csproj" package Microsoft.EntityFrameworkCore --version 8.0.23

# Примусово пов'язуємо шари
RUN dotnet add "AmazonClone.Application/AmazonClone.Application.csproj" reference "AmazonClone.Infrastructure/AmazonClone.Infrastructure.csproj"

# 4. Збираємо все рішення
# Використовуємо NoWarn без лапок, розділяючи коди комами (стандарт для CLI)
RUN dotnet build "AmazonClone.sln" -c Release /p:NoWarn=NU1605,CS8618,CS0114

# 5. Публікуємо API
WORKDIR "/src/AmazonClone.Api"
RUN dotnet publish "AmazonClone.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false /p:NoWarn=NU1605,CS8618,CS0114

# 6. Етап запуску (Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=80
EXPOSE 80
ENTRYPOINT ["dotnet", "AmazonClone.Api.dll"]
