# Структура проекта

	AviationDB_Console_EF/
	├── Models/
	│   └── Aircraft.cs
	├── Data/
	│   └── ApplicationDbContext.cs
	├── Services/
	│   └── AircraftService.cs
	├── UI/                           ← Новая папка
	│   ├── MenuService.cs
	│   ├── AircraftMenuService.cs
	│   └── MainMenuService.cs
	├── Config/
	│   └── AppConfig.cs
	├── appsettings.json
	├── Program.cs                    ← Упрощенный
	└── AircraftCRUD.csproj