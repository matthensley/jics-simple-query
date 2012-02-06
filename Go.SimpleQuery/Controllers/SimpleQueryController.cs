using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using CUS.ICS.SimpleQuery.Helpers;
using CUS.ICS.SimpleQuery.Mappers;
using Go.SimpleQuery.Models;
using Jenzabar.Portal.Framework.Facade;
using mobile.Controllers;
using mobile.Infrastructure.Attributes;

namespace Go.SimpleQuery.Controllers
{
    [PortletView("[CUS] SimpleQuery")]
    public class SimpleQueryController : Controller
    {
        private readonly IPortalUserFacade _userFacade;
        private readonly NHSimpleQuerySettingsMapper _mapper; 
        public SimpleQueryController(IPortalUserFacade userFacade)
        {
            _userFacade = userFacade;
            _mapper = new NHSimpleQuerySettingsMapper();
        }

        public ActionResult Index(ContentIdentifier cid, IPrincipal iPrincipal)
        {
            var helper = GetHelper(cid.PortletId.Value);

            ViewBag.Title = helper.GetSetting("QueryTitle").Value;

            switch (helper.GetSetting("GOOutput").Value)
            {
                case "grid":
                    DataTable dt;
                    try
                    {
                        dt = GetData(helper, iPrincipal.Identity.Name);
                    }
                    catch (Exception ex)
                    {
                        return View("Error",
                                    new Error
                                        {
                                            ErrorMessage = "An error occurred while querying the database.",
                                            Exception =
                                                _userFacade.FindByUsername(iPrincipal.Identity.Name).IsSiteAdmin
                                                    ? ex.ToString().Replace("\n", "<br />")
                                                    : String.Empty
                                        });
                    }
                    var gridmodel = new SimpleQueryDataGrid
                                    {
                                        CellPadding = Convert.ToInt16(helper.GetSetting("GOSGridCellPadding", 5).Value),
                                        ShowAltColors = helper.GetSetting("GOGridAltRowColors", false).BoolValue,
                                        ShowColumnHeaders =
                                            helper.GetSetting("GOGridShowColumnHeadings", false).BoolValue,
                                        ShowGridlines = helper.GetSetting("GOGridShowGridlines", false).BoolValue,
                                        AllowExports = helper.GetSetting("GOAllowExports").BoolValue,
                                        Data = dt
                                    };
                    return View("DataGrid", gridmodel);
                    break;
                case "masterdetail":
                    List<SimpleQueryDataRow> dataRows;
                    try
                    {
                        dataRows = GetMasterDetailData(helper, iPrincipal.Identity.Name);
                    }
                    catch (Exception ex)
                    {
                        return View("Error",
                                    new Error
                                        {
                                            ErrorMessage = "An error occurred while querying the database.",
                                            Exception =
                                                _userFacade.FindByUsername(iPrincipal.Identity.Name).IsSiteAdmin
                                                    ? ex.ToString().Replace("\n","<br />")
                                                    : String.Empty
                                        });
                    }
                    var mdmodel = new SimpleQueryMasterDetail
                                      {
                                          AllowExports = helper.GetSetting("GOAllowExports").BoolValue,
                                          MasterDetailData = dataRows
                                      };
                    return View("MasterDetail", mdmodel);
                    break;
                case "xml":
                    return Xml(cid, iPrincipal);
                case "csv":
                    return Csv(cid, iPrincipal);
                case "literal":
                    return Literal(cid, iPrincipal);
                case "none":
                    return View("Error", new Error { ErrorMessage = "Results for this portlet are not enabled for JICS Go" });
                default:
                    return View("Error", new Error { ErrorMessage = "This portlet has not yet been configured." });
            }

            //DataTable dt = GetData(helper);
            return View();
        }

        public ActionResult Xml(ContentIdentifier cid, IPrincipal iPrincipal)
        {
            var helper = GetHelper(cid.PortletId.Value);
            DataTable dt;
            try
            {
                dt = GetData(helper, iPrincipal.Identity.Name);
            }
            catch (Exception ex)
            {
                return View("Error",
                            new Error
                            {
                                ErrorMessage = "An error occurred while querying the database.",
                                Exception =
                                    _userFacade.FindByUsername(iPrincipal.Identity.Name).IsSiteAdmin
                                        ? ex.ToString().Replace("\n", "<br />")
                                        : String.Empty
                            });
            }
            var model = new SimpleQueryLiteral
                            {
                                Html = "<pre>" + HttpUtility.HtmlEncode(OutputHelper.RenderXml(dt)) + "</pre>",
                                AllowExports = helper.GetSetting("GOAllowExports").BoolValue
                            };
            return View("Literal", model);
        }

