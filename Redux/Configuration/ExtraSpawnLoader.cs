using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;
using Redux.Database.Domain;

namespace Redux.Configuration
{
    /// <summary>
    /// Reads optional spawn definitions from a JSON file so that additional monsters can be
    /// layered on top of database content without requiring SQL changes.
    /// </summary>
    public static class ExtraSpawnLoader
    {
        private static readonly string ConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configuration", "extra-spawns.json");
        private static IReadOnlyList<DbSpawn> _cache;

        public static IEnumerable<DbSpawn> GetSpawnsForMap(ushort mapId)
        {
            if (_cache == null)
            {
                _cache = LoadFromDisk();
            }

            return _cache.Where(x => x.Map == mapId);
        }

        private static IReadOnlyList<DbSpawn> LoadFromDisk()
        {
            if (!File.Exists(ConfigPath))
                return Array.Empty<DbSpawn>();

            try
            {
                var json = File.ReadAllText(ConfigPath);
                var serializer = new JavaScriptSerializer();
                var entries = serializer.Deserialize<List<ExtraSpawnEntry>>(json) ?? new List<ExtraSpawnEntry>();
                return entries.Select(ToDbSpawn).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to load extra spawns: {0}", ex.Message);
                return Array.Empty<DbSpawn>();
            }
        }

        private static DbSpawn ToDbSpawn(ExtraSpawnEntry entry)
        {
            return new DbSpawn
            {
                Map = entry.Map,
                MonsterType = entry.MonsterType,
                X1 = entry.X1,
                Y1 = entry.Y1,
                X2 = entry.X2,
                Y2 = entry.Y2,
                AmountPer = entry.AmountPer,
                AmountMax = entry.AmountMax,
                Frequency = entry.Frequency
            };
        }

        private class ExtraSpawnEntry
        {
            public ushort Map { get; set; }
            public ushort X1 { get; set; }
            public ushort Y1 { get; set; }
            public ushort X2 { get; set; }
            public ushort Y2 { get; set; }
            public uint MonsterType { get; set; }
            public int AmountPer { get; set; } = 5;
            public int AmountMax { get; set; } = 10;
            public int Frequency { get; set; } = 10;
        }
    }
}
