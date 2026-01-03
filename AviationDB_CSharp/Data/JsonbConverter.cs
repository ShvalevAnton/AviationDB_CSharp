using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace AviationDB_CSharp.Data
{
    /// <summary>
    /// Конвертер для преобразования между JSON string в C# и jsonb в PostgreSQL
    /// </summary>
    public class JsonbConverter : ValueConverter<string, string>
    {
        public JsonbConverter()
            : base(
                v => v, // Конвертация в базу данных
                v => v, // Конвертация из базы данных
                convertsNulls: false)
        {
        }
    }
}