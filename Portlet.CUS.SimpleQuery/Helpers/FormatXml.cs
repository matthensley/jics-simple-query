using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.Text;

namespace CUS.ICS.SimpleQuery.Helpers
{
    internal static class FormatXml
    {
        public static string Format(XmlDocument doc, Encoding encoding)
        {
            // Format the Xml document with indentation and save it to a string.
            StringWriterWithEncoding textWriter = new StringWriterWithEncoding(encoding);
            XmlTextWriter xmlWriter = new XmlTextWriter(textWriter);
            xmlWriter.Formatting = Formatting.Indented;
            doc.Save(xmlWriter);
            return textWriter.ToString();
        }
    }
}