using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using CUS.ICS.SimpleQuery.Helpers;
using CUS.ICS.SimpleQuery.Mappers;
using Jenzabar.Portal.Framework;
using Jenzabar.Portal.Framework.Data;
using Jenzabar.Common.ApplicationBlocks.ExceptionManagement;

namespace CUS.ICS.SimpleQuery
{
	/// <summary>
	/// Summary description for ExportEvents.
	/// </summary>
	public class Export_Data : System.Web.UI.Page
	{
		private void Page_Load(object sender, System.EventArgs e)
		{
			var strContentType = "text/plain"; // these defaults will be overwritten if we're successful
			var strFilename = "ErrorOutput.txt";

		    var mstream = new MemoryStream();
			var sw = new StreamWriter(mstream);

			if (Request.QueryString["sqkey"] != null
				&& IsGuidFormat(Request.QueryString["sqkey"].ToString()))
			{
				var strKey = PortalUser.Current.ID.AsGuid + Request.QueryString["sqkey"];
                var strFormat = Request.QueryString["format"];
				if (HttpContext.Current.Session["sqhtml+" + strKey] != null && strFormat != null)
				{
                    try
                    {
                        DataTable dt = (DataTable) HttpContext.Current.Session["sqhtml+" + strKey];
                        var mapper = new NHSimpleQuerySettingsMapper();
                        var settings = mapper.GetSettings(new Guid(Request.QueryString["sqkey"])).ToList();
                        var _helper = new SettingsHelper(settings, new Guid(Request.QueryString["sqkey"]), mapper);
                        string fileName;
                        if (_helper.GetSetting("QueryTitle").Value.Trim().Length > 0)
                            fileName = Regex.Replace(_helper.GetSetting("QueryTitle").Value.Trim(), @"\W", "");//remove non-alphanumeric characters from filename
                        else
                            fileName = "ExportedData";

                        switch (strFormat)
                        {
                            case "xls":
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

                                sw.WriteLine(stringWrite.ToString().Replace("\n", "").Replace("\r", "").Replace("  ", ""));
                                strContentType = "application/vnd.ms-excel";
                                strFilename = fileName + ".xls";
                                break;
                            case "xml":
                                sw.WriteLine(OutputHelper.RenderXml(dt));
                                strContentType = "text/xml";
                                strFilename = fileName + ".xml";
                                break;
                            case "csv":
                                sw.WriteLine(OutputHelper.RenderCsv(dt,
                                                              _helper.GetSetting("JICSGridShowColumnHeadings", false).BoolValue,
                                                              _helper.GetSetting("ColumnLabels").Value));
                                strContentType = "text/csv";
                                strFilename = fileName + ".csv";
                                break;
                            case "literal":
                                sw.WriteLine(OutputHelper.RenderLiteral(dt, _helper.GetSetting("LiteralFormat", "{0}").Value));
                                strContentType = "text/plain";
                                strFilename = fileName + ".txt";
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        sw.WriteLine("Export failed. Please contact site adminstrator. (cache corrupted)" + (PortalUser.Current.IsSiteAdmin ? ex.ToString() : "") ); 
                    }
					
					
				}
				else
				{
					sw.WriteLine("Export failed. Please contact site adminstrator. (cache empty)");
				}
			}
			else
			{
				sw.WriteLine("Export failed. Please contact site adminstrator. (bad key)");
			}
			sw.Flush();
			sw.Close();
			
			byte[] byteArray = mstream.ToArray();

			mstream.Flush();
			mstream.Close();

			Response.Clear();
			Response.AddHeader("Content-Type", strContentType);
			Response.AddHeader("Content-Disposition", "attachment; filename=" + strFilename);
			Response.AddHeader("Content-Length", byteArray.Length.ToString());
			Response.ContentType = "application/octet-stream";
			Response.BinaryWrite(byteArray);
			Response.End();

		}

		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);
		}
		
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{    
			this.Load += new System.EventHandler(this.Page_Load);
		}
		#endregion
		
		private static bool IsGuidFormat(string strTest)
		{
			try
			{
				var guidTest = new ObjectIdentifier(new Guid(strTest));
				return true;
			}
			catch
			{
				return false;
			}
		}

	}
}
