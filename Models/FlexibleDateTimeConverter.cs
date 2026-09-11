using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Booking.Models
{
    public static class FlexibleDateTimeParsers
    {
        public static bool TryParseFlexibleTimeOnly(string? str, out TimeOnly result)
        {
            result = default;
            if (string.IsNullOrWhiteSpace(str))
                return false;

            str = str.Trim();

            // 1. Direct TimeOnly parse
            if (TimeOnly.TryParse(str, CultureInfo.InvariantCulture, out result))
                return true;

            if (TimeOnly.TryParse(str, CultureInfo.CurrentCulture, out result))
                return true;

            // 2. TimeSpan parse
            if (TimeSpan.TryParse(str, CultureInfo.InvariantCulture, out var ts))
            {
                result = TimeOnly.FromTimeSpan(ts);
                return true;
            }

            // 3. Full DateTime parse (extract TimeOfDay)
            if (DateTime.TryParse(str, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
            {
                result = TimeOnly.FromDateTime(dt);
                return true;
            }

            if (DateTimeOffset.TryParse(str, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dto))
            {
                result = TimeOnly.FromTimeSpan(dto.TimeOfDay);
                return true;
            }

            return false;
        }

        public static bool TryParseFlexibleDateOnly(string? str, out DateOnly result)
        {
            result = default;
            if (string.IsNullOrWhiteSpace(str))
                return false;

            str = str.Trim();

            // 1. Direct DateOnly parse
            if (DateOnly.TryParse(str, CultureInfo.InvariantCulture, out result))
                return true;

            if (DateOnly.TryParse(str, CultureInfo.CurrentCulture, out result))
                return true;

            // 2. Full DateTime parse
            if (DateTime.TryParse(str, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
            {
                result = DateOnly.FromDateTime(dt);
                return true;
            }

            if (DateTimeOffset.TryParse(str, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dto))
            {
                result = DateOnly.FromDateTime(dto.DateTime);
                return true;
            }

            return false;
        }

        public static bool TryParseFlexibleDateTime(string? str, out DateTime result)
        {
            result = default;
            if (string.IsNullOrWhiteSpace(str))
                return false;

            str = str.Trim();

            if (DateTime.TryParse(str, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
                return true;

            if (DateTime.TryParse(str, CultureInfo.CurrentCulture, DateTimeStyles.None, out result))
                return true;

            if (DateTimeOffset.TryParse(str, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dto))
            {
                result = dto.UtcDateTime;
                return true;
            }

            return false;
        }
    }

    public class FlexibleTimeOnlyConverter : JsonConverter<TimeOnly>
    {
        public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return default;

            if (reader.TokenType == JsonTokenType.String)
            {
                var str = reader.GetString();
                if (FlexibleDateTimeParsers.TryParseFlexibleTimeOnly(str, out var t))
                    return t;
                return default;
            }

            return default;
        }

        public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("HH:mm:ss"));
        }
    }

    public class FlexibleNullableTimeOnlyConverter : JsonConverter<TimeOnly?>
    {
        public override TimeOnly? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            if (reader.TokenType == JsonTokenType.String)
            {
                var str = reader.GetString();
                if (FlexibleDateTimeParsers.TryParseFlexibleTimeOnly(str, out var t))
                    return t;
                return null;
            }

            return null;
        }

        public override void Write(Utf8JsonWriter writer, TimeOnly? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
                writer.WriteStringValue(value.Value.ToString("HH:mm:ss"));
            else
                writer.WriteNullValue();
        }
    }

    public class FlexibleDateOnlyConverter : JsonConverter<DateOnly>
    {
        public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return default;

            if (reader.TokenType == JsonTokenType.String)
            {
                var str = reader.GetString();
                if (FlexibleDateTimeParsers.TryParseFlexibleDateOnly(str, out var d))
                    return d;
                return default;
            }

            return default;
        }

        public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("yyyy-MM-dd"));
        }
    }

    public class FlexibleNullableDateOnlyConverter : JsonConverter<DateOnly?>
    {
        public override DateOnly? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            if (reader.TokenType == JsonTokenType.String)
            {
                var str = reader.GetString();
                if (FlexibleDateTimeParsers.TryParseFlexibleDateOnly(str, out var d))
                    return d;
                return null;
            }

            return null;
        }

        public override void Write(Utf8JsonWriter writer, DateOnly? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
                writer.WriteStringValue(value.Value.ToString("yyyy-MM-dd"));
            else
                writer.WriteNullValue();
        }
    }

    public class FlexibleDateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return default;

            if (reader.TokenType == JsonTokenType.String)
            {
                var str = reader.GetString();
                if (FlexibleDateTimeParsers.TryParseFlexibleDateTime(str, out var dt))
                    return dt;
                return default;
            }

            return default;
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("o"));
        }
    }

    public class FlexibleNullableDateTimeConverter : JsonConverter<DateTime?>
    {
        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            if (reader.TokenType == JsonTokenType.String)
            {
                var str = reader.GetString();
                if (FlexibleDateTimeParsers.TryParseFlexibleDateTime(str, out var dt))
                    return dt;
                return null;
            }

            return null;
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
                writer.WriteStringValue(value.Value.ToString("o"));
            else
                writer.WriteNullValue();
        }
    }
}
