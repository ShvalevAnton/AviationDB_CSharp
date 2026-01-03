using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace AviationDB_CSharp.Models
{
    /// <summary>
    /// Модель для таблицы aircrafts_data
    /// </summary>
    [Table("aircrafts_data", Schema = "bookings")]
    public class Aircraft
    {
        /// <summary>
        /// Код самолета (IATA код). Первичный ключ
        /// </summary>
        [Key]
        [Column("aircraft_code")]
        [StringLength(3)]
        public string AircraftCode { get; set; } = string.Empty;

        /// <summary>
        /// Модель самолета в формате JSONB
        /// </summary>
        [Column("model", TypeName = "jsonb")]
        public string Model { get; set; } = "{}";

        /// <summary>
        /// Максимальная дальность полета в километрах
        /// </summary>
        [Column("range")]
        [Range(1, int.MaxValue, ErrorMessage = "Дальность должна быть больше 0")]
        public int Range { get; set; }

        /// <summary>
        /// Создание валидного JSON из текста
        /// </summary>
        public static string CreateJsonFromText(string text)
        {
            var jsonObject = new
            {
                en = text ?? "Unknown",
                ru = text ?? "Неизвестно"
            };

            return JsonSerializer.Serialize(jsonObject);
        }

        /// <summary>
        /// Валидация JSON
        /// </summary>
        public static bool IsValidJson(string json)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(json))
                    return false;

                JsonDocument.Parse(json);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }

        /// <summary>
        /// Получение текста модели из JSON
        /// </summary>
        public string GetModelText()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Model) || !IsValidJson(Model))
                    return "Неизвестная модель";

                using var doc = JsonDocument.Parse(Model);

                if (doc.RootElement.TryGetProperty("en", out var enProperty))
                    return enProperty.GetString() ?? "Unknown";

                if (doc.RootElement.TryGetProperty("ru", out var ruProperty))
                    return ruProperty.GetString() ?? "Неизвестно";

                return Model;
            }
            catch
            {
                return Model;
            }
        }
    }
}