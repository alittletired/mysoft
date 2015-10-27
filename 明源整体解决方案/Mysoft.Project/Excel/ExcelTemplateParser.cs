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
using System.Data;
using System.Text.RegularExpressions;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Newtonsoft.Json.Linq;
namespace Mysoft.Project.Excel
{
    public interface IExcelMetaData
    {
        int RowIndex { get; set; }
        int ColIndex { get; set; }
        string BindName { get; set; }
        void SetValue(ISheet sheet, JToken token);
        void GetValue(ISheet sheet, JToken token);
    }

    public class ExcelCellMetaData : IExcelMetaData
    {
        public int RowIndex { get; set; }
        public int ColIndex { get; set; }
        public string BindName { get; set; }
        public void SetCellValue(ICell cell, JToken token) {
            if (token == null)
            {
                cell.SetCellValue((string)null);
                return;
            }
            try
            {
                switch (token.Type)
                {
                    case JTokenType.None:
                    case JTokenType.Null:
                    case JTokenType.Undefined:
                        cell.SetCellValue((string)null);
                        break;
                    case JTokenType.Integer:
                    case JTokenType.Float:
                        cell.SetCellValue((double)token);
                        break;
                    case JTokenType.Date:
                        cell.SetCellValue((DateTime)token);
                        break;
                    case JTokenType.String:
                    case JTokenType.Guid:
                    case JTokenType.Uri:
                        cell.SetCellValue((string)token);
                        break;
                    case JTokenType.Boolean:
                        cell.SetCellValue((bool)token);
                        break;
                }
            }
            catch (Exception ex) {
                var ms = ex.Message;
            }
        }


        public virtual void SetValue(ISheet sheet, JToken token)
        {
            var cell = sheet.GetRow(this.RowIndex).GetCell(this.ColIndex);          
            SetCellValue(cell,token[BindName] );
        }
        public virtual void GetValue(ISheet sheet, JToken token)
        {
            var cell = sheet.GetRow(this.RowIndex).GetCell(this.ColIndex);
            GetCellValue(cell, token);     
        }
        public void GetCellValue(ICell cell, JToken token)
        {
            if (cell == null) return;
            var str = cell.ToString();
            token[BindName] = str;  
        }
    }

    public class ExcelTableMetaData : IExcelMetaData
    {
     
        public string Direction { get; set; }
        public List< ExcelCellMetaData> Cells { get; set; }
        public ExcelTableMetaData() {
            Cells = new List<ExcelCellMetaData>();
        }
        public virtual void SetValue(ISheet sheet, JToken token)
        {
            JToken tokens;
            if (this.BindName == "$root")
            {
                tokens = token;
            }
            else
            {
                tokens = token[BindName];
            }
            var row = sheet.GetRow(RowIndex);
            var items=tokens.Children();
            foreach (var item in items)
            {
                foreach (var cellTemplate in Cells)
                {
                    var cell = row.GetCell(cellTemplate.ColIndex);
                    cellTemplate.SetCellValue(cell, item[cellTemplate.BindName]);
                }
                row = sheet.CopyRow(row.RowNum, row.RowNum + 1);
            }

        }
        public virtual void GetValue(ISheet sheet, JToken json)
        {
            JToken tokens;
            if (this.BindName == "$root")
            {
                tokens = json;
            }
            else
            {
                tokens = json[BindName];
            }
            if (tokens == null) {
                tokens = JToken.Parse("[]");
                json[BindName] = tokens;
            }
           
            JArray array=(JArray)tokens;
            for (int i = RowIndex; i < sheet.LastRowNum; i++)
			{
                var item =JToken.Parse("{}");
                var row = sheet.GetRow(i);
                foreach (var cellTemplate in Cells)
                {
                    var cell = row.GetCell(cellTemplate.ColIndex);
                    cellTemplate.GetCellValue(cell, item);
                }
                  array.Add(item);
			}      
           
        }
       
        #region IExcelMetaData 成员

        public int RowIndex
        {
            get;
            set;
        }

        public int ColIndex
        {
            get;
            set;
        }

        public string BindName
        {
            get;
            set;
        }

        #endregion
    }

    public class ExcelSheetMetaData
    {
        public string SheetName { get; set; }
        public List<ExcelCellMetaData> CellTemplates { get; set; }
        public List<ExcelTableMetaData> TableTemplates { get; set; }
        
        public int LastRowIndex { get; set; }
        public int SheetIndex { get; set; }
        public ExcelSheetMetaData()
        {
            CellTemplates = new List<ExcelCellMetaData>();
            TableTemplates = new List<ExcelTableMetaData>();
        }


    }

    public class ExcelMetaData
    {

