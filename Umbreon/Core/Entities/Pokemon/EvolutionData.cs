using Newtonsoft.Json;

namespace Umbreon.Core.Entities.Pokemon
{
    public partial class EvolutionData
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("evolved_species_id")]
        public int? EvolvedSpeciesId { get; set; }

        [JsonProperty("minimum_level")]
        public int? MinimumLevel { get; set; }
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
            DateParseHandling = DateParseHandling.None
        };
    }
}

