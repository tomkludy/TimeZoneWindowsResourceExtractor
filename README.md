# TimeZoneWindowsResourceExtractor
Program to extract localized time zone resources on Windows.

I created this because I needed the Windows time zone names localized, and I couldn't find any other resource out there that solves this problem.

The hardest part of making this work is that you must have a Windows machine with ALL language packs installed.  I couldn't find a good way to automate that, so I created a Windows Server 2019 machine in Azure, manually, and took a couple of days adding every single language pack.

Once you have done this, the rest is simple: just run this program on the machine with a terminal (cmd or powershell), and it will output a `tzinfo.json` file with all of the time zone names localized.  If you run it a second time, it will regenerate everything and compare it to the last run, and only update the output file if there was a change.

The program has a couple of options:

|Option|Effect|
|------|------|
|`-?`|print options on the terminal.|
|`-d [path_to_dll]`|point to the `tzres.dll` file; you should never need to change this.|
|`-r [path_to_json]`|point to the `tzres.json` file which is output.  If the file exists, it will be overwritten.|
|`-t`|test only; don't write the output file.|
