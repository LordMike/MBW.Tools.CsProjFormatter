using System;
using System.Xml;
using System.Xml.Linq;

namespace MBW.Tools.CsProjFormatter.Library.Visitors
{
    class Newliner : IXmlVisitor
    {
        private readonly XText _newlineText;

        public Newliner(string newlineCharacter)
        {
            _newlineText = new XText(newlineCharacter);
        }

        public bool BeginFromProject => true;

        public bool Visit(XNode node)
        {
            if (node is XElement asElement)
            {
                if (asElement.HasElements)
                    asElement.Add(_newlineText);

                if (!"Project".Equals(asElement.Name.LocalName, StringComparison.OrdinalIgnoreCase))
                    node.AddBeforeSelf(_newlineText);
            }
            else if (node.NodeType == XmlNodeType.Comment)
                node.AddBeforeSelf(_newlineText);

            return true;
        }
    }
}