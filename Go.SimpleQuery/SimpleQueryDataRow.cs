using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Go.SimpleQuery
{
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