using System;
using System.IO;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using System.Xml;
using Jenzabar.Common;
using Jenzabar.ICS.Web.Portlets.Common;
using Jenzabar.Portal.Framework;
using Jenzabar.Portal.Framework.Facade;
using CUS.ICS.SimpleQuery.Helpers;
using CUS.ICS.SimpleQuery.Mappers;
using CUS.ICS.SimpleQuery.Entities;
using System.Web.SessionState;
using Jenzabar.Portal.Framework.Web.UI;

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
            CUS.OdbcConnectionClass3.OdbcConnectionClass3 odbcConn;
            Portlet portlet = Jenzabar.Common.ObjectFactoryWrapper.GetInstance<IPortletFacade>().FindByGuid(_portletID);

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
                        if (dt == null)
                        {
                            return new
                            {
                                success = false,
                                message = "No results returned."
                            };
                        }

                        if (_helper.GetSetting("JICSAllowExports").BoolValue)
                            HttpContext.Current.Session["sqhtml+" + PortalUser.Current.ID.AsGuid + _portletID] = dt;

                        var jsdtc = new JSDataTableConverter(dt, _helper.GetSetting("JICSDataTablesExpandedColumns").Value.Split(','), _helper.GetSetting("ColumnLabels").Value.Split(','));

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
                        Exception ex = null;
                        DataTable dt = GetData(_portletID);

                        String html = string.Empty;

                        if (dt != null && dt.Rows.Count > 0)
                        {
                            if (_helper.GetSetting("JICSAllowExports").BoolValue)
                                HttpContext.Current.Session["sqhtml+" + PortalUser.Current.ID.AsGuid + _portletID] = dt;

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
                                    html = OutputHelper.RenderLiteral(dt,_helper.GetSetting("LiteralFormat", "{0}").Value);
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
            var queryStringFiller = new FillQueryString(_helper.GetSetting("QueryText").Value);

            if (Convert.ToInt16(_helper.GetSetting("QueryTimeout", 0).Value) > 0)
                dt = odbcConn.ConnectToERP(queryStringFiller.FilledQueryString, ref ex, Convert.ToInt16(_helper.GetSetting("QueryTimeout").Value));
            else
                dt = odbcConn.ConnectToERP(queryStringFiller.FilledQueryString, ref ex);

            if (ex != null)
            {
                throw ex;
            }

            return dt;


        }

        //private string StoreExportData(NHSimpleQuery _nhs, DataTable _dt, Guid _key, bool _showHeader, Portlet _portlet)
        //{
        //    String strkey = Jenzabar.Portal.Framework.PortalUser.Current.ID.AsGuid.ToString() + _key.ToString();
        //    String strQueryTitle = "";
        //    if (PortletUtilities.GetSettingValue(_portlet, "QueryTitle").Trim().Length > 0)
        //        strQueryTitle = PortletUtilities.GetSettingValue(_portlet, "QueryTitle").Trim();

        //    if (strQueryTitle.Trim().Length > 0)
        //        HttpContext.Current.Session["sqfilename+" + strkey] = Regex.Replace(strQueryTitle, @"\W", "");//remove non-alphanumeric characters from filename
        //    else
        //        HttpContext.Current.Session["sqfilename+" + strkey] = "ExportedData";

        //    switch (_nhs.ResultFormat)
        //    {
        //        case "xls":


        //            if (PortletUtilities.GetSettingValue(_portlet, "DisplayColumnHeadings") == "Checked")
        //            {
        //                dgResults.ShowHeader = true;
        //            }
        //            else
        //                dgResults.ShowHeader = false;

        //            if (PortletUtilities.GetSettingValue(_portlet, "UseAlternatingRowColor") == "Checked")
        //                dgResults.AlternatingItemStyle.BackColor = dgResults.BorderColor;
        //            else
        //                dgResults.AlternatingItemStyle.BackColor = dgResults.BackColor;

        //            if (PortletUtilities.GetSettingValue(_portlet, "ShowGridBorders") == "Checked")
        //            {
        //                dgResults.GridLines = System.Web.UI.WebControls.GridLines.Both;
        //                dgResults.BorderStyle = System.Web.UI.WebControls.BorderStyle.Solid;
        //            }
        //            else
        //            {
        //                dgResults.GridLines = System.Web.UI.WebControls.GridLines.None;
        //                dgResults.BorderStyle = System.Web.UI.WebControls.BorderStyle.None;
        //            }

        //            if (PortletUtilities.GetSettingValue(_portlet, "CellPadding").Trim().Length > 0)
        //                dgResults.CellPadding = Convert.ToInt16(PortletUtilities.GetSettingValue(_portlet, "CellPadding"));


        //            dgResults.DataSource = _dt;
        //            dgResults.DataBind();
        //            System.IO.StringWriter stringWrite = new System.IO.StringWriter();
        //            System.Web.UI.HtmlTextWriter htmlWrite = new HtmlTextWriter(stringWrite);
        //            dgResults.RenderControl(htmlWrite);

        //            htmlWrite.Flush();
        //            HttpContext.Current.Session["sqhtml+" + strkey] = stringWrite.ToString().Replace("\n", "").Replace("\r", "").Replace("  ", "");
        //            break;

        //        case "xml":
        //            Encoding encoding = Encoding.Unicode;  // other encoding are possible, e.g., utf-8
        //            string xml = String.Empty;

        //            // pretty print the xml
        //            XmlDocument doc = new XmlDocument();
        //            StringWriter writer = new StringWriter();
        //            _dt.TableName = "Row";
        //            _dt.DataSet.DataSetName = "SimpleQueryData";
        //            writer.Write("<?xml version=\"1.0\" encoding=\"utf-16\"?><xmlroot>");
        //            _dt.WriteXml(writer);
        //            writer.Write("</xmlroot>");

        //            try
        //            {
        //                doc.LoadXml(writer.ToString());
        //                xml = FormatXml.Format(doc, encoding);
        //            }
        //            catch (Exception ex)
        //            {
        //                xml = "<xmlroot>" + xml + "</xmlroot>";
        //                xml = FormatXml.Format(doc, encoding);
        //            }


        //            HttpContext.Current.Session["sqhtml+" + strkey] = xml;

        //            break;

        //        case "csv":
        //            string csv = String.Empty;
        //            if (_showHeader)
        //            {
        //                for (int i = 0; i < _dt.Columns.Count; i++)
        //                {
        //                    csv = csv + _dt.Columns[i].ColumnName;
        //                    if (i + 1 < _dt.Columns.Count) csv = csv + ",";
        //                }
        //                csv = csv + "\n";
        //            }
        //            for (int i = 0; i < _dt.Rows.Count; i++)
        //            {
        //                for (int j = 0; j < _dt.Rows[i].ItemArray.Length; j++)
        //                {
        //                    csv = csv + _dt.Rows[i].ItemArray[j].ToString().Trim();
        //                    if (j + 1 < _dt.Rows[i].ItemArray.Length) csv = csv + ",";
        //                }
        //                csv = csv + "\n";
        //            }
        //            HttpContext.Current.Session["sqhtml+" + strkey] = csv;
        //            break;

        //        default:
        //            string txt = "Format as ." + _nhs.ResultFormat + " is not implemented";
        //            HttpContext.Current.Session["sqhtml+" + strkey] = txt;
        //            break;
        //    }
        //    return HttpContext.Current.Session["sqhtml+" + strkey].ToString();
        //}



    }
}