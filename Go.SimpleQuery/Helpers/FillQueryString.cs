using System;
using System.Text.RegularExpressions;
using Jenzabar.CRM.Deserializers;
using Jenzabar.CRM.Utility;
using Jenzabar.Portal.Framework;
using System.Xml.Serialization;

namespace Go.SimpleQuery.Helpers
{
    public class FillQueryString
    {
        private String queryString;
        private PortalUser user;
        public String Error { get; private set; }

        public FillQueryString(string _queryString, PortalUser user)
        {
            this.queryString = _queryString;
            this.user = user;
        }

        public String FilledQueryString { 
            get {
                if (user.HostID != null && user.HostID.Trim().Length > 0)
                {
                    queryString = Regex.Replace(queryString, "@@HostID", user.HostID, RegexOptions.Multiline);
                }
                else
                {
                    if (Regex.Match(queryString, "@@HostID", RegexOptions.Multiline).ToString().Trim().Length > 0)
                    {
                        queryString = Regex.Replace(queryString, "@@HostID", "0", RegexOptions.Multiline);
                    }
                }

                if (!string.IsNullOrEmpty(user.Username))
                    queryString = Regex.Replace(queryString, "@@Username", user.Username, RegexOptions.Multiline);
                if (!string.IsNullOrEmpty(user.EmailAddress))
                    queryString = Regex.Replace(queryString, "@@EmailAddress", user.EmailAddress, RegexOptions.Multiline);
                if (!string.IsNullOrEmpty(user.DisplayName))
                    queryString = Regex.Replace(queryString, "@@DisplayName", user.DisplayName, RegexOptions.Multiline);
                if (!string.IsNullOrEmpty(user.FirstName))
                    queryString = Regex.Replace(queryString, "@@FirstName", user.FirstName, RegexOptions.Multiline);
                if (!string.IsNullOrEmpty(user.LastName))
                    queryString = Regex.Replace(queryString, "@@LastName", user.LastName, RegexOptions.Multiline);
                if (!string.IsNullOrEmpty(user.MiddleName))
                    queryString = Regex.Replace(queryString, "@@MiddleName", user.MiddleName, RegexOptions.Multiline);
                if (queryString.IndexOf("@@CurrentYear") > 0 || queryString.IndexOf("@@CurrentSession") > 0)
                {
                    string strCurYr = "0";
                    string strCurSess = String.Empty;
                    try
                    {
                        var strXML = string.Empty;
                        var common = new Jenzabar.ERP.Common();
                        common.GetTerms("Student Schedule", ref strXML);
                        var tl = (TermList)PlugIn.MapXMLToObject(strXML, new XmlSerializer(typeof(TermList)));
                        string termKey = tl.CurrentTermKey;
                        string[] parts = termKey.Split(";".ToCharArray());
                        strCurYr = parts[0];
                        strCurSess = parts[1];
                    }
                    catch (Exception err)
                    {
                        this.Error = "Current Session and Year Lookup Failed. Contact portal administrator. " + err.Message;
                        return "";
                    }
                    queryString = Regex.Replace(queryString, "@@CurrentYear", strCurYr);
                    queryString = Regex.Replace(queryString, "@@CurrentSession", strCurSess);
                }
                return queryString;
            } 
        }
    }
}