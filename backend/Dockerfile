# Етап збірки
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копіюємо все і відновлюємо залежності
COPY . .
RUN dotnet restore

# Публікуємо додаток у папку /app/publish
RUN dotnet publish -c Release -o /app/publish

# Етап запуску
FROM mcr.microsoft.com/dotnet/sdk:8.0
#FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

RUN dotnet tool install dotnet-ef --version 8.* --global
ENV PATH="$PATH:/root/.dotnet/tools"

# Копіюємо ТІЛЬКИ опубліковані файли з попереднього етапу
COPY --from=build /app/publish .
#COPY --from=build /src /src

# Виправляємо Segmentation Fault (Error 139) для Linux-контейнерів
#ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
ENV ASPNETCORE_HTTP_PORTS=8080

EXPOSE 8080

# ПЕРЕКОНАЙТЕСЬ, що ваш файл дійсно називається Backend.dll
# Якщо назва проекту інша (наприклад AmazonClone.Api), змініть ім'я файлу нижче
#ENTRYPOINT ["dotnet", "Backend.dll"]
ENTRYPOINT ["dotnet", "AmazonClone.Api.dll"]