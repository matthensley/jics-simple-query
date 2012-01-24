using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Runtime.Serialization;

namespace CUS.ICS.SimpleQuery.Helpers
{
    
    public class JSDataTableConverter
    {
        private DataTable dt { get; set; }
        private String[] expandedColumns { get; set; }
        private String[] columnLabels { get; set; }
        public JSDataTable GetJsDataTable()
        {
            JSDataTable jsdatatable = new JSDataTable();


            List<JSDataTableColumnDescription> columns = new List<JSDataTableColumnDescription>();
            for (int col = 0; col < dt.Columns.Count; col++)
            {
                JSDataTableColumnDescription column = new JSDataTableColumnDescription();
                try
                {
                    if (columnLabels[col].Trim() != "" && columnLabels[col] != null)
                        column.sTitle = columnLabels[col];
                    else
                        column.sTitle = dt.Columns[col].ColumnName;
                }
                catch
                {
                    column.sTitle = dt.Columns[col].ColumnName;
                }

                if (dt.Columns[col].DataType == Type.GetType("System.DateTime"))
                {
                    column.sType = "date";
                }
                if (this.expandedColumns.Contains(column.sTitle))
                {
                    column.bDetail = true;
                    column.bVisible = false;
                }
                columns.Add(column);
            }
            if (this.expandedColumns.Count() > 0 && this.expandedColumns.First().Trim() != "")
            {
                JSDataTableColumnDescription column = new JSDataTableColumnDescription();
                column.sTitle = "";
                column.bSortable = false;
                column.sClass = "center";
                columns.Insert(0, column);
            }

            jsdatatable.columns = columns.ToArray();

            object[] data = new object[dt.Rows.Count];

            for (int row = 0; row < dt.Rows.Count; row++)
            {
                List<object> arr = new List<object>();
                for (int col = 0; col < dt.Columns.Count; col++)
                {
                    if (dt.Columns[col].DataType == Type.GetType("System.DateTime"))
                    {
                        if (dt.Rows[row][col] != DBNull.Value)
                            arr.Add(Convert.ToDateTime(dt.Rows[row][col]).ToShortDateString());
                        else
                            arr.Add("");
                    }
                    else
                    {
                        arr.Add(dt.Rows[row][col]);
                    }
                }
                if (this.expandedColumns.Count() > 0 && this.expandedColumns.First().Trim() != "")
                {
                    arr.Insert(0, "<img src='" + HttpContext.Current.Request.ApplicationPath + "/ClientConfig/images/datatables/details_open.png'>");
                }
                data[row] = arr.ToArray();
            }

            jsdatatable.data = data;

            return jsdatatable;
        }



        public JSDataTableConverter(DataTable _dt, String[] _expandedColumns, String[] _columnLabels)
        {
            this.dt = _dt;
            this.expandedColumns = _expandedColumns;
            this.columnLabels = _columnLabels;
        }


    }

    public class JSDataTable
    {
        public JSDataTableColumnDescription[] columns { get; set; }
        public object[] data { get; set; }
    }

    public class JSDataTableColumnDescription
    {
        public string sTitle { get; set; }
        public string sType { get; set; }
        public string sClass { get; set; }
        public bool bDetail { get; set; }
        public bool bVisible { get; set; }
        public bool bSortable { get; set; }

        public JSDataTableColumnDescription()
        {
            this.sType = "string";
            this.bDetail = false;
            this.bVisible = true;
            this.bSortable = true;
            this.sClass = "";
        }
    }
}