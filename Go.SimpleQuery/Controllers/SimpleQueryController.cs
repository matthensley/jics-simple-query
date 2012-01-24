using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Principal;
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


            SettingsHelper helper = GetHelper(cid.PortletId.Value);

            switch (helper.GetSetting("GOOutput").Value)
            {
                case "grid":
                    break;
                case "masterdetail":
                    break;
                case "none":
                    return None();
                    break;
                default:
                    var model = new Models.SimpleQuery
                                    {
                                        Action = helper.GetSetting("GOOutput").Value,
                                        AllowExports = helper.GetSetting("GOAllowExports").BoolValue,
                                        ExportTypes = AllowedExports(helper)
                                    };
                    return View(model);
                    break;
            }
            DataTable dt = GetData(helper);
            //switch (_helper.GetSetting("GOOutput").Value)
            //{
            //    case "grid":
            //        break;
            //    case "xml":
            //        return XmlAction(dt);
            //        break;
            //    case "csv":
            //        return CsvAction(_helper, dt);
            //        break;
            //    case "literal":
            //        return LiteralAction(_helper, dt);
            //        break;
            //}

            return View();
        }

        private ActionResult None()
        {
            return View();
        }


        public ActionResult XmlAction(ContentIdentifier cid)
        {
            var helper = GetHelper(cid.PortletId.Value);
            return Content(OutputHelper.RenderXml(GetData(helper)), "text/xml");
        }

        public ActionResult CsvAction(ContentIdentifier cid)
        {
            var helper = GetHelper(cid.PortletId.Value);
            return Content(OutputHelper.RenderCsv(GetData(helper),
                                                  helper.GetSetting("GOGridShowColumnHeadings", false).BoolValue,
                                                  helper.GetSetting("ColumnLabels").Value), "text/csv");
        }

        public ActionResult LiteralAction(ContentIdentifier cid)
        {
            var helper = GetHelper(cid.PortletId.Value);
            var model = new SimpleQueryLiteral
                            {Html = OutputHelper.RenderLiteral(GetData(helper), helper.GetSetting("LiteralFormat", "{0}").Value),
                            ExportTypes = AllowedExports(helper),
                             AllowExports = helper.GetSetting("GOAllowExports").BoolValue
                            };
            return View(model);
        }

        private static List<String> AllowedExports (SettingsHelper _helper)
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

        private static DataTable GetData(SettingsHelper _helper)
        {
            //Try and connect
            CUS.OdbcConnectionClass3.OdbcConnectionClass3 odbcConn;
            try
            {
                if (_helper.GetSetting("ConfigFile").Value.EndsWith(".config"))
                    odbcConn = new CUS.OdbcConnectionClass3.OdbcConnectionClass3("~/ClientConfig/" + _helper.GetSetting("ConfigFile").Value);
                else
                    odbcConn = new CUS.OdbcConnectionClass3.OdbcConnectionClass3(_helper.GetSetting("ConfigFile").Value);
            }
            catch
            {
                //this.ParentPortlet.ShowFeedback(FeedbackType.Error, "Unable to locate usable connection string. Contact portal administrator.");
                throw new Exception();
            }

            try
            {
                odbcConn.ConnectionTest();
            }
            catch
            {
                //this.ParentPortlet.ShowFeedback(FeedbackType.Error, "Database connection test failed. Contact portal administrator.");
                throw new Exception();
            }

            var ex = new Exception();
            try
            {
                var queryStringFiller = new FillQueryString(_helper.GetSetting("QueryText").Value);
                if (Convert.ToInt16(_helper.GetSetting("QueryTimeout", 0).Value) > 0)
                    return odbcConn.ConnectToERP(queryStringFiller.FilledQueryString, ref ex, Convert.ToInt16(_helper.GetSetting("QueryTimeout").Value));
                else
                    return odbcConn.ConnectToERP(queryStringFiller.FilledQueryString, ref ex);
            }
            catch (Exception ee)
            {
                //this.ParentPortlet.ShowFeedback(FeedbackType.Error, "Query Failed. Contact portal administrator. " + ex.ToString() + ee.ToString());
                throw new Exception();
            }
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
