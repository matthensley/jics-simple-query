using System;
using System.Collections.Generic;
using System.Text;
using SimpleQuery.Helpers;
using Jenzabar.Portal.Framework;

namespace CUS.ICS.SimpleQuery.Helpers
{
    public class FillQueryString
    {
        private readonly String _queryString;
        private readonly String _hostId;
        public String Error { get; private set; }

        public FillQueryString(string queryString, string hostId = "")
        {
            this._queryString = queryString;
            this._hostId = hostId.Trim() != string.Empty ? hostId : PortalUser.Current.HostID;
        }

        public string FilledQueryString()
        {
            var fqs = new StringBuilder(_queryString);
            var pluginService = new CommonPluginService();
            var currentTerm = pluginService.GetTerms();

            var values = new Dictionary<string, string>()
                             {
                                 {"@@HostID", _hostId ?? "0"},
                                 {"@@Username", PortalUser.Current.Username ?? ""},
                                 {"@@EmailAddress", PortalUser.Current.EmailAddress ?? ""},
                                 {"@@DisplayName", PortalUser.Current.DisplayName ?? ""},
                                 {"@@FirstName", PortalUser.Current.FirstName ?? ""},
                                 {"@@MiddleName", PortalUser.Current.MiddleName ?? ""},
                                 {"@@LastName", PortalUser.Current.LastName ?? ""},
                                 {"@@CurrentYear", currentTerm.Year.ToString()},
                                 {"@@CurrentSession", currentTerm.Session}
                             };

            foreach (var item in values)
            {
                fqs.Replace(item.Key, item.Value);
            }

            return fqs.ToString();
        }

    }
}