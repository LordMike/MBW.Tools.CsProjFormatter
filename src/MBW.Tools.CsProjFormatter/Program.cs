using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MBW.Tools.CsProjFormatter.Configuration.EditorConfig;
using MBW.Tools.CsProjFormatter.Library;
using MBW.Tools.CsProjFormatter.Library.Configuration;

namespace MBW.Tools.CsProjFormatter
{
    class Program
    {
        static int Main(string[] args)
        {
            if (!args.Any())
            {
                Console.WriteLine("Missing args, add directory to root of projects");
                return 1;
            }

            IEnumerable<string> files = args.SelectMany(s =>
                Directory.EnumerateFiles(s, "*.csproj", SearchOption.AllDirectories)
                    .Concat(Directory.EnumerateFiles(s, "*.targets", SearchOption.AllDirectories)));

            //files = files.Take(1);

            FormatterSettingsFactory settingsFactory = new FormatterSettingsFactory().AddEditorConfig();
            Formatter formatter = new Formatter(settingsFactory);

            foreach (string file in files)
            {
                Console.WriteLine("Handling " + file);

                formatter.FormatFile(file);
            }

            return 0;
        }
    }
}
