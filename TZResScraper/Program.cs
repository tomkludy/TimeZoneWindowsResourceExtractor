using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace TZResScraper
{
    partial class Program
    {
        readonly static Dictionary<ushort, Language> Languages = new Dictionary<ushort, Language>();
        static string TZResDLL = @"C:\windows\system32\tzres.dll";
        static string OutputFile = "tzinfo.json";
        static bool WriteOutput = true;

        static void Main(string[] args)
        {
            ParseArgs(args);

            Console.WriteLine($"Resource DLL: {TZResDLL}");
            Console.WriteLine($"Resource JSON: {OutputFile}");

            if (ReadJsonFile(OutputFile))
            {
                Console.WriteLine($"Read {Languages.Count} languages from {OutputFile}");
            }
            else
            {
                Console.WriteLine("JSON file does not exist");
            }

            var extractor = new ResourceExtractor(Languages);
            extractor.Extract(TZResDLL);

            // Now, match resources with time zone info, building the
            // ultimate windows TZ rosetta stone
            var tzs = TimeZoneInfo.GetSystemTimeZones();
            var mylang = Languages[(ushort)CultureInfo.InstalledUICulture.LCID];

            // Create a reverse lookup table, while coalescing duplicate values.
            var reverse_lookup = mylang.StringTable
                .GroupBy(kvp => kvp.Value)

                // group key is now the string in OS language;
                // group items' keys are the index in the string table
                // where that string was found; we only care about the
                // first place it was found.
                .ToDictionary(g => g.Key, g => g.First().Key);

            bool changed = false;

            foreach (var lang in Languages.Values.OrderBy(l => l.LCID))
            {
                string fix(string str) =>
                    string.IsNullOrEmpty(str) ? str : lang.StringTable[reverse_lookup[str]];
                foreach (var tz in tzs)
                {
                    var tzi = new TimeZoneInfoEx
                    {
                        BaseUtcOffsetMinutes = (int)tz.BaseUtcOffset.TotalMinutes,
                        DaylightName = fix(tz.DaylightName),
                        DisplayName = fix(tz.DisplayName),
                        Id = tz.Id,
                        StandardName = fix(tz.StandardName),
                        SupportsDaylightSavingTime = tz.SupportsDaylightSavingTime,
                    };

                    if (!lang.TimeZones.ContainsKey(tzi.Id) || !lang.TimeZones[tzi.Id].Equals(tzi))
                    {
                        lang.TimeZones[tzi.Id] = tzi;
                        changed = true;
                    }
                }
            }

            if (!changed)
            {
                Console.WriteLine("No changes found; nothing to write.");
            }
            else if (!WriteOutput)
            {
                Console.WriteLine("Changes found, but not writing output based on '-t' option.");
            }
            else
            {
                WriteJsonFile(OutputFile);
                Console.WriteLine($"Wrote {Languages.Count} languages to {OutputFile}");
            }
        }

        private static void ParseArgs(string[] args)
        {
            for (var i = 0; i < args.Length; i++)
            {
                switch (args[i].ToLowerInvariant())
                {
                    case "-?":
                        Console.WriteLine("Options:");
                        Console.WriteLine("-?                 Show options.");
                        Console.WriteLine("-d [path_to_dll]   Specify DLL to extract resources from.");
                        Console.WriteLine("-r [path_to_json]  Specify JSON file to store resources in.");
                        Console.WriteLine("-t                 Test only; don't update json file.");
                        Environment.Exit(0);
                        break;
                    case "-d":
                        i++;
                        if (i >= args.Length)
                        {
                            Console.WriteLine("Missing DLL file name; exiting");
                            Environment.Exit(-1);
                        }
                        TZResDLL = args[i];
                        if (!File.Exists(TZResDLL))
                        {
                            Console.WriteLine("Bad DLL file name; exiting");
                            Environment.Exit(-1);
                        }
                        break;
                    case "-r":
                        i++;
                        if (i >= args.Length)
                        {
                            Console.WriteLine("Missing JSON file name; exiting");
                            Environment.Exit(-1);
                        }
                        OutputFile = args[i];
                        break;
                    case "-t":
                        WriteOutput = false;
                        break;
                    default:
                        Console.WriteLine($"Invalid option: {args[i]}; exiting");
                        Environment.Exit(-1);
                        break;
                }
            }
        }

        private static bool ReadJsonFile(string fileName)
        {
            if (!File.Exists(fileName))
                return false;

            try
            {
                var json = File.ReadAllText(fileName, Encoding.UTF8);
                var toplevel = JsonConvert.DeserializeObject<JsonFileTopLevel>(json);
                toplevel.Languages.ForEach(lang => Languages.Add(lang.LCID, lang));
            }
            catch
            {
                Console.WriteLine($"Could not read JSON file {fileName}; exiting.");
                Environment.Exit(-1);
            }

            return true;
        }

        private static void WriteJsonFile(string fileName)
        {
            var toplevel = new JsonFileTopLevel
            {
                LastUpdateTime = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ssZ"),
                Languages = Languages.Values.OrderBy(l => l.LCID).ToList()
            };
            var json = JsonConvert.SerializeObject(toplevel, Formatting.Indented);
            File.WriteAllText(fileName, json, Encoding.UTF8);
        }

        class JsonFileTopLevel
        {
            public string LastUpdateTime { get; set; }
            public List<Language> Languages { get; set; }
        }
    }
}
