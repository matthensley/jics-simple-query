using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI.WebControls;
using System.Xml;
using Jenzabar.Common;
using Jenzabar.Portal.Framework;
using Jenzabar.Portal.Framework.Web.UI;
using CUS.ICS.SimpleQuery.Mappers;
using CUS.ICS.SimpleQuery.Entities;
using CUS.ICS.SimpleQuery.Helpers;
using LiteralStringReplacer.Facade;


namespace CUS.ICS.SimpleQuery
{
    public partial class Default_View : PortletViewBase
    {

        private Guid _portletId;
        private NHSimpleQuerySettingsMapper _mapper;
        private List<NHSimpleQuerySetting> _settings;
        private SettingsHelper _helper;
        private ILiteralStringReplacer _literalStringReplacer;

        #region Web Form Designer generated code
        override protected void OnInit(EventArgs e)
        {
            lnkViewResults.Click += new EventHandler(this.LnkViewResultsClick);
            lnbGetData.Click += new EventHandler(lnbGetData_Click);
            //
            // CODEGEN: This call is required by the ASP.NET Web Form Designer.
            //
            InitializeComponent();
            base.OnInit(e);
        }

        void lnbGetData_Click(object sender, EventArgs e)
        {
            LoadData();
        }

        /// <summary>
        ///		Required method for Designer support - do not modify
        ///		the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {

        }
        #endregion

        private void LnkViewResultsClick(object sender, EventArgs e)
        {
            this.ParentPortlet.NextScreen("Results");
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            _portletId = this.ParentPortlet.PortletDisplay.Portlet.ID.AsGuid;
            _mapper = new NHSimpleQuerySettingsMapper();
            _settings = _mapper.GetSettings(_portletId).ToList();
            _helper = new SettingsHelper(_settings, _portletId, _mapper);
            _literalStringReplacer = ObjectFactoryWrapper.GetInstance<ILiteralStringReplacer>();
            pnlResults.Visible = false;
            pnlLinkDescription.Visible = false;
            pnlDataTableResults.Visible = false;
            pnlQueryTitle.Visible = false;

            String strQueryTitle = String.Empty;

            if (_settings.Count == 0 || _settings.Find(x => x.Name == "QueryText") == null || _settings.Find(x => x.Name == "QueryText").Value == String.Empty)
            {
                this.ParentPortlet.ShowFeedback(FeedbackType.Message, "This portlet is not yet configured. Contact portal administrator.");
                return;
            }


            if (_helper.GetSetting("JICSAllowExports", false).BoolValue)
            {
                if (_helper.GetSetting("ExportXls", false).BoolValue)
                {
                    pnlExport.Visible = true;
                    lnkSQExportExcel.Visible = true;
                    lnkSQExportExcel.NavigateUrl = "Export_Data.aspx?format=xls&sqkey=" + this.ParentPortlet.Portlet.ID.AsGuid;
                }

                if (_helper.GetSetting("ExportCsv", false).BoolValue)
                {
                    pnlExport.Visible = true;
                    lnkSQExportCsv.Visible = true;
                    lnkSQExportCsv.NavigateUrl = "Export_Data.aspx?format=csv&sqkey=" + this.ParentPortlet.Portlet.ID.AsGuid;
                }

                if (_helper.GetSetting("ExportXml", false).BoolValue)
                {
                    pnlExport.Visible = true;
                    lnkSQExportXml.Visible = true;
                    lnkSQExportXml.NavigateUrl = "Export_Data.aspx?format=xml&sqkey=" + this.ParentPortlet.Portlet.ID.AsGuid;
                }

                if (_helper.GetSetting("ExportLiteral", false).BoolValue)
                {
                    pnlExport.Visible = true;
                    lnkSQExportLiteral.Visible = true;
                    lnkSQExportLiteral.NavigateUrl = "Export_Data.aspx?format=literal&sqkey=" + this.ParentPortlet.Portlet.ID.AsGuid;
                }
            }


            if (_helper.GetSetting("QueryTitle").Value.Trim().Length > 0)
            {
                lblQueryTitle.Text = _helper.GetSetting("QueryTitle").Value;
                pnlQueryTitle.Visible = true;
            }


            if (_helper.GetSetting("JICSOutput", "grid").Value != "datatables")
            {
                if (_helper.GetSetting("JICSAsync", false).BoolValue)
                {
                    if (ShouldRenderOutput())
                    {
                        hdnUseAJAX.Value = "Y";
                        lnbGetData.Visible = true;
                    }
                }
                else
                {
                    if (ShouldRenderOutput())
                    {
                        RenderOutput();
                    }
                }
            }
            else
            {
                if (ShouldRenderOutput())
                    pnlDataTableResults.Visible = true;
            }


        }

        private void LoadData()
        {
            if (ShouldRenderOutput())
            {
                RenderOutput();
                lnbGetData.Visible = false;
            }

        }

