using System;
using System.Collections.Generic;
using System.Text;
using Jenzabar.Portal.Framework;
using SimpleQuery.Helpers;

namespace Go.SimpleQuery.Helpers
{
    public class FillQueryString
    {
        private readonly String _queryString;
        private readonly PortalUser _user;
        public String Error { get; private set; }

        public FillQueryString(string queryString, PortalUser user)
        {
            this._queryString = queryString;
            this._user = user;
        }

        public string FilledQueryString()
        {
            var fqs = new StringBuilder(_queryString);
            var pluginService = new CommonPluginService();
            var currentTerm = pluginService.GetTerms();

            var values = new Dictionary<string, string>()
                             {
                                 {"@@HostID", _user.HostID ?? "0"},
                                 {"@@Username", _user.Username ?? ""},
                                 {"@@EmailAddress", _user.EmailAddress ?? ""},
                                 {"@@DisplayName", _user.DisplayName ?? ""},
                                 {"@@FirstName", _user.FirstName ?? ""},
                                 {"@@MiddleName", _user.MiddleName ?? ""},
                                 {"@@LastName", _user.LastName ?? ""},
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