        public ActionResult Csv(ContentIdentifier cid, IPrincipal iPrincipal)
        {
            var helper = GetHelper(cid.PortletId.Value);
            DataTable dt;
            try
            {
                dt = GetData(helper, iPrincipal.Identity.Name);
            }
            catch (Exception ex)
            {
                return View("Error",
                            new Error
                            {
                                ErrorMessage = "An error occurred while querying the database.",
                                Exception =
                                    _userFacade.FindByUsername(iPrincipal.Identity.Name).IsSiteAdmin
                                        ? ex.ToString().Replace("\n", "<br />")
                                        : String.Empty
                            });
            }
            var model = new SimpleQueryLiteral
                            {
                                Html = "<pre>" + OutputHelper.RenderCsv(dt,
                                                              helper.GetSetting("GOGridShowColumnHeadings", false).
                                                                  BoolValue,
                                                              helper.GetSetting("ColumnLabels").Value) + "</pre>",
                                AllowExports = helper.GetSetting("GOAllowExports").BoolValue
                            };
            return View("Literal", model);
        }

        public ActionResult Literal(ContentIdentifier cid, IPrincipal iPrincipal)
        {
            var helper = GetHelper(cid.PortletId.Value);
            DataTable dt;
            try
            {
                dt = GetData(helper, iPrincipal.Identity.Name);
            }
            catch (Exception ex)
            {
                return View("Error",
                            new Error
                            {
                                ErrorMessage = "An error occurred while querying the database.",
                                Exception =
                                    _userFacade.FindByUsername(iPrincipal.Identity.Name).IsSiteAdmin
                                        ? ex.ToString().Replace("\n", "<br />")
                                        : String.Empty
                            });
            }
            var model = new SimpleQueryLiteral
            {
                Html = OutputHelper.RenderLiteral(dt, helper.GetSetting("ExportLiteralPattern", "{0}").Value),
                AllowExports = helper.GetSetting("GOAllowExports").BoolValue
            };
            return View("Literal", model);
        }

        public ActionResult Export(ContentIdentifier cid, IPrincipal iPrincipal)
        {
            var helper = GetHelper(cid.PortletId.Value);
            var model = new SimpleQueryExport
                            {
                                AllowExports = helper.GetSetting("GOAllowExports").BoolValue,
                                ExportTypes = AllowedExports(helper)
                            };
            return View(model);
        }

        public ActionResult ExportFile(String fileType, ContentIdentifier cid, IPrincipal iPrincipal)
        {
            var helper = GetHelper(cid.PortletId.Value);
            List<String> allowedTypes = AllowedExports(helper);
            if (allowedTypes.Contains(fileType))
            {
                DataTable dt;
                try
                {
                    dt = GetData(helper, iPrincipal.Identity.Name);
                }
                catch (Exception ex)
                {
                    return View("Error",
                                new Error
                                    {
                                        ErrorMessage = "An error occurred while querying the database.",
                                        Exception =
                                            _userFacade.FindByUsername(iPrincipal.Identity.Name).IsSiteAdmin
                                                ? ex.ToString().Replace("\n", "<br />")
                                                : String.Empty
                                    });
                }
                var mstream = new MemoryStream();
                var sw = new StreamWriter(mstream);
                var strContentType = "text/plain"; // these defaults will be overwritten if we're successful
                var strFilename = "ErrorOutput.txt";
                string fileName;
                if (helper.GetSetting("QueryTitle").Value.Trim().Length > 0)
                    fileName = Regex.Replace(helper.GetSetting("QueryTitle").Value.Trim(), @"\W", "");//remove non-alphanumeric characters from filename
                else
                    fileName = "ExportedData";

                switch (fileType)
                {
                    case "Xls":
                        var dgResults = OutputHelper.CreateDataGrid();

                        OutputHelper.ConfigureDataGrid(ref dgResults,
                                                            dt,
                                                            helper.GetSetting("GOGridShowColumnHeadings", false).BoolValue,
                                                            helper.GetSetting("GOGridAltRowColors", false).BoolValue,
                                                            helper.GetSetting("GOGridShowGridlines", false).BoolValue,
                                                            Convert.ToInt16(helper.GetSetting("GOGridCellPadding", 5).Value),
                                                            helper.GetSetting("ColumnLabels").Value);


                        dgResults.DataSource = dt;
                        dgResults.DataBind();

                        var stringWrite = new StringWriter();
                        var htmlWrite = new HtmlTextWriter(stringWrite);
                        dgResults.RenderControl(htmlWrite);

                        htmlWrite.Flush();

                        sw.WriteLine(stringWrite.ToString().Replace("\n", "").Replace("\r", "").Replace("  ", ""));
                        strContentType = "application/vnd.ms-excel";
                        strFilename = fileName + ".xls";
                        break;
                    case "Xml":
                        sw.WriteLine(OutputHelper.RenderXml(dt));
                        strContentType = "text/xml";
                        strFilename = fileName + ".xml";
                        break;
                    case "Csv":
                        sw.WriteLine(OutputHelper.RenderCsv(dt,
                                                      helper.GetSetting("GOGridShowColumnHeadings", false).BoolValue,
                                                      helper.GetSetting("ColumnLabels").Value));
                        strContentType = "text/csv";
                        strFilename = fileName + ".csv";
                        break;
                    case "Literal":
                        sw.WriteLine(OutputHelper.RenderLiteral(dt, helper.GetSetting("ExportLiteralPattern", "{0}").Value));
                        strContentType = "text/plain";
                        strFilename = fileName + ".txt";
                        break;

                }

                sw.Flush();
                sw.Close();

                byte[] byteArray = mstream.ToArray();

                mstream.Flush();
                mstream.Close();

                return File(byteArray, strContentType, strFilename);
            }

            return View("Error", new Error { ErrorMessage = "File Export Type Not Allowed" });
        }

