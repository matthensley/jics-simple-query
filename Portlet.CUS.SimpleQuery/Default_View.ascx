<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Default_View.ascx.cs"
    Inherits="CUS.ICS.SimpleQuery.Default_View" %>
<link href="<%= this.ResolveUrl("~/ClientConfig/css/jqueryDataTable.css") %>" rel="stylesheet"
    type="text/css" />
<script type="text/javascript">


    jQuery(function ($) {
        // Setup 
        var portletLocation = '<%= this.ResolveUrl("~/Portlets/CUS/ICS/SimpleQuery/") %>';

        var portletId = '<%= this.ParentPortlet.PortletDisplay.Portlet.ID.AsGuid %>';

        var controls = {
            datatable: $('#<%= tblDataTable.ClientID %>'),
            htmlDiv: $('#<%= divSQResults.ClientID %>'),
            error: $('#<%= error.ClientID %>'),
            message: $('#<%= message.ClientID %>'),
            loading: $('#<%= lnbGetData.ClientID %>')
        };
        
        function fnFormatDetails(nTr, aoColumns, oTable) {

            var aData = oTable.fnGetData(nTr);
            var sOut = '<table cellpadding="5" cellspacing="0" border="0" style="padding-left:50px;">';
            for (var column in aoColumns) {
                if (aoColumns[column].bDetail) {
                    sOut += '<tr><td><strong>' + aoColumns[column].sTitle + ':</strong> ' + aData[column] + '</td></td>';
                }
            }
            sOut += '</table>';

            return sOut;
        }

        function getData() {
            $.ajax({
                url: portletLocation + "Query.ashx",
                data: { _portletID: portletId, action: "RunQueryHTML" },
                dataType: "json",
                type: "POST",
                success: function (ret) {
                    if (ret.d.success) {
                        if (ret.d.html == "") {
                            $(controls.message).show().html('No results returned by this query.' + (ret.d.query != null ? ('<br />Query text (visible by Query Admin only):<br />' + ret.d.query) : ''));
                        } else {
                            controls.htmlDiv.html(ret.d.html);
                        }
                        controls.loading.hide();
                    } else {
                        $(controls.error).show().html(ret.d.message + "<br />" + ret.d.exception);
                    }
                },
                failure: function (ret) {
                    $(controls.error).html(ret.Message + "<br />" + ret.StackTrace).show();
                }
            });
        }

        function loadTable() {
            $.ajax({
                url: portletLocation + "Query.ashx",
                data: { _portletID: portletId, action: "RunQuery" },
                dataType: "json",
                type: "POST",
                success: function (ret) {
                    if (ret.d.success) {
                        //data = ret.d.data;
                        //columns = ret.d.columns;
                        var oTable = controls.datatable.dataTable({ "sPaginationType": "full_numbers", "aaData": ret.d.data, "aoColumns": ret.d.columns, "aaSorting": [] });

                        controls.datatable.find('img').live('click', function () {
                            var nTr = this.parentNode.parentNode;
                            if (this.src.match('details_close')) {
                                /* This row is already open - close it */
                                this.src = '<%= this.ResolveUrl("~/ClientConfig/images/datatables/details_open.png") %>';
                                oTable.fnClose(nTr);
                            }
                            else {
                                /* Open this row */
                                this.src = '<%= this.ResolveUrl("~/ClientConfig/images/datatables/details_close.png") %>'
                                oTable.fnOpen(nTr, fnFormatDetails(nTr, ret.d.columns, oTable), 'details');
                            }
                        });
                    } else {
                        $(controls.error).show().html(ret.d.message + "<br />" + ret.d.exception);
                    }
                },
                failure: function (ret) {
                    $(controls.error).show().html(ret.Message + "<br />" + ret.StackTrace);
                }
            });
        }

        if ($('#<%= tblDataTable.ClientID %>').length != 0)
            if ($.fn.dataTableExt == undefined) {
                $.getScript('<%= this.ResolveUrl("~/ClientConfig/js/jquery.dataTables.min.js") %>', function () {
                    loadTable(controls, portletLocation, portletId);
                });
            }
            else {
                loadTable(controls, portletLocation, portletId);
            }
        if ($('#<%= lnbGetData.ClientID %>').length != 0) {
            getData(controls, portletLocation, portletId);
        }
    });

</script>
<style type="text/css">
    .hidden
    {
        display: none;
    }
    .viewResults
    {
        cursor: pointer;
        font-weight: bold;
        text-decoration: underline;
    }
    #tblDataTable
    {
        margin-top: 5px;
    }
    #<%= pnlExport.ClientID %>
    {
        border: 2px solid #D2D2D2;
        border-radius: 4px 4px 4px 4px;
        float: right;
        height: 28px;
        margin-bottom: 5px;
        padding-left: 5px;   
    }
    #<%= pnlExport.ClientID %> a
    {
        float: left;
        background-image: none !important;
    }
</style>
<div class="pContent">
    <asp:HiddenField ID="hdnUseAJAX" runat="server" />
    <div id="message" class="feedbackMessage hidden" runat="server">
    </div>
    <div id="error" class="feedbackError hidden" runat="server">
    </div>
    <asp:Panel ID="pnlQueryTitle" runat="server">
        <h4>
            <asp:Label ID="lblQueryTitle" runat="server"></asp:Label>
        </h4>
    </asp:Panel>
    <asp:Panel runat="server" ID="pnlExport" Visible="False">
        <span style="float: left; margin-top: 3px; padding-right: 10px;">Export To:</span>
        <asp:HyperLink ID="lnkSQExportExcel" runat="server" Target="_blank" Visible="False">Excel</asp:HyperLink>
        <asp:HyperLink ID="lnkSQExportCsv" runat="server" Target="_blank" Visible="False">CSV</asp:HyperLink>
        <asp:HyperLink ID="lnkSQExportXml" runat="server" Target="_blank" Visible="False">XML</asp:HyperLink>
        <asp:HyperLink ID="lnkSQExportLiteral" runat="server" Target="_blank" Visible="False">Literal</asp:HyperLink>
    </asp:Panel>
    <asp:Panel ID="pnlLinkDescription" runat="server">
        <div class="pSection">
            <asp:LinkButton ID="lnkViewResults" runat="server"></asp:LinkButton>
            <asp:Label ID="lblQueryDescription" runat="server"></asp:Label></div>
    </asp:Panel>
    <div id="message2" class="feedbackMessage" runat="server" visible="false">
    </div>
    <asp:LinkButton ID="lnbGetData" runat="server" Text="Loading..." Visible="false"></asp:LinkButton>
    <div id="divSQResults" runat="server" style="float: left;">
    </div>
    <asp:Panel ID="pnlResults" runat="server" Style="float: left;">
        <div class="pSection">
            <asp:DataGrid ID="dgResults" runat="server" Width="" PageSize="30" BorderWidth="1px"
                BorderStyle="Solid" BorderColor="#E0E0E0">
                <AlternatingItemStyle BackColor="#E0E0E0"></AlternatingItemStyle>
                <HeaderStyle Font-Bold="True"></HeaderStyle>
            </asp:DataGrid>
            <asp:Literal ID="preformattedResults" runat="server"></asp:Literal>
        </div>
    </asp:Panel>
    <asp:Panel ID="pnlDataTableResults" runat="server" Visible="false">
        <table id="tblDataTable" runat="server" cellpadding="0" cellspacing="0" border="0"
            class="display">
        </table>
    </asp:Panel>
</div>
<div style="clear: both">
</div>
