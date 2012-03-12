<%@ Page Language="c#" Debug="true" validateRequest="false" TargetSchema="http://schemas.microsoft.com/intellisense/ie5" %>
<%@ Import Namespace="System.Web.UI.WebControls" %>
<%@ Import Namespace="Jenzabar.Common" %>
<%@ Import Namespace="System" %>
<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="System.Data.SqlClient" %>
<%@ Import Namespace="System.Web.UI" %>
<%@ Import Namespace="Jenzabar.Common.ApplicationBlocks.Data" %>
<%@ Import Namespace="Jenzabar.Portal.Framework.Data" %>
<%@ Import Namespace="Jenzabar.Portal.Framework" %>
<%@ Import Namespace="Jenzabar.ICS.Web.Portlets.Common" %>
<%
  /*
  Simple Query Portlet Data Converter Tool
  by Carey Morgan
  */
  string toolName = "Simple Query Portlet Data Converter";
  string toolDesc = "Copies Simple Query portlet data from versions 3.0 and earlier into new 3.1 data structures.";
%>

<script runat="server">

	bool blRequireAdminUser = false; // change to false if you like

    protected override void OnInit(EventArgs e)
    {
        this.Load += new EventHandler(Page_Load);
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (blRequireAdminUser && !Jenzabar.Portal.Framework.PortalUser.Current.IsSiteAdmin)
        {
            ShowMsg("You must be a <strong>Portal Administrator</strong> to use this tool.");
            divForm.Visible = false;
        }
		else
			divForm.Visible = true;
    }
        
    protected void lnkCopyOldToNew_Click(object sender, EventArgs e)
    {
		litProgress.Text = String.Empty;
		Jenzabar.Portal.Framework.Portlet[] SQPortlets = Jenzabar.Portal.Framework.Portlet.FindByPortletTemplate(PortletTemplate.FindByName("[CUS] SimpleQuery"));
		ProgressNote("<p>Run time:" + DateTime.Now.ToString() + "</p>");
        foreach (Jenzabar.Portal.Framework.Portlet sqp in SQPortlets)
        {
            ProgressNote( String.Format("<h4>Portlet: {0}</h4><p>On Page: {1}</p><p>In Context: {2}</p>", sqp.Portlet.DisplayName, sqp.Portlet.ParentPage.DisplayName, sqp.Portlet.Context.DisplayName, sqp.Portlet.Context.DN.Replace("CN=", " in ").Replace(",","").Replace("O=Jenzabar,C=US","")));
            ProgressNote("<p>Copying CUS_SimpleQuery to CUS_SimpleQuerySetting...");
            bool isGrid = ConvertSQTableToSQSettings(sqp).Equals("xls");
            ProgressNote("Done</p>");
            ProgressNote("<p>Copying FWK_PortletSetting to CUS_SimpleQuerySetting...");
            ConvertPortletSettingstoSQSettings(sqp, isGrid);
            ProgressNote("Done</p>");
        };
		ShowMsg("Old Simple Query Data Copied.");
    }
	private void ConvertPortletSettingstoSQSettings(Jenzabar.Portal.Framework.Portlet portlet, bool isGrid)
	{
		ObjectIdentifier portletID = portlet.Portlet.ID;
		StoreSimpleQuerySetting(portletID, "QueryTitle", PortletUtilities.GetSettingValue(portlet, "QueryTitle"));
		StoreSimpleQuerySetting(portletID, "JICSDisplayResultsMimimized", PortletUtilities.GetSettingValue(portlet, "DisplayResultsMinimized").Equals("Checked").ToString());
		StoreSimpleQuerySetting(portletID, "JICSLinkText", PortletUtilities.GetSettingValue(portlet, "ViewResultsLinkText"));
		StoreSimpleQuerySetting(portletID, "JICSDescription", PortletUtilities.GetSettingValue(portlet, "QueryDescription"));
		StoreSimpleQuerySetting(portletID, "JICSAllowExports", PortletUtilities.GetSettingValue(portlet, "DisplayExportToExcel").Equals("Checked").ToString()); 
		
		if (isGrid)
		{
			StoreSimpleQuerySetting(portletID, "JICSGridShowColumnHeadings", PortletUtilities.GetSettingValue(portlet, "DisplayColumnHeadings").Equals("Checked").ToString());
			StoreSimpleQuerySetting(portletID, "JICSGridAltRowColors", PortletUtilities.GetSettingValue(portlet, "UseAlternatingRowColor").Equals("Checked").ToString());
			StoreSimpleQuerySetting(portletID, "JICSGridShowGridlines", PortletUtilities.GetSettingValue(portlet, "ShowGridBorders").Equals("Checked").ToString());
			StoreSimpleQuerySetting(portletID, "JICSGridCellPadding", PortletUtilities.GetSettingValue(portlet, "CellPadding"));
		}
	}
	private string ConvertSQTableToSQSettings(Jenzabar.Portal.Framework.Portlet portlet)
	{
		ObjectIdentifier portletID = portlet.Portlet.ID;
		SqlDataReader drdr = SqlHelper.ExecuteReader(
			Jenzabar.Common.Configuration.ConfigSettings.Current.DatabaseConnectionInfo.ConnectionString, CommandType.Text,
			String.Format("SELECT * FROM CUS_SimpleQuery WHERE PortletID='{0}'", portletID.AsGuid.ToString()));
		DataTable dt = new DataTable();
		dt.Load(drdr);
		string retVal = String.Empty;
		DataRow dr = null;
		if (dt != null && dt.Rows.Count > 0)
		{
			dr = dt.Rows[0];
			StoreSimpleQuerySetting(portletID, "ConfigFile", dr["ConfigFile"].ToString());
			StoreSimpleQuerySetting(portletID, "QueryText", dr["QueryText"].ToString());
			StoreSimpleQuerySetting(portletID, "ColumnLabels", dr["ColumnLabels"].ToString());
			StoreSimpleQuerySetting(portletID, "JICSOutput", (dr["UseDataTable"] != null && dr["UseDataTable"].ToString().Equals("1") ? "datatables" : "grid"));

			StoreSimpleQuerySetting(portletID, "ExportXls", dr["ResultFormat"].ToString().Equals("xls").ToString());
			StoreSimpleQuerySetting(portletID, "ExportCsv", dr["ResultFormat"].ToString().Equals("csv").ToString());
			StoreSimpleQuerySetting(portletID, "ExportXml", dr["ResultFormat"].ToString().Equals("xml").ToString());

			if (dt.Columns.Contains("ASync"))
				StoreSimpleQuerySetting(portletID, "JICSAsync", (dr["ASync"].ToString().Equals("1").ToString()));
			if (dt.Columns.Contains("ExpandedColumnLabels"))
				StoreSimpleQuerySetting(portletID, "JICSDataTablesExpandedColumns", dr["ExpandedColumnLabels"].ToString());
		}
		drdr.Close();
		if (dr != null)
			return dr["ResultFormat"].ToString();
		else
			return String.Empty;
	}
    private void StoreSimpleQuerySetting(ObjectIdentifier portletID, string settingName, string settingValue) 
    { 
        SqlParameter[] parms = new SqlParameter [] 
                        { 
                            new SqlParameter("portletId", portletID.AsGuid.ToString()), 
							new SqlParameter("settingName", settingName), 
                            new SqlParameter("settingValue", settingValue) 
                        }; 
             
        SqlHelper.ExecuteNonQuery( 
             Jenzabar.Common.Configuration.ConfigSettings.Current.DatabaseConnectionInfo.ConnectionString, CommandType.Text, 
             "IF NOT EXISTS (SELECT * FROM CUS_SimpleQuerySetting WHERE PortletID = @portletId AND [Name]=@settingName)" + 
             " INSERT INTO CUS_SimpleQuerySetting (PortletID, [Name], [Value]) VALUES (@portletId,@settingName,@settingValue)" 
             ,parms); 
    }

	protected void lnkClearNewData_Click(object sender, EventArgs e)
	{
	    litProgress.Text = String.Empty;
		SqlHelper.ExecuteNonQuery(
			Jenzabar.Common.Configuration.ConfigSettings.Current.DatabaseConnectionInfo.ConnectionString, CommandType.Text,
			String.Format("DELETE FROM CUS_SimpleQuerySetting") 
			);
		ShowMsg("New Simple Query Data Cleared and ready for Re-Copy.");
	}
	protected void lnkClearOldData_Click(object sender, EventArgs e)
	{
		litProgress.Text = String.Empty;
		Jenzabar.Portal.Framework.Portlet[] SQPortlets = Jenzabar.Portal.Framework.Portlet.FindByPortletTemplate(PortletTemplate.FindByName("[CUS] SimpleQuery"));
		ProgressNote("<p>Run time:" + DateTime.Now.ToString() + "</p>");
        foreach (Jenzabar.Portal.Framework.Portlet sqp in SQPortlets)
        {
            ProgressNote( String.Format("<h4>Portlet: {0}</h4><p>On Page: {1}</p><p>In Context: {2}</p>", sqp.Portlet.DisplayName, sqp.Portlet.ParentPage.DisplayName, sqp.Portlet.Context.DisplayName, sqp.Portlet.Context.DN.Replace("CN=", " in ").Replace(",","").Replace("O=Jenzabar,C=US","")));
            ProgressNote("<p>Deleting FWK_PortletSetting and CUS_SimpleQuery...");
            ClearOldData(sqp.Portlet.ID);
            ProgressNote("Done</p>");
        };
		ShowMsg("Old Simple Query Data Cleared.");

	}
	private void ClearOldData(ObjectIdentifier portletID)
	{
		string strPortletID = portletID.AsGuid.ToString();
		SqlHelper.ExecuteNonQuery(
			Jenzabar.Common.Configuration.ConfigSettings.Current.DatabaseConnectionInfo.ConnectionString, CommandType.Text,
			String.Format("DELETE FROM FWK_PortletSetting WHERE PortletID='{0}'", strPortletID) 
			);
		SqlHelper.ExecuteNonQuery(
			Jenzabar.Common.Configuration.ConfigSettings.Current.DatabaseConnectionInfo.ConnectionString, CommandType.Text,
			String.Format("DELETE FROM CUS_SimpleQuery WHERE PortletID='{0}'", strPortletID) 
			);
	}
    protected void ShowMsg(string text)
    {
        lblMsg.Text = text + " (" + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + ")";
        divMsg.Visible = true;
    }
	protected void ProgressNote(string text)
	{
	    litProgress.Text += text;
	}
	