        private static List<String> AllowedExports(SettingsHelper helper)
        {
            var exports = new List<String>();
            if (helper.GetSetting("ExportXls", false).BoolValue)
            {
                //exports.Add("Xls"); //Disabled for the time being.  The method currently employed to produce "excel" files is fundamentally flawed and really needs to be revisted.
            }

            if (helper.GetSetting("ExportCsv", false).BoolValue)
            {
                exports.Add("Csv");
            }

            if (helper.GetSetting("ExportXml", false).BoolValue)
            {
                exports.Add("Xml");
            }

            if (helper.GetSetting("ExportLiteral", false).BoolValue)
            {
                exports.Add("Literal");
            }

            return exports;
        }

        private DataTable GetData(SettingsHelper helper, string username)
        {
            //Try and connect
            CUS.OdbcConnectionClass3.OdbcConnectionClass3 odbcConn;
            if (helper.GetSetting("ConfigFile").Value.EndsWith(".config"))
                odbcConn = new CUS.OdbcConnectionClass3.OdbcConnectionClass3("~/ClientConfig/" + helper.GetSetting("ConfigFile").Value);
            else
                odbcConn = new CUS.OdbcConnectionClass3.OdbcConnectionClass3(helper.GetSetting("ConfigFile").Value);

            odbcConn.ConnectionTest();

            var ex = new Exception();
            var queryStringFiller = new Helpers.FillQueryString(helper.GetSetting("QueryText").Value,
                                                                               _userFacade.FindByUsername(username));
            DataTable dt;
            if (Convert.ToInt16(helper.GetSetting("QueryTimeout", 0).Value) > 0)
                dt = odbcConn.ConnectToERP(queryStringFiller.FilledQueryString, ref ex, Convert.ToInt16(helper.GetSetting("QueryTimeout").Value));
            else
                dt =  odbcConn.ConnectToERP(queryStringFiller.FilledQueryString, ref ex);
            if (ex != null)
            {
                throw ex;
            }
            return dt;

        }

        private SettingsHelper GetHelper(Guid portletId)
        {
            // Get the NHS object
            try
            {
                var settings = _mapper.GetSettings(portletId).ToList();
                return new SettingsHelper(settings, portletId, _mapper);
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.ToString();
                throw;
            }
        }

        private List<SimpleQueryDataRow> GetMasterDetailData(SettingsHelper helper, string username)
        {
            var ret = new List<SimpleQueryDataRow>();
            var dt = GetData(helper, username);
            var detailColumns = helper.GetSetting("GOMasterDetailDisplayColumns").Value.Split(',');
            var columnLabels = helper.GetSetting("ColumnLabels").Value.Split(',');

            for (var r = 0; r < dt.Rows.Count; r++)
            {
                var row = new SimpleQueryDataRow {Master = dt.Rows[r][0].ToString()};
                foreach (var col in dt.Columns.Cast<DataColumn>())
                {
                    string columnName;
                    try
                    {
                        columnName = columnLabels[col.Ordinal];
                    }
                    catch
                    {
                        columnName = col.ColumnName;
                    }
                    if (detailColumns.Contains(columnName))
                        row.Details.Add(new SimpleQueryDataColumn
                                            {Value = dt.Rows[r][col.Ordinal].ToString(), Name = columnName});

                }
                ret.Add(row);
            }
            return ret;
        }
    }
}
