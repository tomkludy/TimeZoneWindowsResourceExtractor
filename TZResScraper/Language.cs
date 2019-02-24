using Newtonsoft.Json;
using System.Collections.Generic;

namespace TZResScraper
{
    class Language
    {
        public ushort LCID { get; set; }
        public string Name { get; set; }
        public Dictionary<string, TimeZoneInfoEx> TimeZones { get; } = new Dictionary<string, TimeZoneInfoEx>();

        [JsonIgnore]
        internal Dictionary<uint, string> StringTable { get; } = new Dictionary<uint, string>();
    }
}
