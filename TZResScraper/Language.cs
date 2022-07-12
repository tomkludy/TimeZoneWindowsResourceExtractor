using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TZResScraper
{
    internal class Language
    {
        // ReSharper disable once InconsistentNaming
        public ushort LCID { get; set; }
        public string? Name { get; set; }
        public Dictionary<string, string> TimeZones { get; } = new Dictionary<string, string>();

        [JsonIgnore]
        internal Dictionary<uint, string> StringTable { get; } = new Dictionary<uint, string>();
    }
}
