using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;
using Jenzabar.CRM.Utility; //PlugIn method 
using Jenzabar.CRM.Deserializers; //TermList class
using Jenzabar.Common;
using Jenzabar.Portal.Framework;
using System.Xml.Serialization;

namespace CUS.ICS.SimpleQuery.Helpers
{
    public class FillQueryString
    {
        private String queryString;
        private String hostId;
        public String Error { get; private set; }

        public FillQueryString(string _queryString, string _hostID = "")
        {
            this.queryString = _queryString;
            this.hostId = _hostID.Trim() != string.Empty ? _hostID : PortalUser.Current.HostID;
        }

        public String FilledQueryString { 
            get {
                if (hostId != null && hostId.Trim().Length > 0)
                {
                    queryString = Regex.Replace(queryString, "@@HostID", hostId, RegexOptions.Multiline);
                }
                else
                {
                    if (Regex.Match(queryString, "@@HostID", RegexOptions.Multiline).ToString().Trim().Length > 0)
                    {
                        queryString = Regex.Replace(queryString, "@@HostID", "0", RegexOptions.Multiline);
                    }
                }

                if (!string.IsNullOrEmpty(PortalUser.Current.Username))
                    queryString = Regex.Replace(queryString, "@@Username", PortalUser.Current.Username, RegexOptions.Multiline);
                if (!string.IsNullOrEmpty(PortalUser.Current.EmailAddress))
                    queryString = Regex.Replace(queryString, "@@EmailAddress", PortalUser.Current.EmailAddress, RegexOptions.Multiline);
                if (!string.IsNullOrEmpty(PortalUser.Current.DisplayName))
                    queryString = Regex.Replace(queryString, "@@DisplayName", PortalUser.Current.DisplayName, RegexOptions.Multiline);
                if (!string.IsNullOrEmpty(PortalUser.Current.FirstName))
                    queryString = Regex.Replace(queryString, "@@FirstName", PortalUser.Current.FirstName, RegexOptions.Multiline);
                if (!string.IsNullOrEmpty(PortalUser.Current.LastName))
                    queryString = Regex.Replace(queryString, "@@LastName", PortalUser.Current.LastName, RegexOptions.Multiline);
                if (!string.IsNullOrEmpty(PortalUser.Current.MiddleName))
                    queryString = Regex.Replace(queryString, "@@MiddleName", PortalUser.Current.MiddleName, RegexOptions.Multiline);
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