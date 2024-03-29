﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

// ReSharper disable StringLiteralTypo

namespace TZResScraper
{
    public class Program
    {
        // ReSharper disable once InconsistentNaming
        private static readonly Dictionary<ushort, Language> Languages = new();
        // ReSharper disable once InconsistentNaming
        private static string TZResDLL = @"C:\windows\system32\tzres.dll";
        // ReSharper disable once InconsistentNaming
        private static string OutputFile = "tzinfo.json";
        // ReSharper disable once InconsistentNaming
        private static bool WriteOutput = true;

        public static void Main(string[] args)
        {
            ParseArgs(args);

            Console.WriteLine($"Resource DLL: {TZResDLL}");
            Console.WriteLine($"Resource JSON: {OutputFile}");

            var extractor = new ResourceExtractor(Languages);
            extractor.Extract(TZResDLL);

            // Now, match resources with time zone info, building the
            // ultimate windows TZ rosetta stone
            var tzs = TimeZoneInfo.GetSystemTimeZones();
            var myLang = Languages[(ushort)CultureInfo.InstalledUICulture.LCID];

            // Create a reverse lookup table, while coalescing duplicate values.
            var reverseLookup = myLang.StringTable
                .GroupBy(kvp => kvp.Value)

                // group key is now the string in OS language;
                // group items' keys are the index in the string table
                // where that string was found; we only care about the
                // first place it was found.
                .ToDictionary(g => g.Key, g => g.First().Key);

            foreach (var lang in Languages.Values.OrderBy(l => l.LCID))
            {
                string Fix(string str) =>
                    string.IsNullOrEmpty(str) ? str : lang.StringTable[reverseLookup[str]];
                foreach (var tz in tzs)
                {
                    lang.TimeZones[tz.Id] = Fix(tz.DisplayName);
                }
            }

            if (!WriteOutput)
            {
                Console.WriteLine($"{Languages.Count} languages found, but not writing output based on '-t' option.");
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

        private static void WriteJsonFile(string fileName)
        {
            var topLevel = new
            {
                Languages = Languages.Values.Select(l => new
                {
                    Locale = l.Name,
                    l.TimeZones
                }),
            };

            var json = JsonSerializer.Serialize(topLevel, new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = true
            });
            File.WriteAllText(fileName, json, Encoding.UTF8);
        }
    }
}
