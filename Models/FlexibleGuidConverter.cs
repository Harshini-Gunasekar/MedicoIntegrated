using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Booking.Models
{
    public class FlexibleGuidConverter : JsonConverter<Guid>
    {
        public override Guid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return Guid.Empty;

            if (reader.TokenType == JsonTokenType.String)
            {
                string? str = reader.GetString();
                if (string.IsNullOrWhiteSpace(str))
                    return Guid.Empty;

                if (Guid.TryParse(str, out var guid))
                    return guid;

                return Guid.Empty;
            }

            if (reader.TokenType == JsonTokenType.Number)
            {
                return Guid.Empty;
            }

            return Guid.Empty;
        }

        public override void Write(Utf8JsonWriter writer, Guid value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }

    public class FlexibleNullableGuidConverter : JsonConverter<Guid?>
    {
        public override Guid? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            if (reader.TokenType == JsonTokenType.String)
            {
                string? str = reader.GetString();
                if (string.IsNullOrWhiteSpace(str))
                    return null;

                if (Guid.TryParse(str, out var guid))
                {
                    if (guid == Guid.Empty)
                        return null;
                    return guid;
                }

                return null;
            }

            if (reader.TokenType == JsonTokenType.Number)
            {
                return null;
            }

            return null;
        }

        public override void Write(Utf8JsonWriter writer, Guid? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
                writer.WriteStringValue(value.Value.ToString());
            else
                writer.WriteNullValue();
        }
    }
}
