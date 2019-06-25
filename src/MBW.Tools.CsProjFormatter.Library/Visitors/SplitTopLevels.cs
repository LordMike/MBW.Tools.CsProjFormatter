using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace MBW.Tools.CsProjFormatter.Library.Visitors
{
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
}