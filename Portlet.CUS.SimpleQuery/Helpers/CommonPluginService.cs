using System;
using System.Xml.Serialization;
using Jenzabar.CRM.Deserializers;
using Jenzabar.CRM.Utility;

namespace SimpleQuery.Helpers
{
    public interface ICommonPluginService
    {
        Term GetTerms();
    }

    public class CommonPluginService : ICommonPluginService
    {
        public Term GetTerms()
        {
            var ret = new Term();
            var strXML = string.Empty;
            var common = new Jenzabar.ERP.Common();
            common.GetTerms("Student Schedule", ref strXML);
            var tl = (TermList)PlugIn.MapXMLToObject(strXML, new XmlSerializer(typeof(TermList)));
            var termKey = tl.CurrentTermKey;
            var parts = termKey.Split(";".ToCharArray());
            ret.Year = Convert.ToInt32(parts[0]);
            ret.Session = parts[1];
            return ret;
        }
    }

    public class Term
    {
        public Int32 Year { get; set; }
        public String Session { get; set; }
    }
}