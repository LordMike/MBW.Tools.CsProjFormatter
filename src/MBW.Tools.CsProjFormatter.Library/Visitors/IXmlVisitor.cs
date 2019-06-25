using System.Xml.Linq;

namespace MBW.Tools.CsProjFormatter.Library.Visitors
{
    interface IXmlVisitor
    {
        bool BeginFromProject { get; }

        bool Visit(XNode node);
    }
}