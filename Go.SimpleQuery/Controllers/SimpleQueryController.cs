using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using CUS.ICS.SimpleQuery.Helpers;
using CUS.ICS.SimpleQuery.Mappers;
using Go.SimpleQuery.Models;
using Jenzabar.Portal.Framework.Facade;
using mobile.Helpers;
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

            ViewBag.Title = helper.GetSetting("QueryTitle").Value + " - " + iPrincipal.Identity.Name;
            switch (helper.GetSetting("GOOutput").Value)
            {
                case "grid":
                    var model = new SimpleQueryDataGrid
                                    {
                                        CellPadding = Convert.ToInt16(helper.GetSetting("GOSGridCellPadding", 5).Value),
                                        ShowAltColors = helper.GetSetting("GOGridAltRowColors", false).BoolValue,
                                        ShowColumnHeaders =
                                            helper.GetSetting("GOGridShowColumnHeadings", false).BoolValue,
                                        ShowGridlines = helper.GetSetting("GOGridShowGridlines", false).BoolValue,
                                        AllowExports = helper.GetSetting("GOAllowExports").BoolValue,
                                        ExportTypes = AllowedExports(helper),
                                        Data = GetData(helper, iPrincipal.Identity.Name)
                                    };
                    return View("DataGrid", model);
                    break;
                case "masterdetail":
                    //return None();
                    break;
                case "none":
                    return View("None");
                case "xml":
                    return xmlAction(cid, iPrincipal);
                case "csv":
                    return csvAction(cid, iPrincipal);
                case "literal":
                    return literalAction(cid, iPrincipal);
            }

            //DataTable dt = GetData(helper);
            return View();
        }

        public ActionResult xmlAction(ContentIdentifier cid, IPrincipal iPrincipal)
        {
            var helper = GetHelper(cid.PortletId.Value);
            var model = new SimpleQueryLiteral
                            {
                                Html = "<pre>" + HttpUtility.HtmlEncode(OutputHelper.RenderXml(GetData(helper, iPrincipal.Identity.Name))) +"</pre>",
                                ExportTypes = AllowedExports(helper),
                                AllowExports = helper.GetSetting("GOAllowExports").BoolValue
                            };
            return View("Literal", model);
        }

        public ActionResult csvAction(ContentIdentifier cid, IPrincipal iPrincipal)
        {
            var helper = GetHelper(cid.PortletId.Value);
            var model = new SimpleQueryLiteral
                            {
                                Html = "<pre>" + OutputHelper.RenderCsv(GetData(helper, iPrincipal.Identity.Name),
                                                              helper.GetSetting("GOGridShowColumnHeadings", false).
                                                                  BoolValue,
                                                              helper.GetSetting("ColumnLabels").Value) + "</pre>",
                                ExportTypes = AllowedExports(helper),
                                AllowExports = helper.GetSetting("GOAllowExports").BoolValue
                            };
            return View("Literal", model);
        }

        public ActionResult literalAction(ContentIdentifier cid, IPrincipal iPrincipal)
        {
            var helper = GetHelper(cid.PortletId.Value);
            var model = new SimpleQueryLiteral
            {
                Html = OutputHelper.RenderLiteral(GetData(helper, iPrincipal.Identity.Name), helper.GetSetting("LiteralFormat", "{0}").Value),
                ExportTypes = AllowedExports(helper),
                AllowExports = helper.GetSetting("GOAllowExports").BoolValue
            };
            return View("Literal", model);
        }

        private static List<String> AllowedExports(SettingsHelper _helper)
        {
            var exports = new List<String>();
            if (_helper.GetSetting("ExportXls", false).BoolValue)
            {
                exports.Add("Xls");
            }

            if (_helper.GetSetting("ExportCsv", false).BoolValue)
            {
                exports.Add("Csv");
            }

            if (_helper.GetSetting("ExportXml", false).BoolValue)
            {
                exports.Add("Xml");
            }

            if (_helper.GetSetting("ExportLiteral", false).BoolValue)
            {
                exports.Add("Literal");
            }

            return exports;
        }

        private DataTable GetData(SettingsHelper _helper, string username)
        {
            //Try and connect
            CUS.OdbcConnectionClass3.OdbcConnectionClass3 odbcConn;
            if (_helper.GetSetting("ConfigFile").Value.EndsWith(".config"))
                odbcConn = new CUS.OdbcConnectionClass3.OdbcConnectionClass3("~/ClientConfig/" + _helper.GetSetting("ConfigFile").Value);
            else
                odbcConn = new CUS.OdbcConnectionClass3.OdbcConnectionClass3(_helper.GetSetting("ConfigFile").Value);

            odbcConn.ConnectionTest();

            var ex = new Exception();
            var queryStringFiller = new Helpers.FillQueryString(_helper.GetSetting("QueryText").Value,
                                                                               _userFacade.FindByUsername(username));
            if (Convert.ToInt16(_helper.GetSetting("QueryTimeout", 0).Value) > 0)
                return odbcConn.ConnectToERP(queryStringFiller.FilledQueryString, ref ex, Convert.ToInt16(_helper.GetSetting("QueryTimeout").Value));
            else
                return odbcConn.ConnectToERP(queryStringFiller.FilledQueryString, ref ex);
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
                ViewBag["error"] = ex.ToString();
                throw;
            }
        }
    }
}
