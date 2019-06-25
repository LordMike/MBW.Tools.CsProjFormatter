using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace MBW.Tools.CsProjFormatter.Library.Visitors
{
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
}