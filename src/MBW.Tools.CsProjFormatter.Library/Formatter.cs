using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using MBW.Tools.CsProjFormatter.Library.Configuration;
using MBW.Tools.CsProjFormatter.Library.Visitors;

namespace MBW.Tools.CsProjFormatter.Library
{
    public class Formatter
    {
        private readonly IFormatterSettingsFactory _settingsFactory;

        public Formatter()
        {
            _settingsFactory = new DummyFormatterSettingsFactory(new FormatterSettings());
        }

        public Formatter(FormatterSettings settings)
        {
            _settingsFactory = new DummyFormatterSettingsFactory(settings ?? new FormatterSettings());
        }

        public Formatter(IFormatterSettingsFactory settingsFactory)
        {
            _settingsFactory = settingsFactory;
        }

        private void ProcessVisitors(XDocument doc, IXmlVisitor visitor)
        {
            Stack<XNode> nodes = new Stack<XNode>();

            if (visitor.BeginFromProject)
            {
                nodes.Push(doc.Root);
            }
            else
            {
                foreach (XNode node in doc.Root.Nodes().Reverse())
                    nodes.Push(node);
            }

            while (nodes.Any())
            {
                XNode node = nodes.Pop();
                bool @continue = visitor.Visit(node);

                if (!@continue)
                    continue;

                if (node is XElement asElement)
                {
                    foreach (XNode child in asElement.Nodes().Reverse())
                        nodes.Push(child);
                }
            }
        }

        private void Format(XDocument doc, FormatterSettings settings)
        {
            List<IXmlVisitor> visitors = new List<IXmlVisitor>();

            if (settings.PreferPackageReferenceAttributes)
                visitors.Add(new MergePackageReferenceAttributes());

            if (settings.SortPackageProjectReferences)
                visitors.Add(new SortReferences());

            if (settings.SplitTopLevelElements)
                visitors.Add(new SplitTopLevels());

            visitors.Add(new Newliner(settings.NewlineCharacter));

            if (settings.IndentStyle != IndentStyle.Unset)
                visitors.Add(new Indenter(settings.IndentStyle, settings.IndentCount));

            foreach (IXmlVisitor visitor in visitors)
            {
                ProcessVisitors(doc, visitor);
            }
        }

        public void FormatFile(string file, FormatterSettings settings = null)
        {
            using (Stream fs = File.Open(file, FileMode.Open, FileAccess.ReadWrite, FileShare.Read))
            {
                XDocument doc = XDocument.Load(fs);

                settings = settings ?? _settingsFactory.GetSettings(file);

                Format(doc, settings);

                // Use specific settings for file formatting
                Encoding encoding;
                if (settings.Encoding == "utf-8-bom")
                    encoding = new UTF8Encoding(true);
                else if (settings.Encoding == "utf-8")
                    encoding = new UTF8Encoding(false);
                else
                    encoding = Encoding.GetEncoding(settings.Encoding);

                fs.Seek(0, SeekOrigin.Begin);
                fs.SetLength(0);

                using (StreamWriter sw = new StreamWriter(fs, encoding, 4096, true))
                {
                    sw.NewLine = settings.NewlineCharacter;

                    using (XmlWriter xw = XmlWriter.Create(sw, new XmlWriterSettings
                    {
                        OmitXmlDeclaration = true,
                        CloseOutput = false
                    }))
                    {
                        doc.WriteTo(xw);
                    }

                    if (settings.InsertFinalNewline)
                        sw.WriteLine();
                }
            }
        }

        public string Format(string xml, FormatterSettings settings)
        {
            XDocument doc = XDocument.Parse(xml);

            Format(doc, settings);

            return doc.ToString(SaveOptions.DisableFormatting);
        }
    }
}