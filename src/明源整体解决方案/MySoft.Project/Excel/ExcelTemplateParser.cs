using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;
using System.IO;
using Newtonsoft.Json;
using System.Web.Hosting;
using Toxy;
using System.Data;
using System.Text.RegularExpressions;
namespace MySoft.Project.Excel
{
    public interface IExcelTemplate
    {
        int RowIndex { get; set; }
        int ColIndex { get; set; }
        string BindName { get; set; }
       
    }
    public class ExcelCellTemplate : IExcelTemplate
    {
        public int RowIndex { get; set; }
        public int ColIndex { get; set; }
        public string BindName { get; set; }       
    }
    public class ExcelTableTemplate : ExcelCellTemplate
    {
     
        public string ValueName { get; set; }
        public int EndRowIndex { get; set; }
        public int EndColIndex { get; set; }
        public List< ExcelCellTemplate> Cells { get; set; }
        public ExcelTableTemplate() {
            Cells = new List<ExcelCellTemplate>();
        }

      
    }
    public class TemplateMetaData
    {
        public List<ExcelCellTemplate> Cells { get; set; }
        public List<ExcelTableTemplate> Tables { get; set; }
    }
    public class ExcelTemplateParser
    {
        static readonly Regex REG_EXPR = new Regex(@"\#\{(\w+)\}");
        static readonly Regex REG_EXPR_BEGINEACH = new Regex(@"each:(\w+)\:?");
        static readonly Regex REG_EXPR_Value = new Regex(@"\#\{.*value:(\w+)\:?.*\}");
        static readonly Regex REG_EXPR_ENDEACH = new Regex(@"end:(\w+)\:?");

        public string FilePath { get; set; }
         int _currRowIndex { get; set; }
         int _currColIndex { get; set; }
         ToxyTable _currTable { get; set; }
        public ExcelTemplateParser()
        {
          
        }

        ExcelTableTemplate ParseTable(ToxyCell cell)
        {
            if (cell == null || string.IsNullOrEmpty(cell.Value))
            {
                return null;
            }
            var expr = cell.Value;
            if (string.IsNullOrEmpty(expr))
                return null;
            var eachpart = REG_EXPR_BEGINEACH.Match(expr).Groups;
            if (eachpart.Count == 0) return null;

            ExcelTableTemplate token = new ExcelTableTemplate() { RowIndex = _currRowIndex, ColIndex = _currColIndex, BindName = eachpart[0].Value };


            if (eachpart.Count == 1)
            {
                token.ValueName = eachpart[0].Value;
                var valGroup = REG_EXPR_Value.Match(expr).Groups;
                if (valGroup.Count == 0)
                {
                    token.Cells.Add(new ExcelCellTemplate() { RowIndex = _currRowIndex, ColIndex = _currColIndex });
                    _currColIndex++;
                }
                else
                {
                    var row = _currTable.Rows[_currRowIndex];
                    for (; _currColIndex < row.Cells.Count; _currColIndex++)
                    {
                        var celltoken = ParseCell(row.Cells[_currColIndex]);
                        if (celltoken != null)
                            token.Cells.Add(celltoken);
                    }
                }
            }
            return token;
        }

        ExcelCellTemplate ParseCell(ToxyCell cell)
        {
            if (cell == null || string.IsNullOrEmpty(cell.Value)) {
                return null;
            }
            var expr = cell.Value;
            if (string.IsNullOrEmpty(expr))
                return null;

            var bindName = string.Empty;
            var valGroup = REG_EXPR_Value.Match(expr).Groups;
            if (valGroup.Count == 1)
            {
                return new ExcelCellTemplate() { RowIndex = _currRowIndex, ColIndex = _currColIndex, BindName = valGroup[0].Value };
            }
            valGroup = REG_EXPR.Match(expr).Groups;
            if (valGroup.Count == 1)
            {
                return new ExcelCellTemplate() { RowIndex = _currRowIndex, ColIndex = _currColIndex, BindName = valGroup[0].Value };
            }
            return null;

        }

        public ExcelTableTemplate Parse(string filepath)
        {
            var templateMetadata = new TemplateMetaData();
            if (filepath.StartsWith("/"))
                filepath = HostingEnvironment.MapPath(filepath);
            FilePath = filepath;
            ParserContext context = new ParserContext(filepath);
            ISpreadsheetParser parser = ParserFactory.CreateSpreadsheet(context);
            ToxySpreadsheet ss = parser.Parse();
            foreach (var table in ss.Tables)
            {
                _currTable = table;
                for (_currRowIndex = 0; _currRowIndex < table.Rows.Count; _currRowIndex++)
                {
                    var row = table.Rows[_currRowIndex];
                    for (_currColIndex = 0; _currColIndex < row.Cells.Count; _currColIndex++)
                    {
                        var cell = row.Cells[_currColIndex];                      
                      
                        var tabletoken = ParseTable(cell);
                        if (tabletoken != null)
                        {
                            templateMetadata.Tables.Add(tabletoken);
                        }
                        else
                        {
                            var cellToken = ParseCell(cell);
                            if (cellToken != null)
                            {
                                templateMetadata.Cells.Add(cellToken);
                            }
                        }

                    }

                }

            }
            return null;
        }
    }


}