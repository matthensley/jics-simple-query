using System;
using System.IO;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Script.Services;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Web.UI;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Jenzabar.Common;
using Jenzabar.ICS.Web.Portlets.Common;
using Jenzabar.Portal.Framework;
using Jenzabar.Portal.Framework.Facade;
using CUS.ICS.SimpleQuery.Helpers;
using CUS.ICS.SimpleQuery.Mappers;
using CUS.ICS.SimpleQuery.Entities;

namespace CUS.ICS.SimpleQuery
{
    /// <summary>
    /// Summary description for StudentSearch
    /// </summary>
    [WebService(Namespace = "SimpleQuery")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.Web.Script.Services.ScriptService]
    [System.ComponentModel.ToolboxItem(false)]
    public class Query : System.Web.Services.WebService
    {

        [WebMethod(EnableSession = true)]
        public object RunQuery(Guid _portletID)
        {
            CUS.OdbcConnectionClass3.OdbcConnectionClass3 odbcConn;
            Portlet portlet = Jenzabar.Common.ObjectFactoryWrapper.GetInstance<IPortletFacade>().FindByGuid(_portletID);

            if (portlet.ParentPage.CanView(PortalUser.Current))
            {
                try
                {
                    NHSimpleQueryMapper mapper = new NHSimpleQueryMapper();
                    NHSimpleQuery NHS = mapper.GetById(_portletID);

                    if (NHS.ConfigFile.Contains(".config"))
                        odbcConn = new CUS.OdbcConnectionClass3.OdbcConnectionClass3("~/ClientConfig/" + NHS.ConfigFile);
                    else
                        odbcConn = new CUS.OdbcConnectionClass3.OdbcConnectionClass3(NHS.ConfigFile);

                    odbcConn.ConnectionTest();

                    Exception exError = null;
                    try
                    {

                        DataTable dt = new DataTable();
                        if (NHS.QueryTimeout > 0)
                            dt = odbcConn.ConnectToERP(NHS.FilledQueryString(), ref exError, NHS.QueryTimeout);
                        else
                            dt = odbcConn.ConnectToERP(NHS.FilledQueryString(), ref exError);

                        StoreExportData(NHS, dt, _portletID, PortletUtilities.GetSettingValue(portlet, "DisplayColumnHeadings") == "Checked", portlet);

                        

                        JSDataTableConverter jsdtc = new JSDataTableConverter(dt, NHS.ExpandedColumns.Split(','), NHS.ColumnLabels.Split(','));

                        JSDataTable data = jsdtc.GetJsDataTable();

                        if (exError != null)
                            throw exError;
                        return new
                        {
                            success = true,
                            data = data.data,
                            columns = data.columns

                        };

                    }
                    catch (Exception ex)
                    {
                        return new
                        {
                            success = false,
                            message = "Query Failed. " + (PortalUser.Current.IsSiteAdmin ? ": " + ex : "")
                        };
                    }
                }
                catch (Exception ex)
                {
                    return new
                    {
                        success = false,
                        message = "Connection failed before query was executed." + (PortalUser.Current.IsSiteAdmin ? ": " + ex : "")
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

        private void StoreExportData(NHSimpleQuery _nhs, DataTable _dt, Guid _key, bool _showHeader, Portlet _portlet)
        {
            String strkey = Jenzabar.Portal.Framework.PortalUser.Current.ID.AsGuid.ToString() + _key.ToString();
            String strQueryTitle = "";
            if (PortletUtilities.GetSettingValue(_portlet, "QueryTitle").Trim().Length > 0)
                strQueryTitle = PortletUtilities.GetSettingValue(_portlet, "QueryTitle").Trim();

            if (strQueryTitle.Trim().Length > 0)
                HttpContext.Current.Session["sqfilename+" + strkey] = Regex.Replace(strQueryTitle, @"\W", "");//remove non-alphanumeric characters from filename
            else
                HttpContext.Current.Session["sqfilename+" + strkey] = "ExportedData";

            switch (_nhs.ResultFormat)
            {
                case "xml":
                    Encoding encoding = Encoding.Unicode;  // other encoding are possible, e.g., utf-8
                    string xml = String.Empty;
                    for (int i = 0; i < _dt.Rows.Count; i++)
                    {
                        xml = xml + _dt.Rows[i][0].ToString();
                    }

                    // pretty print the xml
                    XmlDocument doc = new XmlDocument();
                    try
                    {
                        doc.LoadXml(xml);
                        xml = FormatXml.Format(doc, encoding);
                    }
                    catch
                    {
                        xml = "<xmlroot>" + xml + "</xmlroot>";
                        doc.LoadXml(xml);
                        xml = FormatXml.Format(doc, encoding);
                    }
                    HttpContext.Current.Session["sqhtml+" + strkey] = xml;

                    break;

                case "csv":
                    string csv = String.Empty;
                    if (_showHeader)
                    {
                        for (int i = 0; i < _dt.Columns.Count; i++)
                        {
                            csv = csv + _dt.Columns[i].ColumnName;
                            if (i + 1 < _dt.Columns.Count) csv = csv + ",";
                        }
                        csv = csv + "\n";
                    }
                    for (int i = 0; i < _dt.Rows.Count; i++)
                    {
                        for (int j = 0; j < _dt.Rows[i].ItemArray.Length; j++)
                        {
                            csv = csv + _dt.Rows[i].ItemArray[j].ToString().Trim();
                            if (j + 1 < _dt.Rows[i].ItemArray.Length) csv = csv + ",";
                        }
                        csv = csv + "\n";
                    }
                    HttpContext.Current.Session["sqhtml+" + strkey] = csv;
                    break;

                default:
                    string txt = "Format as ." + _nhs.ResultFormat + " is not implemented";
                    HttpContext.Current.Session["sqhtml+" + strkey] = txt;
                    break;
            }
        }



    }
}
