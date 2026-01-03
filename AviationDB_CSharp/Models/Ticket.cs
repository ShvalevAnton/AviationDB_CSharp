// file name: Ticket.cs
// file content begin
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace AviationDB_CSharp.Models
{
    /// <summary>
    /// Модель для таблицы tickets
    /// </summary>
    [Table("tickets", Schema = "bookings")]
    public class Ticket
    {
        /// <summary>
        /// Номер билета (первичный ключ)
        /// </summary>
        [Key]
        [Column("ticket_no")]
        [StringLength(13)]
        public string TicketNo { get; set; } = string.Empty;

        /// <summary>
        /// Номер бронирования (внешний ключ)
        /// </summary>
        [Column("book_ref")]
        [StringLength(6)]
        public string BookRef { get; set; } = string.Empty;

        /// <summary>
        /// Идентификатор пассажира
        /// </summary>
        [Column("passenger_id")]
        [StringLength(20)]
        public string PassengerId { get; set; } = string.Empty;

        /// <summary>
        /// Имя пассажира
        /// </summary>
        [Column("passenger_name")]
        public string PassengerName { get; set; } = string.Empty;

        /// <summary>
        /// Контактные данные в формате JSONB
        /// </summary>
        [Column("contact_data", TypeName = "jsonb")]
        public string? ContactData { get; set; }

        /// <summary>
        /// Навигационное свойство к бронированию
        /// </summary>
        [ForeignKey("BookRef")]
        public virtual Booking? Booking { get; set; }

        /// <summary>
        /// Создание валидного JSON для контактных данных
        /// </summary>
        public static string CreateContactDataJson(string? phone = null, string? email = null, string? address = null)
        {
            var contactData = new Dictionary<string, string?>();

            if (!string.IsNullOrWhiteSpace(phone))
                contactData["phone"] = phone;

            if (!string.IsNullOrWhiteSpace(email))
                contactData["email"] = email;

            if (!string.IsNullOrWhiteSpace(address))
                contactData["address"] = address;

            return JsonSerializer.Serialize(contactData);
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
        /// Получение значения из JSON контактных данных
        /// </summary>
        public static string? GetValueFromContactData(string contactDataJson, string key)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(contactDataJson) || !IsValidJson(contactDataJson))
                    return null;

                using var doc = JsonDocument.Parse(contactDataJson);

                if (doc.RootElement.TryGetProperty(key, out var property))
                    return property.GetString();

                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Получение номера телефона из контактных данных
        /// </summary>
        public string? GetPhone()
        {
            return GetValueFromContactData(ContactData ?? "{}", "phone");
        }

        /// <summary>
        /// Получение email из контактных данных
        /// </summary>
        public string? GetEmail()
        {
            return GetValueFromContactData(ContactData ?? "{}", "email");
        }

        /// <summary>
        /// Получение адреса из контактных данных
        /// </summary>
        public string? GetAddress()
        {
            return GetValueFromContactData(ContactData ?? "{}", "address");
        }

        /// <summary>
        /// Форматированный вывод контактных данных
        /// </summary>
        public string GetFormattedContactData()
        {
            if (string.IsNullOrWhiteSpace(ContactData) || !IsValidJson(ContactData))
                return "Нет контактных данных";

            var phone = GetPhone();
            var email = GetEmail();
            var address = GetAddress();

            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(phone))
                parts.Add($"Тел: {phone}");

            if (!string.IsNullOrWhiteSpace(email))
                parts.Add($"Email: {email}");

            if (!string.IsNullOrWhiteSpace(address))
                parts.Add($"Адрес: {address}");

            return parts.Count > 0 ? string.Join(", ", parts) : "Нет контактных данных";
        }

        /// <summary>
        /// Отформатированные контактные данные (только для отображения)
        /// </summary>
        [NotMapped]
        public string FormattedContactData => GetFormattedContactData();
    }
}
// file content end