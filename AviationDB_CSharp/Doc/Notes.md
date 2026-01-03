# Объяснение каждого пакета:

	1. Npgsql.EntityFrameworkCore.PostgreSQL (версия 10.0.0)
	
	Что делает: Основной пакет для работы Entity Framework с PostgreSQL
	Зачем нужен: Преобразует C# код в SQL запросы, управляет подключениями	
	Альтернативные версии: Можно использовать 10.0.0 для .NET 10

	2. Microsoft.EntityFrameworkCore.Design (версия 10.0.0)
	
	Что делает: Инструменты для миграций базы данных
	Зачем нужен: Позволяет создавать миграции командой dotnet ef
	Важно: Должен быть той же версии, что и основной EF

	3. Microsoft.Extensions.Configuration.Json (версия 10.0.0)

	Что делает: Чтение JSON конфигураций
	Зачем нужен: Для загрузки строк подключения из appsettings.json
	Альтернатива: Можно хранить строку подключения прямо в коде

	4. Microsoft.Extensions.DependencyInjection (версия 10.0.0)
	
	Что делает: Внедрение зависимостей
	Зачем нужен: Для лучшей организации кода (опционально)

## Настроить файл конфигурации подключения (appsettings)

	Проверьте свойства файла
	В Solution Explorer:

	Правой кнопкой на appsettings.json → Properties

	Убедитесь, что:

	Build Action: None

	Copy to Output Directory: Copy always или Copy if newer