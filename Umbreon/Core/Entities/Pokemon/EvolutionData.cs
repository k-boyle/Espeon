using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Globalization;

namespace Umbreon.Core.Entities.Pokemon
{
    public partial class EvolutionData
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("evolved_species_id")]
        public long EvolvedSpeciesId { get; set; }

        [JsonProperty("evolution_trigger_id")]
        public long EvolutionTriggerId { get; set; }

        [JsonProperty("trigger_item_id")]
        public GenderId TriggerItemId { get; set; }

        [JsonProperty("minimum_level")]
        public GenderId MinimumLevel { get; set; }

        [JsonProperty("gender_id")]
        public GenderId GenderId { get; set; }

        [JsonProperty("location_id")]
        public GenderId LocationId { get; set; }

        [JsonProperty("held_item_id")]
        public GenderId HeldItemId { get; set; }

        [JsonProperty("time_of_day")]
        public TimeOfDay TimeOfDay { get; set; }

        [JsonProperty("known_move_id")]
        public GenderId KnownMoveId { get; set; }

        [JsonProperty("known_move_type_id")]
        public GenderId KnownMoveTypeId { get; set; }

        [JsonProperty("minimum_happiness")]
        public GenderId MinimumHappiness { get; set; }

        [JsonProperty("minimum_beauty")]
        public GenderId MinimumBeauty { get; set; }

        [JsonProperty("minimum_affection")]
        public GenderId MinimumAffection { get; set; }

        [JsonProperty("relative_physical_stats")]
        public GenderId RelativePhysicalStats { get; set; }

        [JsonProperty("party_species_id")]
        public GenderId PartySpeciesId { get; set; }

        [JsonProperty("party_type_id")]
        public GenderId PartyTypeId { get; set; }

        [JsonProperty("trade_species_id")]
        public GenderId TradeSpeciesId { get; set; }

        [JsonProperty("needs_overworld_rain")]
        public long NeedsOverworldRain { get; set; }

        [JsonProperty("turn_upside_down")]
        public long TurnUpsideDown { get; set; }
    }

    public enum TimeOfDay { Day, Empty, Night };

    public partial struct GenderId
    {
        public long? Integer;
        public string String;

        public static implicit operator GenderId(long Integer) => new GenderId { Integer = Integer };
        public static implicit operator GenderId(string String) => new GenderId { String = String };
    }

    public partial class EvolutionData
    {
        public static EvolutionData[] FromJson(string json) => JsonConvert.DeserializeObject<EvolutionData[]>(json, EvolConverter.Settings);
    }

    public static class EvolSerialize
    {
        public static string ToJson(this EvolutionData[] self) => JsonConvert.SerializeObject(self, EvolConverter.Settings);
    }

    internal static class EvolConverter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters = {
                GenderIdConverter.Singleton,
                TimeOfDayConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    internal class GenderIdConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(GenderId) || t == typeof(GenderId?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.Integer:
                    var integerValue = serializer.Deserialize<long>(reader);
                    return new GenderId { Integer = integerValue };
                case JsonToken.String:
                case JsonToken.Date:
                    var stringValue = serializer.Deserialize<string>(reader);
                    return new GenderId { String = stringValue };
            }
            throw new Exception("Cannot unmarshal type GenderId");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            var value = (GenderId)untypedValue;
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
            throw new Exception("Cannot marshal type GenderId");
        }

        public static readonly GenderIdConverter Singleton = new GenderIdConverter();
    }

    internal class TimeOfDayConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(TimeOfDay) || t == typeof(TimeOfDay?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "":
                    return TimeOfDay.Empty;
                case "day":
                    return TimeOfDay.Day;
                case "night":
                    return TimeOfDay.Night;
            }
            throw new Exception("Cannot unmarshal type TimeOfDay");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (TimeOfDay)untypedValue;
            switch (value)
            {
                case TimeOfDay.Empty:
                    serializer.Serialize(writer, "");
                    return;
                case TimeOfDay.Day:
                    serializer.Serialize(writer, "day");
                    return;
                case TimeOfDay.Night:
                    serializer.Serialize(writer, "night");
                    return;
            }
            throw new Exception("Cannot marshal type TimeOfDay");
        }

        public static readonly TimeOfDayConverter Singleton = new TimeOfDayConverter();
    }
}

