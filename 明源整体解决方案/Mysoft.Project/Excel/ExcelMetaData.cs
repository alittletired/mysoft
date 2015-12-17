using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;
using System.IO;

using System.Web.Hosting;
using System.Data;
using System.Text.RegularExpressions;
using Mysoft.Project.NPOI.HSSF.UserModel;
using Mysoft.Project.NPOI.SS.UserModel;
using Mysoft.Project.NPOI.XSSF.UserModel;
using Mysoft.Project.Json;
using Mysoft.Project.NPOI.HSSF.Util;
using Mysoft.Project.Json.Linq;
namespace Mysoft.Project.Excel
{
    public interface IExcelCell
    {
        int RowIndex { get; set; }
        int ColIndex { get; set; }
       
        void SetValue(ISheet sheet, JToken token);
        void GetValue(ISheet sheet, JToken token);
    }

    public class ExcelCell : IExcelCell
    {
        public int RowIndex { get; set; }
        public int ColIndex { get; set; }
        public string Bind { get; set; }
        public string Format { get; set; }
        public bool IsLock { get; set; }
        public bool IsHide { get; set; }
        public int Width { get; set; }
        public virtual void SetCellValue(ICell cell, JToken token) {
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
            SetCellValue(cell,token[Bind] );
        }
        public virtual void GetValue(ISheet sheet, JToken token)
        {
            var cell = sheet.GetRow(this.RowIndex).GetCell(this.ColIndex);
            GetCellValue(cell, token);     
        }
        public void GetCellValue(ICell cell, JToken token)
        {
            if (cell == null) return;
            var str = cell.ToString().Trim();
            token[Bind] = str;  
        }
    }
    
    public class ExcelTable : ExcelCell
    {
     
        public string Direction { get; set; }
        public string Each { get; set; }
        public List< ExcelCell> Cells { get; set; }
        public ExcelTable() {
            Cells = new List<ExcelCell>();
        }
        public string TreeCode { get; set; }
        public override void SetValue(ISheet sheet, JToken token)
        {
            JArray data;
            if (this.Each == "$root")
            {
                data = (JArray)token;
            }
            else
            {
                data = (JArray)token[Each];
            }
            var row = sheet.GetRow(RowIndex);

            var grouprows = ExcelGroupRow.GetGroupRows(data, TreeCode);

            for (int i = 0; i < data.Count; i++)
            {
                var item = data[i];
                foreach (var cellTemplate in Cells)
                {
                    var cell = row.GetCell(cellTemplate.ColIndex);
                    var val = item[cellTemplate.Bind];
                  
                        cellTemplate.SetCellValue(cell, val);
                }
                if (i != data.Count - 1) {
                    row = sheet.CopyRow(row.RowNum, row.RowNum + 1);
                }
            }
           

            var groups = grouprows.Values.OrderByDescending(o => o.TreeCode).ToList();
            var b = JsonConvert.SerializeObject(groups);
            foreach (var grouprow in groups)
            {
               
                sheet.GroupRow(grouprow.StartIndex + RowIndex+1, grouprow.EndIndex + RowIndex);
            }
            if (data.Count == 0) {
                sheet.ShiftRows(row.RowNum+1, sheet.LastRowNum, -1);
            }


        }
        public override void GetValue(ISheet sheet, JToken json)
        {
            JToken tokens;
            if (this.Each == "$root")
            {
                tokens = json;
            }
            else
            {
                tokens = json[Each];
            }
            if (tokens == null) {
                tokens = JToken.Parse("[]");
                json[Each] = tokens;
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
    }    
    

    public class ExcelSheetMetaData
    {
        public string SheetName { get; set; }
        public List<ExcelCell> CellTemplates { get; set; }
        public List<ExcelTable> TableTemplates { get; set; }        
        public int SheetIndex { get; set; }
        public ExcelSheetMetaData()
        {
            CellTemplates = new List<ExcelCell>();
            TableTemplates = new List<ExcelTable>();
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
        public static ExcelMetaData FromExcel(IWorkbook workbook)
        {
            ExcelMetaDataParser parser = new ExcelMetaDataParser();
            return parser.Parse(workbook);
        }


        public void Render(object data) {         
            var json = JToken.FromObject(data);
            IsArray = json.Type == JTokenType.Array;
          
            for (int i = 0; i < Workbook.NumberOfSheets; i++)
            {
                ISheet sheet = Workbook.GetSheetAt(i);
                var sheetMeta = Sheets[i];
                Workbook.SetSheetName(i, sheetMeta.SheetName);
                RenderSheet(sheetMeta, sheet, json);

            }

            //将模版元数据放到excel文件中，便于下次读取
            var templateSheet = Workbook.CreateSheet("模版元数据");
            templateSheet.ProtectSheet("author:zhulin");
            templateSheet.TabColorIndex = HSSFColor.Yellow.Index;
            var row = templateSheet.CreateRow(0);
            var metastr = JsonConvert.SerializeObject(this);
            row.CreateCell(0).SetCellValue(metastr);
            var sheetIdx = Workbook.GetSheetIndex(templateSheet);
            Workbook.SetSheetHidden(sheetIdx, SheetState.VeryHidden);
        }
        void RenderSheet(ExcelSheetMetaData meta, ISheet sheet, JToken json)
        {
            foreach (var cellTemplate in meta.CellTemplates)
            {
                cellTemplate.SetValue(sheet, json);

            }
            foreach (var tableTemplate in meta.TableTemplates)
            {
               
                tableTemplate.SetValue(sheet, json);
            }

        }
        
    }
   

   

  


}