        public ExcelMetaData() {
            Sheets = new List<ExcelSheetMetaData>();
        }
        public List<ExcelSheetMetaData> Sheets { get; set; }
      
        [JsonIgnore]
        public IWorkbook Workbook { get; set; }


        public bool IsArray { get; set; }
        public static ExcelMetaData FromTemplate(string filePath)
        {
            ExcelMetaDataParser parser = new ExcelMetaDataParser();
            return parser.Parse(filePath);
        }

        public static ExcelMetaData FromTemplate(IWorkbook workbook)
        {
            ExcelMetaDataParser parser = new ExcelMetaDataParser();
            return parser.Parse(workbook);
        }

        

        class ExcelMetaDataParser
        {
            static readonly Regex REG_Template = new Regex(@"\#\{([^\}]+)\}");

          

            public ExcelMetaDataParser()
            {

               
            
            }

            void ParseTable(ICell cell, string template, ExcelSheetMetaData sheetMeta)
            {
                var tokens = template.Split(',');
                ExcelTableMetaData tableTemplate = new ExcelTableMetaData()
                {
                    RowIndex = cell.RowIndex,
                    ColIndex = cell.ColumnIndex,

                };


                foreach (var token in tokens)
                {
                    var pair = token.Split(':');
                    if (pair[0] == "each")
                    {
                        tableTemplate.BindName = pair[1];
                    }
                    if (pair[0] == "direction")
                    {
                        tableTemplate.Direction = pair[1];
                    }

                }
                var row = cell.Row;
                for (int i = cell.ColumnIndex; i < row.LastCellNum; i++)
                {
                    ParseCell(row.Cells[i], tableTemplate.Cells);
                }
                sheetMeta.TableTemplates.Add(tableTemplate);
            }

            void ParseRow(IRow row, ExcelSheetMetaData sheetMeta)
            {
                foreach (ICell cell in row)
                {
                    var template = GetTemplate(cell);
                    if (string.IsNullOrEmpty(template))
                        continue;

                    if (template.IndexOf("each:") > -1)
                    {
                        ParseTable(cell, template, sheetMeta);
                        return;
                    }
                    else
                    {
                        ParseCell(cell, sheetMeta.CellTemplates);
                    }

                }

            }

            string GetTemplate(ICell cell)
            {
                if (cell == null)
                    return null;
                var expr = cell.ToString();
                if (string.IsNullOrEmpty(expr))
                    return null;
                var match = REG_Template.Match(expr);
                if (!match.Success)
                    return null;
                var template = match.Groups[1].Value;
                return template;
            }


            void ParseCell(ICell cell, List<ExcelCellMetaData> listCell)
            {

                ParseCell(cell, null, listCell);
            }


            void ParseCell(ICell cell, string template, List<ExcelCellMetaData> listCell)
            {
                if (cell == null)
                    return;
                template = template ?? GetTemplate(cell);
                if (string.IsNullOrEmpty(template))
                    return;

                var cellTemplate = new ExcelCellMetaData() { RowIndex = cell.RowIndex, ColIndex = cell.ColumnIndex };
                if (template.IndexOf(":") > 0)
                {
                    var tokens = template.Split(',');
                    foreach (var token in tokens)
                    {
                        var pair = token.Split(':');
                        if (pair[0] == "value")
                        {
                            cellTemplate.BindName = pair[1];
                        }
                    }
                }
                else
                {
                    cellTemplate.BindName = template;
                }
                listCell.Add(cellTemplate);

            }

            ExcelSheetMetaData ParseSheet(ISheet sheet)
            {

                ExcelSheetMetaData sheetMeta = new ExcelSheetMetaData();
                sheetMeta.SheetName = sheet.SheetName;
                sheetMeta.LastRowIndex = sheet.LastRowNum;
                foreach (IRow row in sheet)
                {
                    ParseRow(row, sheetMeta);
                }
                return sheetMeta;

            }

            public ExcelMetaData Parse(IWorkbook workbook) {
                var meta = new ExcelMetaData();               
                meta.Workbook = workbook;
                for (int i = 0; i < workbook.NumberOfSheets; i++)
                {
                    ISheet sheet = workbook.GetSheetAt(i);
                    ExcelSheetMetaData sheetMeta = ParseSheet(sheet);
                    sheetMeta.SheetIndex = i;
                    meta.Sheets.Add(sheetMeta);
                }
                return meta;
            }
            public ExcelMetaData Parse(string filePath)
            {
              
                if (filePath.StartsWith("/"))
                    filePath = HostingEnvironment.MapPath(filePath);
                if (!File.Exists(filePath))
                    throw new FileNotFoundException("模版文件 " + filePath + " 不存在！");
                IWorkbook workbook = WorkbookFactory.Create(filePath);
                return Parse(workbook);

            }




        }
    }

   

  


}