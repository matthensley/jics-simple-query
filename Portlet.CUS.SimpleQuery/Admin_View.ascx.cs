using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Xml;
using Jenzabar.Common;
using Jenzabar.Portal.Framework.InstalledApplications;
using Jenzabar.Portal.Framework.Web.UI;
using CUS.ICS.SimpleQuery.Mappers;
using CUS.ICS.SimpleQuery.Entities;
using LiteralStringReplacer.Facade;

namespace CUS.ICS.SimpleQuery
{
    public partial class Admin_View : PortletViewBase
    {
        private  Guid _portletId;
        private  NHSimpleQuerySettingsMapper _mapper;
        private List<NHSimpleQuerySetting> _settings;
        private  HtmlInputRadioButton[] _jicsOutputTypes;
        private  HtmlInputRadioButton[] _goOutputTypes;
        private SettingsHelper _helper;

        public override string ViewName
        {
            get
            {
                return "Admin";
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            _portletId = this.ParentPortlet.PortletDisplay.Portlet.ID.AsGuid;
            _mapper = new NHSimpleQuerySettingsMapper();
            _settings = _mapper.GetSettings(_portletId).ToList();
            _helper = new SettingsHelper(_settings, _portletId, _mapper);
            _jicsOutputTypes = new[] { rbJICSOutputCSV, rbJICSOutputDataTables, rbJICSOutputGrid, rbJICSOutputXML, rbJICSOutputLiteral };
            _goOutputTypes = new[] { rbGOOutputXML, rbGOOutputNone, rbGOOutputMasterDetail, rbGOOutputGrid, rbGOOutputCSV, rbGOOutputLiteral };

            // Don't show the Go settings unless it is version 1.2 or greater.
            if (new InstalledApplicationService().IsApplicationAtLeastThisVersion("JICS Go", "1.2"))
            {
                pnlGoSettings.Visible = true;
            }
            if (IsFirstLoad)
            {
                SetDdlConfigFiles();
                ShowCurrentValues();
                var literalStringReplacer = ObjectFactoryWrapper.GetInstance<ILiteralStringReplacer>();
                rptLiteralStringReplacementsAvailable.DataSource = literalStringReplacer.GetAvailableLiterals().Where(x => new[] { "@@HostID", "@@FirstName", "@@LastName", "@@Username", "@@EmailAddress", "@@DisplayName", "@@CurrentYear", "@@CurrentSession" }.Contains(x.Key));
                rptLiteralStringReplacementsAvailable.DataBind();


            }
        }

        #region Web Form Designer generated code
        override protected void OnInit(EventArgs e)
        {
            btnSave.Click += new EventHandler(btnSave_Click);
            btnCancel.Click += new EventHandler(btnCancel_Click);
            btnSaveAndExit.Click += new EventHandler(btnSaveAndExit_Click);
            InitializeComponent();
            base.OnInit(e);
        }

        void btnSaveAndExit_Click(object sender, EventArgs e)
        {
            Save();
            this.ParentPortlet.PreviousScreen();
        }

        void btnCancel_Click(object sender, EventArgs e)
        {
            this.ParentPortlet.PreviousScreen();
        }



        /// <summary>
        ///		Required method for Designer support - do not modify
        ///		the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {

        }
        #endregion



        void btnSave_Click(object sender, EventArgs e)
        {
            Save();
            ShowCurrentValues();
        }

