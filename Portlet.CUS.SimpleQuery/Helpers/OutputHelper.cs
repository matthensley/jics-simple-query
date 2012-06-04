using System;
using System.Data;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using System.Xml;
using CUS.ICS.SimpleQuery.Entities;

namespace CUS.ICS.SimpleQuery.Helpers
{
    public static class OutputHelper
    {
        public static String RenderXml(DataTable dt)
        {
            var encoding = Encoding.Unicode;  // other encoding are possible, e.g., utf-8
            string xml;

            // pretty print the xml
            var doc = new XmlDocument();
            var writer = new StringWriter();
            dt.TableName = "Row";
            //dt.DataSet.DataSetName = "SimpleQueryData";
            writer.Write("<?xml version=\"1.0\" encoding=\"utf-16\"?><xmlroot>");
            dt.WriteXml(writer);
            writer.Write("</xmlroot>");

            try
            {
                doc.LoadXml(writer.ToString());
                xml = FormatXml.Format(doc, encoding);
            }
            catch
            {
                xml = FormatXml.Format(doc, encoding);
            }
            return xml;
        }

        public static String RenderCsv(DataTable dt, bool showColumnHeadings, String columnLabels)
        {
            var csv = new StringBuilder();
            if (showColumnHeadings)
            {
                String[] strColumnLabel = columnLabels.Split(',');

                for (var i = 0; i < dt.Columns.Count; i++)
                {
                    try
                    {
                        csv.Append(strColumnLabel[i].Trim() != "" ? strColumnLabel[i] : dt.Columns[i].ColumnName);
                    }
                    catch
                    {
                        csv.Append(dt.Columns[i].ColumnName);
                    }

                    if (i + 1 < dt.Columns.Count) csv.Append(",");
                }
                csv.AppendLine("");
            }

            for (var i = 0; i < dt.Rows.Count; i++)
            {
                for (var j = 0; j < dt.Rows[i].ItemArray.Length; j++)
                {
                    csv.Append(dt.Rows[i].ItemArray[j].ToString().Trim());
                    if (j + 1 < dt.Rows[i].ItemArray.Length) csv.Append(",");
                }
                csv.AppendLine();
            }

            return csv.ToString();
        }

        public static String RenderLiteral(DataTable dt, String literalFormat)
        {
            var literalResults = new StringBuilder();
            for (var row = 0; row < dt.Rows.Count; row++)
            {
                for (var col = 0; col < dt.Columns.Count; col++)
                {
                    literalResults.Append(dt.Rows[row].ItemArray[col].ToString());
                }
            }
            return literalFormat.Replace("{0}", literalResults.ToString());
        }

        public static void ConfigureDataGrid(ref DataGrid dgResults, DataTable dt, bool showColumnHeadings, bool useAlternatingRowColor, bool showGridLines, Int16 cellPadding, string columnLabels)
        {
            dgResults.ShowHeader = showColumnHeadings;

            dgResults.AlternatingItemStyle.BackColor = useAlternatingRowColor ? dgResults.BorderColor : dgResults.BackColor;

            if (showGridLines)
            {
                dgResults.GridLines = GridLines.Both;
                dgResults.BorderStyle = BorderStyle.Solid;
            }
            else
            {
                dgResults.GridLines = GridLines.None;
                dgResults.BorderStyle = BorderStyle.None;
            }

            dgResults.CellPadding = cellPadding;

            if (showColumnHeadings)
            {
                String[] strColumnLabel = columnLabels.Split(',');

                for (var i = 0; i < dt.Columns.Count; i++)
                {
                    try
                    {
                        dt.Columns[i].ColumnName = strColumnLabel[i].Trim();
                    }
                    catch
                    {
                    }
                }
            }
        }

        public static DataGrid CreateDataGrid()
        {
            var dgResults = new DataGrid
            {
                PageSize = 30,
                BorderWidth = 1,
                BorderStyle = BorderStyle.Solid,
                BorderColor = System.Drawing.Color.FromArgb(224, 224, 224)
            };
            dgResults.AlternatingItemStyle.BackColor = System.Drawing.Color.FromArgb(224, 224, 224);
            dgResults.HeaderStyle.Font.Bold = true;
            return dgResults;
        }

        public static string RenderTemplate(DataTable dt, String templateHeader, String templateRow, String templateFooter)
        {
            var template = new StringBuilder();
            template.Append(templateHeader);
            
            for (var i = 0; i < dt.Rows.Count; i++)
            {
                var rowTemplate = new StringBuilder(templateRow);
                for (var j = 0; j < dt.Rows[i].ItemArray.Length; j++)
                {
                    rowTemplate.Replace(@"@%" + dt.Columns[j].ColumnName + "%", dt.Rows[i].ItemArray[j].ToString());
                }
                template.Append(rowTemplate);
            }
            template.Append(templateFooter);
            return template.ToString();
        }
    }
}