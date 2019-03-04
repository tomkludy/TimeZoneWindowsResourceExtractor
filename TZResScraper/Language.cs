using Newtonsoft.Json;
using System.Collections.Generic;

namespace TZResScraper
{
    class Language
    {
        public ushort LCID { get; set; }
        public string Name { get; set; }
        public Dictionary<string, string> TimeZones { get; } = new Dictionary<string, string>();

        [JsonIgnore]
        internal Dictionary<uint, string> StringTable { get; } = new Dictionary<uint, string>();
    }
}
