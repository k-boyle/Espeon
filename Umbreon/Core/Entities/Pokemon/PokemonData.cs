using Newtonsoft.Json;

namespace Umbreon.Core.Entities.Pokemon
{
    public partial class PokemonData
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("identifier")]
        public string Name { get; set; }

        [JsonProperty("evolves_from_species_id")]
        public int? EvolvesFromSpeciesId { get; set; }

        [JsonProperty("evolution_chain_id")]
        public int? EvolutionChainId { get; set; }

        [JsonProperty("color_id")]
        public int ColorId { get; set; }

        [JsonProperty("habitat_id")]
        public int? HabitatId { get; set; }

        [JsonProperty("capture_rate")]
        public int CaptureRate { get; set; }

        [JsonProperty("encounter_rate")]
        public int EncounterRate { get; set; }
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
            DateParseHandling = DateParseHandling.None
        };
    }
}

