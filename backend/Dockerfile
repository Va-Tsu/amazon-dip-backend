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

# Додаємо пакет EF Core конкретної версії 8.x (сумісної з вашим SDK)
RUN dotnet add "AmazonClone.Application/AmazonClone.Application.csproj" package Microsoft.EntityFrameworkCore --version 8.0.0

# ПРИМУСОВО пов'язуємо Application з Infrastructure. 
# Це виправить помилку "Infrastructure does not exist in the namespace AmazonClone"
RUN dotnet add "AmazonClone.Application/AmazonClone.Application.csproj" reference "AmazonClone.Infrastructure/AmazonClone.Infrastructure.csproj"

# 3. Відновлюємо залежності
RUN dotnet restore "AmazonClone.sln"

# 4. Копіюємо решту коду
COPY . .

# 5. Збираємо все рішення
RUN dotnet build "AmazonClone.sln" -c Release

# 6. Публікуємо саме проект API
WORKDIR "/src/AmazonClone.Api"
RUN dotnet publish "AmazonClone.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 7. Етап запуску (Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Копіюємо результат збірки
COPY --from=build /app/publish .

EXPOSE 80
ENTRYPOINT ["dotnet", "AmazonClone.Api.dll"]
