using System;
using System.Linq;
using System.Web;
using Jenzabar.Portal.Framework;
using Jenzabar.Portal.Framework.Web.UI;
using Jenzabar.Portal.Framework.Web.UI.Controls.MetaControls;
using Jenzabar.Portal.Framework.Web.UI.Controls.MetaControls.Attributes;
using Jenzabar.Portal.Framework.Security.Authorization;
using Jenzabar.Common.ApplicationBlocks.Data;
using Jenzabar.Portal.Framework.Configuration;
using Jenzabar.Common.ApplicationBlocks.ExceptionManagement;

namespace CUS.ICS.SimpleQuery
{
	#region Operations
	[PortletOperation("CanAdminQueries", 
		"CUS_SIMPLEQUERY_OPR_CANADMINQUERIES", 
		"Allows the user to administer the actual query used by each instance of this portlet.", 
		 PortletOperationScope.Global)]
	[PortletOperation("CanAdminAdvQueries", 
		 "CUS_SIMPLEQUERY_OPR_CANADMINADVQUERIES", 
		 "Allows the user to use dangerously powerful SQL statements for use in this query.", 
		 PortletOperationScope.Global)]
	#endregion

	#region Settings
	#endregion

    [PortletInstaller(typeof(SimpleQueryPortletInstaller))]

	public class SimpleQuery : SecuredPortletBase
	{
		public SimpleQuery()
		{
            this.EnableViewState = false;
		}
		protected override PortletViewBase GetCurrentScreen()
		{
			PortletViewBase screen = null;

			switch(this.CurrentPortletScreenName)
			{
				case "Admin" :
					this.State = PortletState.Maximized;
					screen = this.LoadPortletView("ICS/SimpleQuery/Admin_View.ascx");
					break;

				case "Results" :
					this.State = PortletState.Maximized;
					screen = this.LoadPortletView("ICS/SimpleQuery/Default_View.ascx");
					break;

				case "Default" :
				default :
					screen = this.LoadPortletView("ICS/SimpleQuery/Default_View.ascx");
					break;
			}
			return screen;
		}

        protected override bool PopulateToolbar(Jenzabar.Common.Web.UI.Controls.Toolbar toolbar)
        {
            toolbar.MenuItems.Add("Admin This Query", "Admin");

            return (this.AccessCheck("CanAdminQueries") || this.AccessCheck("CanAdminAdvQueries"));
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            this.Toolbar.ItemCommand += new System.Web.UI.WebControls.CommandEventHandler(Toolbar_ItemCommand);
        }

        void Toolbar_ItemCommand(object sender, System.Web.UI.WebControls.CommandEventArgs e)
        {
            this.NextScreen(e.CommandName);
        }
	}

    class SimpleQueryPortletInstaller : PortletInstaller
    {
        public override void PortletRemovedFromPage(Portlet portlet, PortalPageInfo page)
        {
            try
            {
                var mapper = new Mappers.NHSimpleQuerySettingsMapper();
                mapper.Delete(mapper.GetSettings(portlet.ID).ToList());
            }
            catch (Exception ex)
            {
                ExceptionManager.Publish(ex);
            }
        }
     
    }
}
