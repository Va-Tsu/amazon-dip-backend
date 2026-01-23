# Встановлюємо .NET SDK
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build

WORKDIR /app

# Копіюємо csproj та відновлюємо залежності
COPY *.csproj ./
RUN dotnet restore

# Копіюємо решту файлів
COPY . ./

# Будуємо проект
RUN dotnet build -c Release -o /app/build

# Створюємо runtime образ
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build /app/build .

EXPOSE 5000

# Запуск API
CMD ["dotnet", "MyApi.dll"]
