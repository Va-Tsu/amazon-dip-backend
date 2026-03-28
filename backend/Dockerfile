# 1. Етап збірки (SDK 8.0)
# Етап збірки
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
# Копіюємо все і відновлюємо залежності
COPY . .
RUN dotnet restore

# --- Крок-милиця (Правки конфігурації) ---
# Публікуємо додаток у папку /app/publish
RUN dotnet publish -c Release -o /app/publish

# Створюємо файл налаштувань, який вимкне всі попередження автоматично для всіх проектів
RUN echo '<Project><PropertyGroup><NoWarn>$(NoWarn);NU1605;CS8618;CS0114;CS8603</NoWarn><Nullable>disable</Nullable></PropertyGroup></Project>' > Directory.Build.props

# Додаємо пакет EF Core потрібної версії
RUN dotnet add "AmazonClone.Application/AmazonClone.Application.csproj" package Microsoft.EntityFrameworkCore --version 8.0.23
# Етап запуску
FROM mcr.microsoft.com/dotnet/sdk:8.0
#FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Примусово пов'язуємо шари
RUN dotnet add "AmazonClone.Application/AmazonClone.Application.csproj" reference "AmazonClone.Infrastructure/AmazonClone.Infrastructure.csproj"
RUN dotnet tool install dotnet-ef --version 8.* --global
ENV PATH="$PATH:/root/.dotnet/tools"

# 4. Збираємо все рішення (БЕЗ аргументів -p:NoWarn, вони вже у файлі вище)
RUN dotnet build "AmazonClone.sln" -c Release
# Копіюємо ТІЛЬКИ опубліковані файли з попереднього етапу
COPY --from=build /app/publish .
COPY --from=build /src /src

# 5. Публікуємо API
WORKDIR "/src/AmazonClone.Api"
RUN dotnet publish "AmazonClone.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false
# Виправляємо Segmentation Fault (Error 139) для Linux-контейнерів
#ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
ENV ASPNETCORE_HTTP_PORTS=8080

# 6. Етап запуску (Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080

ENV ASPNETCORE_HTTP_PORTS=80
EXPOSE 80
# ПЕРЕКОНАЙТЕСЬ, що ваш файл дійсно називається Backend.dll
# Якщо назва проекту інша (наприклад AmazonClone.Api), змініть ім'я файлу нижче
#ENTRYPOINT ["dotnet", "Backend.dll"]
ENTRYPOINT ["dotnet", "AmazonClone.Api.dll"]
