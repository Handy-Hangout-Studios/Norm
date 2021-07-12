using Norm.Omdb.Enums;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Norm.Omdb.JsonConverters
{
    class OmdbSearchTypeJsonConverter : JsonConverter<OmdbSearchType>
    {
        public override OmdbSearchType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType is not JsonTokenType.String)
                throw new ArgumentException("Must have a string to parse");
            string switcher = reader.GetString()!.ToLower();
            return switcher switch
            {
                "movie" => OmdbSearchType.MOVIE,
                "series" => OmdbSearchType.SERIES,
                "episode" => OmdbSearchType.EPISODE,
                "game" => OmdbSearchType.GAME,
                _ => throw new JsonException($"An unknown OMDB search type was used: {switcher}")
            };
        }

        public override void Write(Utf8JsonWriter writer, OmdbSearchType value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToQueryValue());
        }
    }
}
