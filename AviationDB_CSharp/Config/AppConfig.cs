using Microsoft.Extensions.Configuration;
using System.IO;

namespace AviationDB_CSharp.Config
{
    /// <summary>
    /// Класс для работы с конфигурацией приложения
    /// </summary>
    public static class AppConfig
    {
        private static IConfiguration _configuration;

        /// <summary>
        /// Инициализация конфигурации
        /// </summary>
        public static void Initialize()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
        }

        /// <summary>
        /// Получение строки подключения к базе данных
        /// </summary>
        public static string GetConnectionString()
        {
            return _configuration.GetConnectionString("DefaultConnection");
        }
    }
}