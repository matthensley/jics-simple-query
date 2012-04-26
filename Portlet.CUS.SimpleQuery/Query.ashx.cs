using System;
using System.IO;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using Jenzabar.Common;
using Jenzabar.Portal.Framework;
using Jenzabar.Portal.Framework.Facade;
using CUS.ICS.SimpleQuery.Helpers;
using CUS.ICS.SimpleQuery.Mappers;
using System.Web.SessionState;
using LiteralStringReplacer.Facade;

namespace CUS.ICS.SimpleQuery
{
    /// <summary>
    /// Summary description for Query1
    /// </summary>
    public class Query1 : IHttpHandler, IRequiresSessionState
    {

        public void ProcessRequest(HttpContext context)
        {

            context.Response.ContentType = "application/json; charset=utf-8";
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer {MaxJsonLength = 2097152};

            switch (context.Request.Params["action"])
            {
                case "RunQuery":
                    if (context.Request.Params["_portletID"] != null)
                    {
                        context.Response.Write("{ \"d\" : " + serializer.Serialize(RunQuery(new Guid(context.Request.Params["_portletID"]))) + " } ");
                    }
                    break;
                case "RunQueryHTML":
                    if (context.Request.Params["_portletID"] != null)
                    {
                        context.Response.Write("{ \"d\" : " + serializer.Serialize(RunQueryHTML(new Guid(context.Request.Params["_portletID"]))) + " } ");
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


        public object RunQuery(Guid _portletID)
        {
            Portlet portlet = ObjectFactoryWrapper.GetInstance<IPortletFacade>().FindByGuid(_portletID);

            if (portlet.ParentPage.CanView(PortalUser.Current))
            {
                try
                {

                    var mapper = new NHSimpleQuerySettingsMapper();
                    var settings = mapper.GetSettings(_portletID).ToList();
                    var helper = new SettingsHelper(settings, _portletID, mapper);

                    try
                    {
                        DataTable dt = GetData(_portletID);
                        if (dt == null)
                        {
                            return new
                            {
                                success = false,
                                message = "No results returned."
                            };
                        }

                        if (helper.GetSetting("JICSAllowExports").BoolValue)
                            HttpContext.Current.Session["sqhtml+" + PortalUser.Current.ID.AsGuid + _portletID] = dt;

                        if (Convert.ToInt32(helper.GetSetting("RowLimit", 0).Value) > 0)
                            dt = dt.AsEnumerable().Take(Convert.ToInt32(helper.GetSetting("RowLimit", 0).Value)).CopyToDataTable();

                        var jsdtc = new JSDataTableConverter(dt, helper.GetSetting("JICSDataTablesExpandedColumns").Value.Split(','), helper.GetSetting("ColumnLabels").Value.Split(','));

                        var data = jsdtc.GetJsDataTable();

                        return new
                        {
                            success = true,
                            data.data,
                            data.columns
                        };

                    }
                    catch (Exception ex)
                    {
                        return new
                        {
                            success = false,
                            message = "Query results handling failed. " + (PortalUser.Current.IsSiteAdmin ? ": " + ex.Message + " " + ex.StackTrace : "")
                        };
                    }
                }
                catch (Exception ex)
                {
                    return new
                    {
                        success = false,
                        message = "Connection failed before query was executed. " + (PortalUser.Current.IsSiteAdmin ? ": " + ex.Message + " " + ex.StackTrace : "")
                    };
                }
            }
            else
            {
                return new
                {
                    success = false,
                    message = "You do not have permissions to view this portlet"
                };

            }
        }



        public object RunQueryHTML(Guid _portletID)
        {
            Portlet portlet = ObjectFactoryWrapper.GetInstance<IPortletFacade>().FindByGuid(_portletID);

            if (portlet.ParentPage.CanView(PortalUser.Current))
            {
                try
                {

                    var mapper = new NHSimpleQuerySettingsMapper();
                    var settings = mapper.GetSettings(_portletID).ToList();
                    var _helper = new SettingsHelper(settings, _portletID, mapper);


                    try
                    {
                        DataTable dt = GetData(_portletID);

                        String html = string.Empty;

                        if (dt != null && dt.Rows.Count > 0)
                        {
                            if (_helper.GetSetting("JICSAllowExports").BoolValue)
                                HttpContext.Current.Session["sqhtml+" + PortalUser.Current.ID.AsGuid + _portletID] = dt;

                            if (Convert.ToInt32(_helper.GetSetting("RowLimit", 0).Value) > 0)
                                dt = dt.AsEnumerable().Take(Convert.ToInt32(_helper.GetSetting("RowLimit", 0).Value)).CopyToDataTable();

                            switch (_helper.GetSetting("JICSOutput", "grid").Value)
                            {
                                case "grid":
                                    var dgResults = OutputHelper.CreateDataGrid();

                                    OutputHelper.ConfigureDataGrid(ref dgResults,
                                                                        dt, 
                                                                        _helper.GetSetting("JICSGridShowColumnHeadings", false).BoolValue,
                                                                        _helper.GetSetting("JICSGridAltRowColors", false).BoolValue, 
                                                                        _helper.GetSetting("JICSGridShowGridlines", false).BoolValue, 
                                                                        Convert.ToInt16(_helper.GetSetting("JICSGridCellPadding", 5).Value),
                                                                        _helper.GetSetting("ColumnLabels").Value); 

                                    
                                    dgResults.DataSource = dt;
                                    dgResults.DataBind();

                                    var stringWrite = new StringWriter();
                                    var htmlWrite = new HtmlTextWriter(stringWrite);
                                    dgResults.RenderControl(htmlWrite);

                                    htmlWrite.Flush();

                                    html = stringWrite.ToString().Replace("\n", "").Replace("\r", "").Replace("  ", "");
                                    break;
                                case "xml":
                                    html = "<pre>" + HttpUtility.HtmlEncode(OutputHelper.RenderXml(dt)) + "</pre>";
                                    break;
                                case "csv":
                                    html = "<pre>" + OutputHelper.RenderCsv(dt,
                                                                  _helper.GetSetting("JICSGridShowColumnHeadings", false).BoolValue,
                                                                  _helper.GetSetting("ColumnLabels").Value) + "</pre>";
                                    break;
                                case "literal":
                                    html = OutputHelper.RenderLiteral(dt, _helper.GetSetting("ExportLiteralPattern", "{0}").Value);
                                    break;

                            }
                            return new
                            {
                                success = true,
                                resultFormat = _helper.GetSetting("JICSOutput", "grid").Value, 
                                html
                            };
                        }

                        return new
                                   {
                                       success = true,
                                       resultFormat = _helper.GetSetting("JICSOutput", "grid").Value, 
                                       html,
                                       query =
                                            (PortalUser.Current.IsSiteAdmin
                                                      ? _helper.GetSetting("QueryText").Value
                                                      : "")
                                   };

                    }
                    catch (Exception ex)
                    {
                        return new
                        {
                            success = false,
                            message = "Query Failed. " + (PortalUser.Current.IsSiteAdmin ? ": " + ex.StackTrace : "")
                        };
                    }
                }
                catch (Exception ex)
                {
                    return new
                    {
                        success = false,
                        message = "Connection failed before query was executed." + (PortalUser.Current.IsSiteAdmin ? ": " + ex.StackTrace : "")
                    };
                }
            }

            return new
                       {
                           success = false,
                           message = "You do not have permissions to view this portlet"
                       };
        }

        private DataTable GetData( Guid _portletID)
        {
            var mapper = new NHSimpleQuerySettingsMapper();
            var settings = mapper.GetSettings(_portletID).ToList();
            var _helper = new SettingsHelper(settings, _portletID, mapper);

            OdbcConnectionClass3.OdbcConnectionClass3 odbcConn;
            if (_helper.GetSetting("ConfigFile").Value.EndsWith(".config"))
                odbcConn = new CUS.OdbcConnectionClass3.OdbcConnectionClass3("~/ClientConfig/" + _helper.GetSetting("ConfigFile").Value);
            else
                odbcConn = new CUS.OdbcConnectionClass3.OdbcConnectionClass3(_helper.GetSetting("ConfigFile").Value);

            odbcConn.ConnectionTest();

            Exception ex = null;
            DataTable dt;
            var literalStringReplacer = ObjectFactoryWrapper.GetInstance<ILiteralStringReplacer>();

            var queryString = literalStringReplacer.Process(_helper.GetSetting("QueryText").Value);

            if (Convert.ToInt16(_helper.GetSetting("QueryTimeout", 0).Value) > 0)
                dt = odbcConn.ConnectToERP(queryString, ref ex, Convert.ToInt16(_helper.GetSetting("QueryTimeout").Value));
            else
                dt = odbcConn.ConnectToERP(queryString, ref ex);

            if (ex != null)
            {
                throw ex;
            }

            return dt;


        }

    }
}