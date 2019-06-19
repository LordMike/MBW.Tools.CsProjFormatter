using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace MBW.Tools.CsProjFormatter
{
    internal class FormatterSettings
    {

    }

    interface IXmlVisitor
    {
        bool BeginFromProject { get; }

        bool Visit(XNode node);
    }

    class Newliner : IXmlVisitor
    {
        public bool BeginFromProject => true;

        public bool Visit(XNode node)
        {
            if (node is XElement asElement)
            {
                if (asElement.HasElements)
                    asElement.Add(new XText("\n"));

                if (!"Project".Equals(asElement.Name.LocalName, StringComparison.OrdinalIgnoreCase))
                    node.AddBeforeSelf(new XText("\n"));
            }
            else if (node.NodeType == XmlNodeType.Comment)
                node.AddBeforeSelf(new XText("\n"));

            return true;
        }
    }

    class Indenter : IXmlVisitor
    {
        public bool BeginFromProject => false;

        public bool Visit(XNode node)
        {
            if (node.NodeType != XmlNodeType.Element && node.NodeType != XmlNodeType.Comment)
                return true;

            int parents = 1;

            XNode parent = node;
            while (parent.Parent != node.Document.Root)
            {
                parent = parent.Parent;
                parents++;
            }

            string indentText = new string(' ', parents * 2);

            Console.WriteLine(indentText + " " + node.NodeType);
            node.AddBeforeSelf(new XText(indentText));

            if (node is XElement asElement && asElement.HasElements)
            {
                asElement.Add(new XText(indentText));
            }

            return true;
        }
    }

    class SplitTopLevels : IXmlVisitor
    {
        public bool BeginFromProject => false;

        public bool Visit(XNode node)
        {
            if (node.NodeType == XmlNodeType.Element)
            {
                node.AddBeforeSelf(new XText("\n"));

                if (node.NodesAfterSelf().All(x => x.NodeType == XmlNodeType.Text))
                    node.AddAfterSelf(new XText("\n"));
            }

            return false;
        }
    }

    class MergeReferences : IXmlVisitor
    {
        public bool BeginFromProject => false;

        public bool Visit(XNode node)
        {
            if (!(node is XElement asElement) ||
                !asElement.HasElements ||
                (!"PackageReference".Equals(asElement.Name.LocalName, StringComparison.OrdinalIgnoreCase) &&
                 !"ProjectReference".Equals(asElement.Name.LocalName, StringComparison.OrdinalIgnoreCase)))
                return true;

            // Find all child elements
            List<XElement> childs = asElement.Descendants().ToList();
            foreach (XElement element in childs)
            {
                List<XNode> elementNodes = element.Nodes().ToList();
                if (!elementNodes.All(s => s.NodeType == XmlNodeType.Text))
                    continue;

                // Merge these texts into an attribute
                string newValue = string.Join(string.Empty, elementNodes.OfType<XText>().Select(s => s.Value));

                asElement.SetAttributeValue(element.Name.LocalName, newValue);
                element.Remove();
            }

            return true;
        }
    }

    class SortReferences : IXmlVisitor
    {
        public bool BeginFromProject => false;

        public bool Visit(XNode node)
        {
            if (node is XElement asElement && "ItemGroup".Equals(asElement.Name.LocalName, StringComparison.OrdinalIgnoreCase))
            {
                // Check that all child elements are PackageReference & ProjectReference
                List<XNode> childs = asElement.Nodes().ToList();

                if (childs.OfType<XElement>().All(x =>
                    "PackageReference".Equals(x.Name.LocalName, StringComparison.OrdinalIgnoreCase) ||
                    "ProjectReference".Equals(x.Name.LocalName, StringComparison.OrdinalIgnoreCase)))
                {
                    // Sort all elements, while moving all nodes before the element with it
                    List<(string key, XNode[] nodes)> sortList = new List<(string key, XNode[] nodes)>();

                    int lastIdx = 0;
                    for (int i = 0; i < childs.Count; i++)
                    {
                        if (!(childs[i] is XElement childElement))
                            continue;

                        XAttribute includeAttribute = childElement.Attribute("Include");
                        string include = includeAttribute?.Value;

                        if (string.IsNullOrEmpty(include))
                        {
                            // Try as an element named Include
                            include = (childElement.DescendantsAndSelf("Include").FirstOrDefault()?.Nodes().SingleOrDefault() as XText)?.Value;
                        }

                        sortList.Add((include, childs.Skip(lastIdx).Take(1 + i - lastIdx).ToArray()));
                        lastIdx = i + 1;
                    }

                    sortList.Sort((a, b) => a.key.CompareTo(b.key));

                    asElement.ReplaceNodes(sortList.SelectMany(s => s.nodes).ToArray());
                }

            }

            return false;
        }
    }

    internal class Formatter
    {
        private readonly FormatterSettings _settings;
        private readonly UTF8Encoding _encoding = new UTF8Encoding(false);

        public Formatter(FormatterSettings settings = null)
        {
            _settings = settings ?? new FormatterSettings();
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

            while (nodes.TryPop(out XNode node))
            {
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

        public string Format(string source)
        {
            XDocument doc = XDocument.Parse(source);

            List<IXmlVisitor> visitors = new List<IXmlVisitor>();
            visitors.Add(new MergeReferences());
            visitors.Add(new SortReferences());
            visitors.Add(new SplitTopLevels());
            visitors.Add(new Newliner());
            visitors.Add(new Indenter());

            foreach (IXmlVisitor visitor in visitors)
            {
                ProcessVisitors(doc, visitor);
            }

            return doc.ToString(SaveOptions.DisableFormatting);
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

            IEnumerable<string> files = args.SelectMany(s =>
                Directory.EnumerateFiles(s, "*.csproj", SearchOption.AllDirectories)
                    .Concat(Directory.EnumerateFiles(s, "*.targets", SearchOption.AllDirectories)))
                .Take(1);

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

                //break;
            }

            return 0;
        }
    }
}
