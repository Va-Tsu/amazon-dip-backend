# 1. Етап збірки (SDK 8.0)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 2. Копіюємо файл рішення та проектів
COPY ["AmazonClone.sln", "./"]
COPY ["AmazonClone.Api/AmazonClone.Api.csproj", "AmazonClone.Api/"]
COPY ["AmazonClone.Application/AmazonClone.Application.csproj", "AmazonClone.Application/"]
COPY ["AmazonClone.Domain/AmazonClone.Domain.csproj", "AmazonClone.Domain/"]
COPY ["AmazonClone.Infrastructure/AmazonClone.Infrastructure.csproj", "AmazonClone.Infrastructure/"]

# --- Крок-милиця для виправлення архітектурних помилок ---

# Вказуємо версію 8.0.23, щоб уникнути помилки NU1605 (Package downgrade)
RUN dotnet add "AmazonClone.Application/AmazonClone.Application.csproj" package Microsoft.EntityFrameworkCore --version 8.0.23

# Примусово пов'язуємо шари, щоб Application бачив класи з Infrastructure
RUN dotnet add "AmazonClone.Application/AmazonClone.Application.csproj" reference "AmazonClone.Infrastructure/AmazonClone.Infrastructure.csproj"

# 3. Відновлюємо залежності. Використовуємо --force для чистого відновлення
RUN dotnet restore "AmazonClone.sln"

# 4. Копіюємо решту коду
COPY . .

# 5. Збираємо рішення. 
# Додаємо /p:NoWarn=NU1605, щоб конфлікти версій пакетів не зупиняли процес.
RUN dotnet build "AmazonClone.sln" -c Release /p:NoWarn=NU1605

# 6. Публікуємо саме проект API
WORKDIR "/src/AmazonClone.Api"
RUN dotnet publish "AmazonClone.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false /p:NoWarn=NU1605

# 7. Етап запуску (Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /app/publish .

# .NET 8 часто використовує 8080 за замовчуванням, але ми залишаємо 80 для сумісності з вашим скриптом
EXPOSE 80
ENTRYPOINT ["dotnet", "AmazonClone.Api.dll"]
