using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MBW.Tools.CsProjFormatter
{
    internal static class Extensions
    {
        public static ILogger<T> GetLogger<T>(this IServiceProvider serviceProvider)
        {
            return serviceProvider.GetRequiredService<ILogger<T>>();
        }

        public static string[] ExpandPath(string path)
        {
            var lastIdx = path.LastIndexOfAny(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar });

            if (lastIdx < 0)
                return new[] { path };

            var dir = path.Substring(0, lastIdx);
            var name = path.Substring(lastIdx + 1);

            var dirs = Directory.GetDirectories(dir, name);
            if (dirs.Any())
                return dirs;

            return new[] { path };
        }
    }
}