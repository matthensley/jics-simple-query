using System;
using System.IO;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using Jenzabar.Portal.Framework;
using Jenzabar.Portal.Framework.Facade;
using System.Text;
using System.Web.Script.Services;
using Jenzabar.Common;
using CUS.ICS.SimpleQuery.Helpers;

namespace CUS.ICS.SimpleQuery
{
    /// <summary>
    /// Summary description for StudentSearch
    /// </summary>
    [WebService(Namespace = "SimpleQuery")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.Web.Script.Services.ScriptService]
    [System.ComponentModel.ToolboxItem(false)]
    public class Admin : System.Web.Services.WebService
    {
        [WebMethod(EnableSession = true)]
        public object TestConnection(string _connectionFile, Guid _portletID)
        {
            Portlet portlet = Jenzabar.Common.ObjectFactoryWrapper.GetInstance<IPortletFacade>().FindByGuid(_portletID);

            if (portlet.AccessCheck("CanAdminQueries"))
            {

                if (_connectionFile.EndsWith(".config") &&
                    !System.IO.File.Exists(HttpContext.Current.Server.MapPath("~/ClientConfig/" + _connectionFile)))
                {
                    return new
                    {
                        success = false,
                        message = "Connection File specified does not exist in ClientConfig folder.",
                        looked = HttpContext.Current.Server.MapPath("~/ClientConfig/" + _connectionFile).ToString()
                    };

                }
                else
                {
                    try
                    {
                        CUS.OdbcConnectionClass3.OdbcConnectionClass3 odbcConn;

                        if (_connectionFile.EndsWith(".config"))
                            odbcConn = new CUS.OdbcConnectionClass3.OdbcConnectionClass3("~/ClientConfig/" + _connectionFile);
                        else
                            odbcConn = new CUS.OdbcConnectionClass3.OdbcConnectionClass3(_connectionFile);

                        odbcConn.ConnectionTest();
                        return new
                        {
                            success = true,
                            message = "Connection test was successful."
                        };
                    }
                    catch (Exception ex)
                    {
                        return new
                        {
                            success = false,
                            message = "Connection could not be established using the Connection provided.",
                            exception = ex.ToString()
                        };
                    }
                }
            }
            else
            {
                return new
                {
                    success = false,
                    message = "No Permissions to Modify Queries"
                };
            }
        }

        [WebMethod(EnableSession = true)]
        public object TestQuery(string _connectionFile, string _queryString, Guid _portletID, string _expandedColumns, string _columnLabels, string queryTimeout)
        {
            CUS.OdbcConnectionClass3.OdbcConnectionClass3 odbcConn;
            Portlet portlet = Jenzabar.Common.ObjectFactoryWrapper.GetInstance<IPortletFacade>().FindByGuid(_portletID);

            if (portlet.AccessCheck("CanAdminQueries"))
            {
                try
                {
                    if (_connectionFile.Contains(".config"))
                        odbcConn = new CUS.OdbcConnectionClass3.OdbcConnectionClass3("~/ClientConfig/" + _connectionFile);
                    else
                        odbcConn = new CUS.OdbcConnectionClass3.OdbcConnectionClass3( _connectionFile);

                    odbcConn.ConnectionTest();

                    Exception exError = null;
                    try
                    {
                        QuerySafe QS = new QuerySafe();
                        if (QS.IsQuerySafeEnough(_queryString, portlet))
                        {

                            FillQueryString FQS = new FillQueryString(_queryString);

                            DataTable dt = new DataTable();
                            int qt = 0;
                            if (Int32.TryParse(queryTimeout, out qt) && qt > 0)
                                dt = odbcConn.ConnectToERP(FQS.FilledQueryString, ref exError, qt);
                            else
                                dt = odbcConn.ConnectToERP(FQS.FilledQueryString, ref exError);

                            List<String> expandedColumns = new List<String>();
                            List<String> columnLabels = new List<String>();

                            if (_expandedColumns.Trim().Length > 0)
                            {
                                if (_expandedColumns.Contains(','))
                                {
                                    foreach (String column in _expandedColumns.Split(','))
                                    {
                                        expandedColumns.Add(column.Trim());
                                    }
                                }
                                else
                                {
                                    expandedColumns.Add(_expandedColumns.Trim());
                                }
                            }

                            if (_columnLabels.Trim().Length > 0)
                            {
                                if (_columnLabels.Contains(','))
                                {
                                    foreach (String label in _columnLabels.Split(','))
                                    {
                                        columnLabels.Add(label.Trim());
                                    }
                                }
                                else
                                {
                                    columnLabels.Add(_columnLabels.Trim());
                                }
                            }

                            JSDataTableConverter jsdtc = new JSDataTableConverter(dt, expandedColumns.ToArray(), columnLabels.ToArray());

                            JSDataTable data = jsdtc.GetJsDataTable();
                            
                            if (exError != null)
                                throw exError;
                            return new
                            {
                                success = true,
                                message = "Query test was successful. " + dt.Rows.Count.ToString() + " rows returned. ",
                                data = data.data,
                                columns = data.columns,
                                a = expandedColumns.ToArray(),
                                b = columnLabels.ToArray()

                            };
                        }
                        else
                        {
                            return new
                            {
                                success = false,
                                message = "You do not have permissions to create advanced queries that use Update, Delete, Insert, or Execute."
                            };
                        }
                    }
                    catch (Exception ex)
                    {
                        return new
                        {
                            success = false,
                            message = "Query Failed. Test your query using an external tool and paste your corrected version into place. <br>Error:<br>" + ex.Message
                        };
                    }
                }
                catch
                {
                    return new
                    {
                        success = false,
                        message = "Connection failed before query was executed."
                    };
                }
            }
            else
            {
                return new
                {
                    success = false,
                    message = "You do not have permissions to create queries."
                };

            }
        }

       
    }
}
