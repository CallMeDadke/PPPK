using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MedicalSystemApp.Json
{
    public class HrDateTimeConverter : JsonConverter<DateTime>
    {
        private static readonly string[] Formats = new[]
        {
            "dd.MM.yyyy.", "dd.MM.yyyy", "d.M.yyyy.", "d.M.yyyy",
            "yyyy-MM-dd", "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-ddTHH:mm:ssZ"
        };

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var s = reader.GetString();
            if (string.IsNullOrWhiteSpace(s)) return default;

            if (DateTime.TryParseExact(s, Formats, new CultureInfo("hr-HR"),
                                       DateTimeStyles.AssumeLocal | DateTimeStyles.AllowWhiteSpaces, out var dt))
            {
               
                return DateTime.SpecifyKind(new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0), DateTimeKind.Utc);
            }

            
            if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out dt))
                return DateTime.SpecifyKind(dt, DateTimeKind.Utc);

            throw new JsonException($"Neispravan format datuma: '{s}'. Očekivano npr. 12.04.2000.");
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
           
            writer.WriteStringValue(value.ToUniversalTime().ToString("dd.MM.yyyy"));
        }
    }

    public class HrNullableDateTimeConverter : JsonConverter<DateTime?>
    {
        private readonly HrDateTimeConverter _inner = new();
        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => reader.TokenType == JsonTokenType.Null ? null : _inner.Read(ref reader, typeof(DateTime), options);

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (value is null) { writer.WriteNullValue(); return; }
            _inner.Write(writer, value.Value, options);
        }
    }
}
