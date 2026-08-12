using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Booking.Models
{
    public class FlexibleBoolConverter : JsonConverter<bool>
    {
        public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.True)
                return true;
            if (reader.TokenType == JsonTokenType.False)
                return false;
            if (reader.TokenType == JsonTokenType.Number)
            {
                if (reader.TryGetInt64(out long l))
                    return l != 0;
                if (reader.TryGetDouble(out double d))
                    return d != 0;
            }
            if (reader.TokenType == JsonTokenType.String)
            {
                string? str = reader.GetString();
                if (string.IsNullOrWhiteSpace(str))
                    return false;
                if (bool.TryParse(str, out bool b))
                    return b;
                if (long.TryParse(str, out long l))
                    return l != 0;
                if (str.Equals("1", StringComparison.OrdinalIgnoreCase) ||
                    str.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                    str.Equals("yes", StringComparison.OrdinalIgnoreCase) ||
                    str.Equals("y", StringComparison.OrdinalIgnoreCase))
                    return true;
                return false;
            }
            if (reader.TokenType == JsonTokenType.Null)
                return false;

            return false;
        }

        public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
        {
            writer.WriteBooleanValue(value);
        }
    }

    public class FlexibleNullableBoolConverter : JsonConverter<bool?>
    {
        public override bool? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;
            if (reader.TokenType == JsonTokenType.True)
                return true;
            if (reader.TokenType == JsonTokenType.False)
                return false;
            if (reader.TokenType == JsonTokenType.Number)
            {
                if (reader.TryGetInt64(out long l))
                    return l != 0;
                if (reader.TryGetDouble(out double d))
                    return d != 0;
            }
            if (reader.TokenType == JsonTokenType.String)
            {
                string? str = reader.GetString();
                if (string.IsNullOrWhiteSpace(str))
                    return null;
                if (bool.TryParse(str, out bool b))
                    return b;
                if (long.TryParse(str, out long l))
                    return l != 0;
                if (str.Equals("1", StringComparison.OrdinalIgnoreCase) ||
                    str.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                    str.Equals("yes", StringComparison.OrdinalIgnoreCase) ||
                    str.Equals("y", StringComparison.OrdinalIgnoreCase))
                    return true;
                if (str.Equals("0", StringComparison.OrdinalIgnoreCase) ||
                    str.Equals("false", StringComparison.OrdinalIgnoreCase) ||
                    str.Equals("no", StringComparison.OrdinalIgnoreCase) ||
                    str.Equals("n", StringComparison.OrdinalIgnoreCase))
                    return false;
                return null;
            }
            return null;
        }

        public override void Write(Utf8JsonWriter writer, bool? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
                writer.WriteBooleanValue(value.Value);
            else
                writer.WriteNullValue();
        }
    }
}
