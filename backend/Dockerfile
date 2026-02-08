# 1. Етап збірки (SDK 8.0)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 2. Копіюємо файл рішення (.sln)
COPY ["AmazonClone.sln", "./"]

# 3. Копіюємо ВСІ файли проектів (.csproj) зі збереженням структури папок
# Це необхідно, щоб команда restore побачила зв'язки між проектами
COPY ["AmazonClone.Api/AmazonClone.Api.csproj", "AmazonClone.Api/"]
COPY ["AmazonClone.Application/AmazonClone.Application.csproj", "AmazonClone.Application/"]
COPY ["AmazonClone.Domain/AmazonClone.Domain.csproj", "AmazonClone.Domain/"]
COPY ["AmazonClone.Infrastructure/AmazonClone.Infrastructure.csproj", "AmazonClone.Infrastructure/"]

# 4. Відновлюємо залежності (NuGet)
RUN dotnet restore "AmazonClone.sln"

# 5. Копіюємо решту вихідного коду
COPY . .

# 6. Публікуємо саме проект API
WORKDIR "/src/AmazonClone.Api"
RUN dotnet publish "AmazonClone.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 7. Етап запуску (Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Копіюємо результат збірки
COPY --from=build /app/publish .

# .NET 8 за замовчуванням слухає порт 8080 (або 80), 
# тому переконайтеся, що в docker-compose порти співпадають
EXPOSE 80

ENTRYPOINT ["dotnet", "AmazonClone.Api.dll"]
