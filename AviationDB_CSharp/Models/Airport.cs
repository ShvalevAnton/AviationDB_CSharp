 using NetTopologySuite.Geometries;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace AviationDB_CSharp.Models
{
    /// <summary>
    /// Модель для таблицы airports_data
    /// </summary>
    [Table("airports_data", Schema = "bookings")]
    public class Airport
    {
        /// <summary>
        /// Код аэропорта (IATA код). Первичный ключ
        /// </summary>
        [Key]
        [Column("airport_code")]
        [StringLength(3)]
        public string AirportCode { get; set; } = string.Empty;

        /// <summary>
        /// Название аэропорта в формате JSONB
        /// </summary>
        [Column("airport_name", TypeName = "jsonb")]
        public string AirportName { get; set; } = "{}";

        /// <summary>
        /// Город в формате JSONB
        /// </summary>
        [Column("city", TypeName = "jsonb")]
        public string City { get; set; } = "{}";

        /// <summary>
        /// Координаты аэропорта (геометрия Point)
        /// </summary>
        [Column("coordinates")]
        public Point Coordinates { get; set; } = null!;

        /// <summary>
        /// Часовой пояс аэропорта
        /// </summary>
        [Column("timezone")]
        public string Timezone { get; set; } = string.Empty;

        /// <summary>
        /// Долгота (не маппируется в БД)
        /// </summary>
        [NotMapped]
        public double? Longitude
        {
            get => GetLongitude();
            set
            {
                if (value.HasValue && Coordinates != null)
                {
                    // Создаем новую точку с обновленной долготой
                    var latitude = Coordinates?.Y ?? 0;
                    Coordinates = CreatePoint(value.Value, latitude);
                }
            }
        }

        /// <summary>
        /// Широта (не маппируется в БД)
        /// </summary>
        [NotMapped]
        public double? Latitude
        {
            get => GetLatitude();
            set
            {
                if (value.HasValue && Coordinates != null)
                {
                    // Создаем новую точку с обновленной широтой
                    var longitude = Coordinates?.X ?? 0;
                    Coordinates = CreatePoint(longitude, value.Value);
                }
            }
        }

        /// <summary>
        /// Текстовое представление координат
        /// </summary>
        [NotMapped]
        public string CoordinatesText =>
            Longitude.HasValue && Latitude.HasValue
                ? $"{Longitude:F4}°, {Latitude:F4}°"
                : "Неизвестно";

        /// <summary>
        /// Создание валидного JSON из текста для локализованных полей
        /// </summary>
        public static string CreateLocalizedJson(string text)
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
        /// Получение текста из локализованного JSON
        /// </summary>
        public static string GetTextFromJson(string json, string language = "en")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(json) || !IsValidJson(json))
                    return "Unknown";

                using var doc = JsonDocument.Parse(json);

                if (doc.RootElement.TryGetProperty(language, out var property))
                    return property.GetString() ?? "Unknown";

                // Если запрошенный язык не найден, пробуем английский
                if (language != "en" && doc.RootElement.TryGetProperty("en", out var enProperty))
                    return enProperty.GetString() ?? "Unknown";

                // Если английского нет, возвращаем первый найденный
                foreach (var prop in doc.RootElement.EnumerateObject())
                {
                    return prop.Value.GetString() ?? "Unknown";
                }

                return json;
            }
            catch
            {
                return json;
            }
        }

        /// <summary>
        /// Получение названия аэропорта на английском
        /// </summary>
        public string GetAirportNameEn() => GetTextFromJson(AirportName, "en");

        /// <summary>
        /// Получение названия аэропорта на русском
        /// </summary>
        public string GetAirportNameRu() => GetTextFromJson(AirportName, "ru");

        /// <summary>
        /// Получение города на английском
        /// </summary>
        public string GetCityEn() => GetTextFromJson(City, "en");

        /// <summary>
        /// Получение города на русском
        /// </summary>
        public string GetCityRu() => GetTextFromJson(City, "ru");

        /// <summary>
        /// Получение широты из координат
        /// </summary>
        public double? GetLatitude() => Coordinates?.Y;

        /// <summary>
        /// Получение долготы из координат
        /// </summary>
        public double? GetLongitude() => Coordinates?.X;

        /// <summary>
        /// Создание точки из координат
        /// </summary>
        public static Point CreatePoint(double longitude, double latitude)
        {
            return new Point(longitude, latitude) { SRID = 4326 };
        }
    }
}