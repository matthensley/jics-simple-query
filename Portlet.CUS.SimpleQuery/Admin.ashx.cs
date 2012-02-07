using System;
using System.IO;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Services;
using Jenzabar.Portal.Framework;
using Jenzabar.Portal.Framework.Facade;
using CUS.ICS.SimpleQuery.Helpers;
using System.Web.SessionState;

namespace CUS.ICS.SimpleQuery
{
    /// <summary>
    /// Summary description for Admin1
    /// </summary>
    public class Admin1 : IHttpHandler, IRequiresSessionState
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json; charset=utf-8";
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer {MaxJsonLength = 2097152};

            switch (context.Request.Params["action"])
            {
                case "TestConnection":
                    if (context.Request.Params["_portletID"] != null && context.Request.Params["_connectionFile"] != null)
                    {
                        context.Response.Write("{ \"d\" : " + serializer.Serialize(TestConnection(context.Request.Params["_connectionFile"], new Guid(context.Request.Params["_portletID"]))) + " } ");
                    }
                    break;
                case "TestQuery":
                    if (context.Request.Params["_portletID"] != null && context.Request.Params["_connectionFile"] != null && context.Request.Params["_queryString"] != null && context.Request.Params["_expandedColumns"] != null && context.Request.Params["_columnLabels"] != null)
                    {
                        context.Response.Write("{ \"d\" : " + serializer.Serialize(TestQuery(context.Request.Params["_connectionFile"], context.Request.Params["_queryString"], new Guid(context.Request.Params["_portletID"]), context.Request.Params["_expandedColumns"], context.Request.Params["_columnLabels"], context.Request.Params["_queryTimeout"], context.Request.Params["_hostId"])) + " } ");
                    }
                    break;
                default:
                    context.Response.Write("{ \"d\" : { success: false, message: \"Action Not Defined\" } }");
                    break;
            }

        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        public object TestConnection(string _connectionFile, Guid _portletID)
        {
            Portlet portlet = Jenzabar.Common.ObjectFactoryWrapper.GetInstance<IPortletFacade>().FindByGuid(_portletID);

            if (portlet.AccessCheck("CanAdminQueries"))
            {
                if (_connectionFile.EndsWith(".config") &&
                    !File.Exists(HttpContext.Current.Server.MapPath("~/ClientConfig/" + _connectionFile)))
                {
                    return new
                    {
                        success = false,
                        message = "Connection File specified does not exist in ClientConfig folder.",
                        looked = HttpContext.Current.Server.MapPath("~/ClientConfig/" + _connectionFile).ToString()
                    };

                }

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

            return new
                       {
                           success = false,
                           message = "No Permissions to Modify Queries"
                       };
        }

        [WebMethod(EnableSession = true)]
        public object TestQuery(string _connectionFile, string _queryString, Guid _portletID, string _expandedColumns, string _columnLabels, string _queryTimeout, string _testHostId)
        {
            Portlet portlet = Jenzabar.Common.ObjectFactoryWrapper.GetInstance<IPortletFacade>().FindByGuid(_portletID);

            if (portlet.AccessCheck("CanAdminQueries"))
            {
                try
                {
                    CUS.OdbcConnectionClass3.OdbcConnectionClass3 odbcConn;
                    if (_connectionFile.Contains(".config"))
                        odbcConn = new CUS.OdbcConnectionClass3.OdbcConnectionClass3("~/ClientConfig/" + _connectionFile);
                    else
                        odbcConn = new CUS.OdbcConnectionClass3.OdbcConnectionClass3( _connectionFile);

                    odbcConn.ConnectionTest();

                    try
                    {
                        var qs = new QuerySafe();
                        if (qs.IsQuerySafeEnough(_queryString, portlet))
                        {

                            var fqs = new FillQueryString(_queryString, _testHostId);

                            Exception exError = null;
                            DataTable dt;
                            var qt = 0;
                            if (Int32.TryParse(_queryTimeout, out qt) && qt > 0)
                                dt = odbcConn.ConnectToERP(fqs.FilledQueryString(), ref exError, qt);
                            else
                                dt = odbcConn.ConnectToERP(fqs.FilledQueryString(), ref exError);
                            if (exError != null)
                            {
                                return new
                                {
                                    success = false,
                                    message = "Query Test Failed. " + exError.Message + " " + exError.StackTrace
                                };
                            }
                            if (dt == null)
                            {
                                return new
                                {
                                    success = true,
                                    message = "Query test was successful, but no results were returned."
                                };
                            } 
                            var expandedColumns = new List<String>();
                            var columnLabels = new List<String>();

                            if (_expandedColumns.Trim().Length > 0)
                            {
                                if (_expandedColumns.Contains(','))
                                {
                                    expandedColumns.AddRange(_expandedColumns.Split(',').Select(column => column.Trim()));
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
                                    columnLabels.AddRange(_columnLabels.Split(',').Select(label => label.Trim()));
                                }
                                else
                                {
                                    columnLabels.Add(_columnLabels.Trim());
                                }
                            }

                            var jsdtc = new JSDataTableConverter(dt, expandedColumns.ToArray(), columnLabels.ToArray());

                            var data = jsdtc.GetJsDataTable();

                            return new
                            {
                                success = true,
                                message = "Query test was successful. " + dt.Rows.Count.ToString() + " rows returned. ", 
                                data.data, 
                                data.columns
                            };
                        }

                        return new
                                   {
                                       success = false,
                                       message = "You do not have permissions to create advanced queries that use Update, Delete, Insert, or Execute."
                                   };
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

            return new
                       {
                           success = false,
                           message = "You do not have permissions to create queries."
                       };
        }

       
    }
}