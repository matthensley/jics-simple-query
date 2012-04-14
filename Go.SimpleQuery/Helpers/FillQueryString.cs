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

            var currentSession = String.Empty;
            var currentYear = String.Empty;
            if (_queryString.Contains("@@CurrentYear") || _queryString.Contains("@@CurrentSession"))
            {
                var pluginService = new CommonPluginService();
                var currentTerm = pluginService.GetTerms();
                currentSession = currentTerm.Session;
                currentYear = currentTerm.Year.ToString();
            }

            var values = new Dictionary<string, string>()
                             {
                                 {"@@HostID", _user.HostID ?? "0"},
                                 {"@@Username", _user.Username ?? ""},
                                 {"@@EmailAddress", _user.EmailAddress ?? ""},
                                 {"@@DisplayName", _user.DisplayName ?? ""},
                                 {"@@FirstName", _user.FirstName ?? ""},
                                 {"@@MiddleName", _user.MiddleName ?? ""},
                                 {"@@LastName", _user.LastName ?? ""},
                                 {"@@CurrentYear", currentSession},
                                 {"@@CurrentSession", currentYear}
                             };

            foreach (var item in values)
            {
                fqs.Replace(item.Key, item.Value);
            }

            return fqs.ToString();
        }
    }
}