using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using MBW.Tools.CsProjFormatter.Library.Configuration;

namespace MBW.Tools.CsProjFormatter.Library.Visitors
{
    class Indenter : IXmlVisitor
    {
        private readonly char _char;
        private readonly int _count;
        private readonly Dictionary<int, XText> _indentsCache;

        public Indenter(IndentStyle style, int count)
        {
            _char = style == IndentStyle.Space ? ' ' : '\t';
            _count = count;
            _indentsCache = new Dictionary<int, XText>();
        }

        public bool BeginFromProject => false;

        private XText GetIndentText(int indents)
        {
            if (_indentsCache.TryGetValue(indents, out var txt))
                return txt;

            int characters = indents * _count;
            char[] newText = new char[characters];
            for (var i = 0; i < newText.Length; i++)
                newText[i] = _char;

            return _indentsCache[indents] = new XText(new string(newText));
        }

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

            // Indent before this node
            node.AddBeforeSelf(GetIndentText(parents));

            // If this node is an Element, and has childs, we need to indent the end tag as well
            if (node is XElement asElement && asElement.HasElements)
            {
                asElement.Add(GetIndentText(parents));
            }

            return true;
        }
    }
}