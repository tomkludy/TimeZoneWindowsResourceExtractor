using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TZResScraper
{
    class Language
    {
        public ushort LCID { get; set; }
        public string Name { get; set; }
        public Dictionary<uint, string> StringTable { get; } = new Dictionary<uint, string>();
    }
}
