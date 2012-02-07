using System;
using System.Collections.Generic;
using System.Data;

namespace Go.SimpleQuery.Models
{
    public class SimpleQuery
    {
        public bool AllowExports { get; set; }
        
    }

    public class SimpleQueryExport : SimpleQuery
    {
        public List<string> ExportTypes { get; set; }
    }

    public class SimpleQueryMasterDetail : SimpleQuery
    {
        public List<SimpleQueryDataRow> MasterDetailData { get; set; }
    }

    public class SimpleQueryDataGrid : SimpleQuery
    {
        public DataTable Data { get; set; }
        public bool ShowColumnHeaders { get; set; }
        public bool ShowAltColors { get; set; }
        public bool ShowGridlines { get; set; }
        public Int16 CellPadding { get; set; }
    }

    public class SimpleQueryLiteral: SimpleQuery
    {
        public string Html { get; set; }
        public string Action { get; set; }
    }

    public class SimpleQueryDataRow
    {
        public string Master { get; set; }
        public List<SimpleQueryDataColumn> Details { get; set; }

        public SimpleQueryDataRow()
        {
            Details = new List<SimpleQueryDataColumn>();
        }
    }

    public class SimpleQueryDataColumn
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}