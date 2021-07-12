using Norm.Omdb.Enums;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Norm.Omdb.JsonConverters
{
    public class OmdbParentalRatingJsonConverter : JsonConverter<OmdbParentalRating>
    {
        public override OmdbParentalRating Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType is not JsonTokenType.String)
                throw new ArgumentException("Must have a string to parse");
            return reader.GetString()!.ToOmdbParentalRating();
        }

        public override void Write(Utf8JsonWriter writer, OmdbParentalRating value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToQueryValue());
        }
    }
}
