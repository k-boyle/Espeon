using Discord;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Umbreon.Attributes;
using Umbreon.Core;
using Umbreon.Core.Entities.Pokemon;
using Umbreon.Helpers;
using Colour = Discord.Color;

namespace Umbreon.Services
{
    [Service]
    public class PokemonDataService
    {
        private const string DataDir = "./Pokemon/pokemon_species.json";
        private const string EvolDir = "./Pokemon/pokemon_evolution.json";
        private const string ImageDir = "./Pokemon/Sprites";

        private readonly IReadOnlyDictionary<int, Colour> _colours = new Dictionary<int, Colour>
        {
            { 1, Colour.Default },
            { 2, Colour.Blue },
            { 3, new Colour(146, 110, 0) },
            { 4, Colour.LightGrey },
            { 5, Colour.Green },
            { 6, new Colour(231, 140, 255) },
            { 7, Colour.Purple },
            { 8, Colour.Red },
            { 9, new Colour(255, 255, 255) },
            { 10, new Colour(255, 255, 0) }
        };

        private PokemonData[] _data;
        private EvolutionData[] _evol;
        
        private readonly LogService _log;

        public PokemonDataService(LogService log)
        {
            _log = log;
        }

        public void Initialise()
        {
            _data = PokemonData.FromJson(File.ReadAllText(DataDir)).Where(x => x.Id <= ConstantsHelper.PokemonLimit).ToArray();
            _evol = EvolutionData.FromJson(File.ReadAllText(EvolDir)).Where(x => x.Id <= ConstantsHelper.PokemonLimit).ToArray();
            _log.NewLogEvent(LogSeverity.Info, LogSource.Pokemon, "Pokemon data has been Initialised");
        }

        public PokemonData GetData(int id)
            => _data.FirstOrDefault(x => x.Id == id);

        public PokemonData GetData(string name)
            => _data.FirstOrDefault(x => string.Equals(x.Identifier, name, StringComparison.CurrentCultureIgnoreCase));

        public static Stream GetImage(PokemonData pokemon)
            => GetImage((int)pokemon.Id);

        public static Stream GetImage(int id)
        {
            var dir = $"{ImageDir}/Normal/{id}.png";
            return !File.Exists(dir) ? null : new FileStream(dir, FileMode.Open);
        }

        public Colour GetColour(PokemonData pokemon)
            => GetColour((int)pokemon.ColorId);

        public Colour GetColour(int key)
            => _colours[key];

        public IEnumerable<KeyValuePair<PokemonData, int>> GetEvolutions(PokemonData pokemon)
        {
            var foundEvolutions = _data.Where(x => x.EvolutionChainId == pokemon.EvolutionChainId).OrderBy(y => y.Id).ToImmutableArray();
            if (foundEvolutions.Length == 1)
                yield break;
            foreach (var found in foundEvolutions)
            {
                var evolveData = _evol.FirstOrDefault(x => x.EvolvedSpeciesId == found.Id);
                var evolvesAt = evolveData?.MinimumLevel.Integer;
                yield return new KeyValuePair<PokemonData, int>(found, (int?)evolvesAt ?? 0);
            }
        }
    }
}