</script>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.01 Transitional//EN" "http://www.w3.org/TR/html4/loose.dtd">

<html>
<head>
    <meta http-equiv="Content-Type" content="text/html; charset=UTF-8">
    <title><%=toolName%></title>
    <link rel="shortcut icon" href="../favicon.ico" type="image/x-icon" />
	<link href="/ics/clientconfig/HtmlContent/custom.css" type="text/css" rel="stylesheet">	
	<style media="all" type="text/css">
	.link {background-color: #ddd; border: solid 1px black; padding: 6px; margin: 5px; display: inline-block; color: black!important; font-weight: bold; text-decoration:none;}
	</style>

</head>
<body>
    <div id="header">
                <h2>
                    <a href="<%=Request.Path%>">
                        <%=toolName%></a></h2>
                <p><%=toolDesc %></p>
            </div>
    <div id="main">
        <div id="divMsg" runat="server" visible="false" style="border:1px solid black; color:Red; margin:5px 0px; padding: 5px;"><asp:Label ID="lblMsg" runat="server" EnableViewState="False"></asp:Label></div>
        <div id="divForm" runat="server">
			<p>Note: This tool must be run BEFORE any portal pages containing existing Simple Query portlets are loaded.
            <form id="frmTool" method="post" runat="server" enableviewstate="False">
                 <asp:linkbutton runat="server" id="lnkCopyOldToNew" 
                    onclick="lnkCopyOldToNew_Click" CssClass="link">Copy Old Data to New Structure</asp:linkbutton>
                <br />
                <asp:LinkButton runat="server" id="lnkClearNewData" 
                    onclick="lnkClearNewData_Click" CssClass="link">Clear New Data before a Repeat Copy</asp:LinkButton>
            </form>
			<asp:Literal ID="litProgress" runat="server" EnableViewState="False"></asp:Literal>
        </div>
    </div>
</body>
</html>

