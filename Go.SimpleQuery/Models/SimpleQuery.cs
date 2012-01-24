using System;
using System.Collections.Generic;
using System.Data;

namespace Go.SimpleQuery.Models
{
    public class SimpleQuery
    {
        public List<string> ExportTypes { get; set; }
        public bool AllowExports { get; set; }
        public string Action { get; set; }
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
    }
}