        private void ShowCurrentValues()
        {
            try
            {
                if (_helper.GetSetting("ConfigFile").Value.Contains(".config")){
                    ddlConfigFile.SelectedValue = _helper.GetSetting("ConfigFile").Value;
                    tbConfigFile.Attributes["style"] = "display:none";
                }
                else
                {
                    ddlConfigFile.SelectedValue = "";
                    tbConfigFile.Text = _helper.GetSetting("ConfigFile").Value;
                    tbConfigFile.Attributes["style"] = "";
                }

                tbQueryTitle.Text = _helper.GetSetting("QueryTitle").Value;
                txtQuery.Text = _helper.GetSetting("QueryText").Value;
                txtColumnLabels.Text = _helper.GetSetting("ColumnLabels").Value;
                tbQueryTimeout.Text = _helper.GetSetting("QueryTimeout", 0).Value;

                tbRowLimit.Text = _helper.GetSetting("RowLimit", 0).Value;
                chkDisplayResultsMinimized.Checked =_helper.GetSetting("JICSDisplayResultsMinimized", false).BoolValue;
                if (chkDisplayResultsMinimized.Checked)
                {
                    trLinkText.Attributes["style"] = "display:none";
                    trLinkDescription.Attributes["style"] = "display:none";
                }else
                {
                    trLinkText.Attributes["style"] = "";
                    trLinkDescription.Attributes["style"] = "";
                }

                chkExportXls.Checked = _helper.GetSetting("ExportXls", false).BoolValue;
                chkExportXml.Checked = _helper.GetSetting("ExportXml", false).BoolValue;
                chkExportCsv.Checked = _helper.GetSetting("ExportCsv", false).BoolValue;
                chkExportLiteral.Checked = _helper.GetSetting("ExportLiteral",false).BoolValue;
                if (chkExportLiteral.Checked)
                    trLiteralFormat.Attributes["style"] = "";
                else
                {
                    trLiteralFormat.Attributes["style"] = "display:none;";
                }
                
                tbExportLiteralPattern.Text = _helper.GetSetting("ExportLiteralPattern", "{0}").Value;

                chkJICSAllowExports.Checked = _helper.GetSetting("JICSAllowExports", false).BoolValue;
                tbLinkText.Text = _helper.GetSetting("JICSLinkText").Value;
                tbDescription.Text = _helper.GetSetting("JICSDescription").Value;
                chkJICSAsync.Checked = _helper.GetSetting("JICSAsync", false).BoolValue;

                chkJICSGridShowGridlines.Checked = _helper.GetSetting("JICSGridShowGridlines", false).BoolValue;
                tbJICSGridCellPadding.Text = _helper.GetSetting("JICSGridCellPadding", 5).Value;
                chkJICSGridAltRowColors.Checked = _helper.GetSetting("JICSGridAltRowColors", false).BoolValue;
                chkJICSShowColumnHeadings.Checked = _helper.GetSetting("JICSGridShowColumnHeadings", false).BoolValue;
                tbJICSDataTablesExpandedColumns.Text = _helper.GetSetting("JICSDataTablesExpandedColumns").Value;

                using (var htmlInputRadioButton = _jicsOutputTypes.SingleOrDefault(x => x.Value == _helper.GetSetting("JICSOutput", "grid").Value))
                {
                    if (htmlInputRadioButton != null)
                    {
                        htmlInputRadioButton.Checked = true;
                        divJICSOutputGridSettings.Attributes["style"] = "display:none;margin-left:20px;";
                        divJICSOutputDataTablesSettings.Attributes["style"] = "display:none;margin-left:20px;";
                        switch (htmlInputRadioButton.Value)
                        {
                            case "grid":
                                divJICSOutputGridSettings.Attributes["style"] = "margin-left:20px;";
                                break;
                            case "datatables":
                                divJICSOutputDataTablesSettings.Attributes["style"] = "margin-left:20px;";
                                break;
                        }
                    }
                }

                chkGOAllowExports.Checked = _helper.GetSetting("GOAllowExports", false).BoolValue;
                chkGOGridShowGridlines.Checked = _helper.GetSetting("GOGridShowGridlines", false).BoolValue;
                tbGOGridCellPadding.Text = _helper.GetSetting("GOGridCellPadding", 5).Value;
                chkGOGridAltRowColors.Checked = _helper.GetSetting("GOGridAltRowColors", false).BoolValue;
                chkGOGridShowColumnHeadings.Checked = _helper.GetSetting("GOGridShowColumnHeadings", false).BoolValue;
                tbGOOutputMasterDetailDisplayColumns.Text = _helper.GetSetting("GOMasterDetailDisplayColumns").Value;

                using (var htmlInputRadioButton = _goOutputTypes.SingleOrDefault( x=> x.Value == _helper.GetSetting("GOOutput", "none").Value) )
                {
                    if (htmlInputRadioButton != null)
                    {
                        htmlInputRadioButton.Checked = true;
                        divGOOutputGridSettings.Attributes["style"] = "display:none;margin-left:20px;";
                        divGOOutputMasterDetailSettings.Attributes["style"] = "display:none;margin-left:20px;";
                        switch (htmlInputRadioButton.Value)
                        {
                            case "grid":
                                divGOOutputGridSettings.Attributes["style"] = "margin-left:20px;";

                                break;
                            case "masterdetail":
                                divGOOutputMasterDetailSettings.Attributes["style"] = "margin-left:20px;";
                                
                                break;
                        }
                    }
                }



            }
            catch (Exception ex)
            {
                ddlConfigFile.SelectedIndex = 0;
                txtQuery.Text = string.Empty;
                txtColumnLabels.Text = string.Empty;
                this.ParentPortlet.ShowFeedback(FeedbackType.Error, "Couldn't Load Settings: " + ex);
            }
        }

