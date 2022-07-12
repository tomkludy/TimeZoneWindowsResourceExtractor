using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace TZResScraper
{
    internal class ResourceExtractor
    {
        // ReSharper disable once InconsistentNaming
        private readonly Dictionary<ushort, Language> Languages;

        public ResourceExtractor(Dictionary<ushort, Language> languages)
        {
            Languages = languages;
        }

        public bool Extract(string dll)
        {
            try
            {
                var module = LoadLibraryEx(dll, IntPtr.Zero, LOAD_LIBRARY_AS_DATAFILE);
                if (module == IntPtr.Zero) throw new Win32Exception();
                if (!EnumResourceNames(module, (IntPtr)RT_STRING, EnumNamesFunc, IntPtr.Zero))
                    throw new Win32Exception();
                return true;
            }
            catch (Win32Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        private bool EnumNamesFunc(IntPtr hModule, IntPtr type, IntPtr name, IntPtr lp)
        {
            //var idOrName = (long)name;
            //string? sName;
            //if (idOrName >> 16 == 0) sName = $"#{idOrName}";
            //else sName = Marshal.PtrToStringAnsi(name);
            //
            //Console.WriteLine($"RT_STRING {sName}:");

            if (!EnumResourceLanguages(hModule, type, name, EnumLanguagesFunc, IntPtr.Zero))
                throw new Win32Exception();

            return true;
        }

        private bool EnumLanguagesFunc(IntPtr hModule, IntPtr type, IntPtr name, ushort wLang, IntPtr lp)
        {
            var nBundle = (uint)name;
            var min = (nBundle - 1) * 16;
            var max = nBundle * 16;

            var hRSrc = FindResourceEx(hModule, (IntPtr)RT_STRING, (IntPtr)nBundle, wLang);
            if (hRSrc == IntPtr.Zero)
                throw new Win32Exception();

            var hGlob = LoadResource(hModule, hRSrc);
            if (hGlob == IntPtr.Zero)
                throw new Win32Exception();

            var pwSz = LockResource(hGlob);
            if (pwSz == IntPtr.Zero)
                throw new Win32Exception();

            for (var uId = min; uId < max; uId++)
            {
                var len = Marshal.ReadInt16(pwSz);
                if (len != 0)
                {
                    var str = Marshal.PtrToStringUni(pwSz + 2, len);
                    //Console.WriteLine($"  STRING {wLang} {uId}={str}");

                    AddString(wLang, uId, str);
                }
                pwSz += 2 * (1 + len);
            }

            return true;
        }

        private void AddString(ushort wLang, uint uId, string str)
        {
            if (!Languages.TryGetValue(wLang, out var lang))
            {
                var name = new StringBuilder(500);
                if (0 == GetLocaleInfo(wLang, LOCALE_SNAME, name, name.MaxCapacity))
                    throw new Win32Exception();
                lang = new Language
                {
                    LCID = wLang,
                    Name = name.ToString(),
                };
                Languages.Add(wLang, lang);
            }
            lang.StringTable[uId] = str;
        }

        // ReSharper disable once InconsistentNaming
        private const int RT_STRING = 6;
        // ReSharper disable once InconsistentNaming
        private const int LOAD_LIBRARY_AS_DATAFILE = 2;
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once IdentifierTypo
        private const uint LOCALE_SNAME = 0x0000005c;

        private delegate bool EnumResNameProc(IntPtr hModule, IntPtr type, IntPtr name, IntPtr lp);
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool EnumResourceNames(IntPtr hModule, IntPtr type, EnumResNameProc lpEnumFunc, IntPtr lp);
        private delegate bool EnumResLangProc(IntPtr hModule, IntPtr type, IntPtr name, ushort wLang, IntPtr lp);
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool EnumResourceLanguages(IntPtr hModule, IntPtr type, IntPtr name, EnumResLangProc lpEnumFunc, IntPtr lp);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr FindResourceEx(IntPtr hModule, IntPtr type, IntPtr name, ushort wLang);
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr LoadResource(IntPtr hModule, IntPtr hResInfo);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr LockResource(IntPtr hResData);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hFile, int dwFlags);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern int GetLocaleInfo(uint locale, uint lcType, StringBuilder lpLcData, int cchData);
    }
}
