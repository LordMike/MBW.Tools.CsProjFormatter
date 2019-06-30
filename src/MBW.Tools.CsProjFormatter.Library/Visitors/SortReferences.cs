using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;

namespace MBW.Tools.CsProjFormatter.Library.Visitors
{
    class SortReferences : IXmlVisitor
    {
        private readonly ILogger _logger;
        public bool BeginFromProject => false;

        public SortReferences(ILogger logger)
        {
            _logger = logger;
        }

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
                    bool doSort = true;
                    for (int i = 0; i < childs.Count; i++)
                    {
                        if (!(childs[i] is XElement childElement))
                            continue;

                        string packageName = GetAttributeOrChild(childElement, "Include");

                        if (string.IsNullOrEmpty(packageName))
                            packageName = GetAttributeOrChild(childElement, "Update");

                        if (string.IsNullOrEmpty(packageName))
                            packageName = GetAttributeOrChild(childElement, "Remove");

                        if (string.IsNullOrEmpty(packageName))
                        {
                            // Error, do not sort this
                            _logger.LogWarning("File {File} has references that cannot be sorted");
                            doSort = false;
                            break;
                        }

                        sortList.Add((packageName, childs.Skip(lastIdx).Take(1 + i - lastIdx).ToArray()));
                        lastIdx = i + 1;
                    }

                    if (doSort)
                    {
                        sortList.Sort((a, b) => a.key.CompareTo(b.key));

                        asElement.ReplaceNodes(sortList.SelectMany(s => s.nodes).ToArray());
                    }
                }

            }

            return false;
        }

        private string GetAttributeOrChild(XElement element, string name)
        {
            XAttribute includeAttribute = element.Attribute(name);
            string include = includeAttribute?.Value;

            if (string.IsNullOrEmpty(include))
            {
                // Try as an element named Include
                include = (element.DescendantsAndSelf(name).FirstOrDefault()?.Nodes().SingleOrDefault() as XText)?.Value;
            }

            return include;
        }
    }
}