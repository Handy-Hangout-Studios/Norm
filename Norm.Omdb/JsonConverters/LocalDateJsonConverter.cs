using NodaTime;
using NodaTime.Text;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Norm.Omdb.JsonConverters
{
    public class LocalDateJsonConverter : JsonConverter<LocalDate>
    {
        private readonly LocalDatePattern pattern;

        public LocalDateJsonConverter()
        {
            this.pattern = LocalDatePattern.CreateWithInvariantCulture("dd MMM yyyy");
        }

        public override LocalDate Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType is not JsonTokenType.String)
                throw new ArgumentException("the token type must be of type string");

            string dateToParse = reader.GetString()!;
            ParseResult<LocalDate> pr = this.pattern.Parse(dateToParse);
            if (!pr.Success)
            {
                throw new JsonException($"Failed to parse the date: {dateToParse}", pr.Exception);
            }

            return pr.Value;
        }

        public override void Write(Utf8JsonWriter writer, LocalDate value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("dd MMM yyyy", null));
        }
    }
}
