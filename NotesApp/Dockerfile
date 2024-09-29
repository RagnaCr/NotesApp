# Используем .NET SDK для сборки приложения
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем файлы .csproj и восстанавливаем зависимости
COPY ./NotesApp/*.csproj ./
RUN dotnet restore

# Копируем все исходные файлы и публикуем проект
COPY ./NotesApp ./
RUN dotnet publish -c Release -o /app -v diag

# Используем более легкий образ .NET для выполнения приложения
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Копируем опубликованные файлы из стадии сборки
COPY --from=build /app .

# Открываем порт 80 для работы приложения в контейнере
EXPOSE 80
 
# Запускаем приложение
ENTRYPOINT ["dotnet", "NotesApp.dll"]