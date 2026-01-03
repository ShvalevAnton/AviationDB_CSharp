using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace AviationDB_CSharp.Data
{
    /// <summary>
    /// Конвертер для DateTime в UTC для PostgreSQL
    /// </summary>
    public class DateTimeToUtcConverter : ValueConverter<DateTime, DateTime>
    {
        public DateTimeToUtcConverter()
            : base(
                v => v.ToUniversalTime(), // Конвертация в базу данных (в UTC)
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc)) // Конвертация из базы данных
        {
        }
    }
}