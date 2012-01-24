using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Jenzabar.Portal.Framework;
using System.Text.RegularExpressions;

namespace CUS.ICS.SimpleQuery.Helpers
{
    public class QuerySafe
    {
        public bool IsQuerySafeEnough(string strQueryText, Portlet portlet)
        {
            if (portlet.AccessCheck("CanAdminAdvQueries") || PortalUser.Current.IsSiteAdmin == true)
                return true;
            else
            {
                if (Regex.Match(strQueryText, "DELETE", RegexOptions.IgnoreCase).ToString().Trim().Length > 0 ||
                    Regex.Match(strQueryText, "INSERT", RegexOptions.IgnoreCase).ToString().Trim().Length > 0 ||
                    Regex.Match(strQueryText, "EXECUTE", RegexOptions.IgnoreCase).ToString().Trim().Length > 0 ||
                    Regex.Match(strQueryText, "DROP", RegexOptions.IgnoreCase).ToString().Trim().Length > 0 ||
                    Regex.Match(strQueryText, "UPDATE", RegexOptions.IgnoreCase).ToString().Trim().Length > 0)
                    return false;
                else
                    return true;
            }
        }
    }
}