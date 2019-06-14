using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace MBW.Tools.CsProjFormatter
{
    internal class FormatterSettings
    {

    }

    internal class ProjectLine
    {
        public string Text { get; set; }
        public int Indent { get; set; }
    }

    internal interface ISpecialLine
    {

    }

    internal class SpecialPackageReference : ISpecialLine
    {
        public string PackageName { get; set; }

        public string Version { get; set; }
    }

    internal class SpecialProjectReference : ISpecialLine
    {
        public string Project { get; set; }
    }

    internal class Formatter
    {
        private readonly Regex _selfClosingTag = new Regex(@"/>[\s]*$", RegexOptions.Compiled);
        private readonly Regex _hasClosingTag = new Regex(@"</[^>]+>[\s]*$", RegexOptions.Compiled);
        private readonly Regex _hasOpeningTag = new Regex(@"^<[\w]+(?: [^>]+)?>", RegexOptions.Compiled);
        private readonly Regex _shouldSort = new Regex(@"^<(?:PackageReference|ProjectReference) ", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private readonly FormatterSettings _settings;
        private readonly UTF8Encoding _encoding = new UTF8Encoding(false);

        public Formatter(FormatterSettings settings = null)
        {
            _settings = settings ?? new FormatterSettings();
        }

        private List<ProjectLine> Load(string source)
        {
            XDocument doc = XDocument.Parse(source, LoadOptions.PreserveWhitespace);

            List<ProjectLine> lines = new List<ProjectLine>();
            using (MemoryStream strm = new MemoryStream())
            {
                using (XmlWriter wr = XmlWriter.Create(strm, new XmlWriterSettings
                {
                    Encoding = _encoding,
                    IndentChars = "  ",
                    Indent = true,
                    OmitXmlDeclaration = true,
                    CloseOutput = false
                }))
                {
                    doc.WriteTo(wr);
                }

                strm.Seek(0, SeekOrigin.Begin);

                using (StreamReader sr = new StreamReader(strm, _encoding))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                        lines.Add(new ProjectLine { Text = line });
                }
            }

            // Trim all lines
            for (int i = 1; i < lines.Count; i++)
                lines[i].Text = lines[i].Text.Trim();

            // Indent
            int indent = 0;
            for (int i = 0; i < lines.Count; i++)
            {
                ProjectLine line = lines[i];

                if (line.Text == string.Empty)
                    continue;

                bool isOpening = _hasOpeningTag.IsMatch(line.Text);
                bool isSelfClosing = _selfClosingTag.IsMatch(line.Text);
                bool isClosing = _hasClosingTag.IsMatch(line.Text);

                if (isClosing && !isOpening)
                    indent--;

                line.Indent = indent;

                if (isOpening && !isClosing && !isSelfClosing)
                {
                    indent++;
                }
            }

            return lines;
        }

        private void ParseSpecialLines(IList<ProjectLine> lines)
        {
            foreach (ProjectLine projectLine in lines)
            {
                if (projectLine.Text.StartsWith("<PackageReference", StringComparison.OrdinalIgnoreCase))
                {

                }
                else if (projectLine.Text.StartsWith("<ProjectReference", StringComparison.OrdinalIgnoreCase))
                {

                }
            }
        }

        public string Format(string source)
        {
            var lines = Load(source);

            // Sort all consecutive package references
            var shouldSortIndexes = lines.Select(s => _shouldSort.IsMatch(s)).ToArray();

            int offset = 0;
            while (true)
            {
                var first = Array.FindIndex(shouldSortIndexes, offset, x => x);
                if (first < 0)
                    break;

                var last = Array.FindIndex(shouldSortIndexes, first, x => !x);
                offset = last;

                // Sort first..last
                lines.Sort(first, last - first, StringComparer.OrdinalIgnoreCase);
            }

            // Find consecutive blank lines
            for (int i = 1; i < lines.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(lines[i - 1]) || !string.IsNullOrWhiteSpace(lines[i]))
                    continue;

                lines.RemoveAt(i);
                i--;
            }

            return string.Join(Environment.NewLine, lines);
        }
    }

    class Program
    {
        static int Main(string[] args)
        {
            if (!args.Any())
            {
                Console.WriteLine("Missing args, add directory to root of projects");
                return 1;
            }

            throw new Exception("Does not handle multiline tags when sorting ... -.-'");

            IEnumerable<string> files = args.SelectMany(s =>
                Directory.EnumerateFiles(s, "*.csproj", SearchOption.AllDirectories)
                    .Concat(Directory.EnumerateFiles(s, "*.targets", SearchOption.AllDirectories)));

            Formatter formatter = new Formatter();

            Encoding enc = new UTF8Encoding(false);

            foreach (string file in files)
            {
                Console.WriteLine("Handling " + file);

                string str = File.ReadAllText(file);
                string res = formatter.Format(str);

                if (str.Equals(res, StringComparison.Ordinal))
                    continue;

                File.WriteAllText(file, res, enc);
            }

            return 0;
        }
    }
}