        private void Save()
        {
            _mapper.UpdateSetting(_helper.GetSetting("QueryTitle"), tbQueryTitle.Text);
            _mapper.UpdateSetting(_helper.GetSetting("QueryText"), txtQuery.Text);


            if (ddlConfigFile.SelectedValue == "") //Set to Other
            {
                _mapper.UpdateSetting(_helper.GetSetting("ConfigFile"), tbConfigFile.Text);
            }
            else
            {
                tbConfigFile.Attributes.CssStyle.Remove("display");
                _mapper.UpdateSetting(_helper.GetSetting("ConfigFile"), ddlConfigFile.SelectedValue);
                tbConfigFile.Text = "";
            }


            var i = 0;
            _mapper.UpdateSetting(_helper.GetSetting("QueryTimeout", 0),
                                  Int32.TryParse(tbQueryTimeout.Text, out i) ? i.ToString() : "0");

            i = 0;
            _mapper.UpdateSetting(_helper.GetSetting("RowLimit", 0),
                                  Int32.TryParse(tbRowLimit.Text, out i) ? i.ToString() : "0");

            _mapper.UpdateSetting(_helper.GetSetting("JICSDisplayResultsMinimized", false), chkDisplayResultsMinimized.Checked);

            var columnLabels = new List<String>();
            if (txtColumnLabels.Text.Trim().Length > 0)
            {
                if (txtColumnLabels.Text.Contains(","))
                {
                    columnLabels.AddRange(txtColumnLabels.Text.Split(',').Select(column => column.Trim()));
                }
                else
                {
                    columnLabels.Add(txtColumnLabels.Text.Trim());
                }
            }
            _mapper.UpdateSetting(_helper.GetSetting("ColumnLabels"), String.Join(",", columnLabels.ToArray()));
            txtColumnLabels.Text = String.Join(",", columnLabels.ToArray());


            _mapper.UpdateSetting(_helper.GetSetting("ExportXls"), chkExportXls.Checked);
            _mapper.UpdateSetting(_helper.GetSetting("ExportXml"), chkExportXml.Checked);
            _mapper.UpdateSetting(_helper.GetSetting("ExportCsv"), chkExportCsv.Checked);
            _mapper.UpdateSetting(_helper.GetSetting("ExportLiteral"), chkExportLiteral.Checked);
            _mapper.UpdateSetting(_helper.GetSetting("ExportLiteralPattern"), tbExportLiteralPattern.Text);

            _mapper.UpdateSetting(_helper.GetSetting("JICSAllowExports"), chkJICSAllowExports.Checked);
            _mapper.UpdateSetting(_helper.GetSetting("JICSLinkText"), tbLinkText.Text);
            _mapper.UpdateSetting(_helper.GetSetting("JICSDescription"), tbDescription.Text);
            _mapper.UpdateSetting(_helper.GetSetting("JICSAsync"), chkJICSAsync.Checked);

            using (var htmlInputRadioButton = _jicsOutputTypes.SingleOrDefault(x => x.Checked))
            {
                if (htmlInputRadioButton != null)
                {
                    _mapper.UpdateSetting(_helper.GetSetting("JICSOutput"), htmlInputRadioButton.Value);
                    switch (htmlInputRadioButton.Value)
                    {
                        case "grid":

                            _mapper.UpdateSetting(_helper.GetSetting("JICSGridShowGridlines"), chkJICSGridShowGridlines.Checked);
                            _mapper.UpdateSetting(_helper.GetSetting("JICSGridCellPadding"), tbJICSGridCellPadding.Text.Trim() == "" ? "0" : tbJICSGridCellPadding.Text);
                            _mapper.UpdateSetting(_helper.GetSetting("JICSGridAltRowColors"), chkJICSGridAltRowColors.Checked);
                            _mapper.UpdateSetting(_helper.GetSetting("JICSGridShowColumnHeadings"), chkJICSShowColumnHeadings.Checked);
                            break;
                        case "datatables":
                            var expandedColumn = new List<string>();
                            if (tbJICSDataTablesExpandedColumns.Text.Trim().Length > 0)
                            {
                                if (tbJICSDataTablesExpandedColumns.Text.Contains(","))
                                {
                                    expandedColumn.AddRange(tbJICSDataTablesExpandedColumns.Text.Split(',').Select(column => column.Trim()));
                                }
                                else
                                {
                                    expandedColumn.Add(tbJICSDataTablesExpandedColumns.Text.Trim());
                                }
                            }
                            _mapper.UpdateSetting(_helper.GetSetting("JICSDataTablesExpandedColumns"), String.Join(",", expandedColumn.ToArray()));

                            tbJICSDataTablesExpandedColumns.Text = String.Join(",", expandedColumn.ToArray());

                            break;
                    }
                }
                else
                {
                    _mapper.UpdateSetting(_helper.GetSetting("JICSOutput"), "grid");
                }
            }

            _mapper.UpdateSetting(_helper.GetSetting("GOAllowExports"), chkGOAllowExports.Checked);
            using (var htmlInputRadioButton = _goOutputTypes.SingleOrDefault(x => x.Checked))
            {
                if (htmlInputRadioButton != null)
                {
                    _mapper.UpdateSetting(_helper.GetSetting("GOOutput"), htmlInputRadioButton.Value);
                    switch (htmlInputRadioButton.Value)
                    {
                        case "grid":

                            _mapper.UpdateSetting(_helper.GetSetting("GOGridShowGridlines"), chkGOGridShowGridlines.Checked);
                            _mapper.UpdateSetting(_helper.GetSetting("GOSGridCellPadding"), tbGOGridCellPadding.Text.Trim() == "" ? "0" : tbGOGridCellPadding.Text);
                            _mapper.UpdateSetting(_helper.GetSetting("GOGridAltRowColors"), chkGOGridAltRowColors.Checked);
                            _mapper.UpdateSetting(_helper.GetSetting("GOGridShowColumnHeadings"), chkGOGridShowColumnHeadings.Checked);
                            break;
                        case "masterdetail":
                            var expandedColumn = new List<string>();
                            if (tbGOOutputMasterDetailDisplayColumns.Text.Trim().Length > 0)
                            {
                                if (tbGOOutputMasterDetailDisplayColumns.Text.Contains(","))
                                {
                                    expandedColumn.AddRange(tbGOOutputMasterDetailDisplayColumns.Text.Split(',').Select(column => column.Trim()));
                                }
                                else
                                {
                                    expandedColumn.Add(tbGOOutputMasterDetailDisplayColumns.Text.Trim());
                                }
                            }
                            _mapper.UpdateSetting(_helper.GetSetting("GOMasterDetailDisplayColumns"), String.Join(",", expandedColumn.ToArray()));

                            tbGOOutputMasterDetailDisplayColumns.Text = String.Join(",", expandedColumn.ToArray());
                            break;
                    }

                }
                else
                {
                    _mapper.UpdateSetting(_helper.GetSetting("GOOutput"), "none");
                }
            }
            _settings = _mapper.GetSettings(_portletId).ToList();
        }

        private void SetDdlConfigFiles()
        {
            string[] configFiles = Directory.GetFiles(HttpContext.Current.Server.MapPath("ClientConfig/"), "*.config");

            foreach (var configFile in configFiles)
            {

                var reader = new XmlTextReader(configFile);
                var filename = configFile.Replace(HttpContext.Current.Server.MapPath("ClientConfig/"), "");
                while (reader.Read())
                {
                    if (reader.Name == "CUSConnection")
                        if (ddlConfigFile.Items.FindByValue(filename) == null)
                            ddlConfigFile.Items.Add(new ListItem(filename));
                    Trace.Warn(reader.Name);
                }
                reader.Close();
            }
            ddlConfigFile.Items.Add(new ListItem("Other", ""));
        }


    }
}