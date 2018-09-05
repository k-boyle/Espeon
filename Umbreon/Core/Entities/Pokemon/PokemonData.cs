using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Globalization;

namespace Umbreon.Core.Entities.Pokemon
{
    public partial class PokemonData
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("identifier")]
        public string Identifier { get; set; }

        [JsonProperty("generation_id")]
        public long GenerationId { get; set; }

        [JsonProperty("evolves_from_species_id")]
        public ConquestOrder EvolvesFromSpeciesId { get; set; }

        [JsonProperty("evolution_chain_id")]
        public long EvolutionChainId { get; set; }

        [JsonProperty("color_id")]
        public long ColorId { get; set; }

        [JsonProperty("shape_id")]
        public long ShapeId { get; set; }

        [JsonProperty("habitat_id")]
        public ConquestOrder HabitatId { get; set; }

        [JsonProperty("gender_rate")]
        public long GenderRate { get; set; }

        [JsonProperty("capture_rate")]
        public long CaptureRate { get; set; }

        [JsonProperty("base_happiness")]
        public long BaseHappiness { get; set; }

        [JsonProperty("is_baby")]
        public long IsBaby { get; set; }

        [JsonProperty("hatch_counter")]
        public long HatchCounter { get; set; }

        [JsonProperty("has_gender_differences")]
        public long HasGenderDifferences { get; set; }

        [JsonProperty("growth_rate_id")]
        public long GrowthRateId { get; set; }

        [JsonProperty("forms_switchable")]
        public long FormsSwitchable { get; set; }

        [JsonProperty("order")]
        public long Order { get; set; }

        [JsonProperty("conquest_order")]
        public ConquestOrder ConquestOrder { get; set; }
    }

    public partial struct ConquestOrder
    {
        public long? Integer;
        public string String;

        public static implicit operator ConquestOrder(long Integer) => new ConquestOrder { Integer = Integer };
        public static implicit operator ConquestOrder(string String) => new ConquestOrder { String = String };
    }

    public partial class PokemonData
    {
        public static PokemonData[] FromJson(string json) => JsonConvert.DeserializeObject<PokemonData[]>(json, DataConverter.Settings);
    }

    public static class DataSerialize
    {
        public static string ToJson(this PokemonData[] self) => JsonConvert.SerializeObject(self, DataConverter.Settings);
    }

    internal static class DataConverter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters = {
                ConquestOrderConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    internal class ConquestOrderConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(ConquestOrder) || t == typeof(ConquestOrder?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.Integer:
                    var integerValue = serializer.Deserialize<long>(reader);
                    return new ConquestOrder { Integer = integerValue };
                case JsonToken.String:
                case JsonToken.Date:
                    var stringValue = serializer.Deserialize<string>(reader);
                    return new ConquestOrder { String = stringValue };
            }
            throw new Exception("Cannot unmarshal type ConquestOrder");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            var value = (ConquestOrder)untypedValue;
            if (value.Integer != null)
            {
                serializer.Serialize(writer, value.Integer.Value);
                return;
            }
            if (value.String != null)
            {
                serializer.Serialize(writer, value.String);
                return;
            }
            throw new Exception("Cannot marshal type ConquestOrder");
        }

        public static readonly ConquestOrderConverter Singleton = new ConquestOrderConverter();
    }
}