        private bool ShouldRenderOutput()
        {
            if (this.ParentPortlet.State == PortletState.Maximized ||
                 (_helper.GetSetting("JICSDisplayResultsMinimized", false).BoolValue))
            {
                return true;
            }

            pnlLinkDescription.Visible = true;

            lnkViewResults.Text = _helper.GetSetting("JICSLinkText").Value.Trim().Length == 0 ? "View Results" : _helper.GetSetting("JICSLinkText").Value;

            if (_helper.GetSetting("JICSDescription").Value.Trim().Length > 0)
                lblQueryDescription.Text = _helper.GetSetting("JICSDescription").Value;

            return false;
        }
        private void RenderOutput()
        {
            if (!ShouldRenderOutput()) return;
            DataTable dt = null;
            try
            {
                dt = GetData();
            }
            catch (Exception ex)
            {
                this.ParentPortlet.ShowFeedback(FeedbackType.Error, "Query Failed. Contact portal administrator. " + ex);
                return;
            }

            //if the below is true then at least one row was returned
            if (dt != null && dt.Rows.Count > 0)
            {
                if (_helper.GetSetting("JICSAllowExports").BoolValue)
                    HttpContext.Current.Session["sqhtml+" + PortalUser.Current.ID.AsGuid + this.ParentPortlet.Portlet.ID.AsGuid] = dt;

                if (Convert.ToInt32(_helper.GetSetting("RowLimit", 0).Value) > 0)
                    dt = dt.AsEnumerable().Take(Convert.ToInt32(_helper.GetSetting("RowLimit", 0).Value)).CopyToDataTable();

                switch (_helper.GetSetting("JICSOutput", "grid").Value)
                {
                    case "grid":
                        OutputHelper.ConfigureDataGrid(ref dgResults,
                                                                       dt,
                                                                       _helper.GetSetting("JICSGridShowColumnHeadings", false).BoolValue,
                                                                       _helper.GetSetting("JICSGridAltRowColors", false).BoolValue,
                                                                       _helper.GetSetting("JICSGridShowGridlines", false).BoolValue,
                                                                       Convert.ToInt16(_helper.GetSetting("JICSGridCellPadding", 5).Value),
                                                                       _helper.GetSetting("ColumnLabels").Value);

                        dgResults.DataSource = dt;
                        dgResults.DataBind();
                        dgResults.Visible = true;

                        break;
                    case "xml":
                        preformattedResults.Text = "<pre>" + HttpUtility.HtmlEncode(OutputHelper.RenderXml(dt)) + "</pre>";
                        break;
                    case "csv":
                        preformattedResults.Text = "<pre>" + OutputHelper.RenderCsv(dt,
                                                      _helper.GetSetting("JICSGridShowColumnHeadings", false).BoolValue,
                                                      _helper.GetSetting("ColumnLabels").Value) + "</pre>";
                        break;
                    case "literal":
                        preformattedResults.Text = OutputHelper.RenderLiteral(dt, _helper.GetSetting("ExportLiteralPattern", "{0}").Value);
                        break;
                    case "template":
                        preformattedResults.Text = _literalStringReplacer.Process(OutputHelper.RenderTemplate(dt, 
                                                                                                                _helper.GetSetting("JICSTemplateHeader").Value, 
                                                                                                                _helper.GetSetting("JICSTemplateRow").Value, 
                                                                                                                _helper.GetSetting("JICSTemplateFooter").Value), ParentPortlet.Portlet);
                        break;
                }
                pnlResults.Visible = true;
            }
            else
            {
                message2.Visible = true;
                //If no matching records are found, they are shown a message
                if (ParentPortlet.AccessCheck("CanAdminQueries") || PortalUser.Current.IsSiteAdmin == true)
                {
                    message2.InnerHtml = "No results returned by this query.<br />Query text (visible by Query Admin only):<br />" + Regex.Replace(Regex.Replace(_helper.GetSetting("QueryText").Value, "<", "&lt;", RegexOptions.Multiline), ">", "&gt;", RegexOptions.Multiline);
                }
                else
                    message2.InnerHtml = "No results returned by this query.";
                return;
            }
        }

        private DataTable GetData()
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
                this.ParentPortlet.ShowFeedback(FeedbackType.Error, "Unable to locate usable connection string. Contact portal administrator.");
                throw;
            }

            try
            {
                odbcConn.ConnectionTest();
            }
            catch
            {
                this.ParentPortlet.ShowFeedback(FeedbackType.Error, "Database connection test failed. Contact portal administrator.");
                throw;
            }

            var ex = new Exception();
            try
            {
                

                var queryString = _literalStringReplacer.Process(_helper.GetSetting("QueryText").Value, ParentPortlet.Portlet);
                if (Convert.ToInt16(_helper.GetSetting("QueryTimeout", 0).Value) > 0)
                    return odbcConn.ConnectToERP(queryString, ref ex, Convert.ToInt16(_helper.GetSetting("QueryTimeout").Value));
                else
                    return odbcConn.ConnectToERP(queryString, ref ex);
            }
            catch (Exception ee)
            {
                this.ParentPortlet.ShowFeedback(FeedbackType.Error, "Query Failed. Contact portal administrator. " + ex.ToString() + ee.ToString());
                throw;
            }
        }
    }
}