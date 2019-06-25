using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace MBW.Tools.CsProjFormatter.Library.Visitors
{
    class MergePackageReferenceAttributes : IXmlVisitor
    {
        public bool BeginFromProject => false;

        public bool Visit(XNode node)
        {
            if (!(node is XElement asElement) ||
                !asElement.HasElements ||
                !"PackageReference".Equals(asElement.Name.LocalName, StringComparison.OrdinalIgnoreCase))
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
}