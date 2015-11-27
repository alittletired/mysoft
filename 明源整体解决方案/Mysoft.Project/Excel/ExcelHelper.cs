using System;
using System.Data;
using System.IO;
using NPOI.HSSF.UserModel;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NPOI.SS.UserModel;
using System.Text;
using System.Web.Hosting;
using Mysoft.Project.Json.Linq;
using System.Linq;
using Mysoft.Project.Json;
using NPOI.HSSF.Util;
using Mysoft.Project.Core;
namespace Mysoft.Project.Excel
{
    public class ExcelHelper
    {

        public static string ExportExcel(ExcelMetaData meta, object data)
        {
            string dirPath = AppDomain.CurrentDomain.BaseDirectory;
            var randCode = Guid.NewGuid().ToString().Replace("-", "");
            var dir = "/tempfiles/excel/";
            var excelPath = dir + randCode + ".xls";
            var physicalPath = HostingEnvironment.MapPath(excelPath);
            FileInfo fi = new FileInfo(physicalPath);
            if (!fi.Directory.Exists)
                fi.Directory.Create();
            meta.Render(data);
            using (FileStream fs = new FileStream(physicalPath, FileMode.Create))
            {
                meta.Workbook.Write(fs);
            }

            return excelPath;
        }

        
        /// <summary>
        /// 导出Excel
        /// </summary>
        /// <param name="templateFilePath">Excel模版文件路径:如D:\map\upfiles\temp.xls</param>
        /// <param name="data">匹配Excel模版的数据源</param>
        /// <returns></returns>
        public static string ExportExcel(string templateFilePath, object data)
        {
            var meta = ExcelMetaData.FromTemplate(templateFilePath);
            return ExportExcel(meta, data);
          
        }

        //自定义简单列表导出
        public static string ExportExcel(List<ExcelColumn> cols, object data)
        {
            var filePath = "/Project/js/excel/NullTemplate.xls";
            filePath = HostingEnvironment.MapPath(filePath);
            IWorkbook workbook = WorkbookFactory.Create(filePath);
            var sheet = workbook.GetSheetAt(0);
            var namecell = sheet.GetRow(0).GetCell(0);
            var filedcell = sheet.GetRow(1).GetCell(0);
            //添加树形支持
            var treeCode = "";
            var treeCol = cols.FirstOrDefault(o => !string.IsNullOrEmpty(o.TreeCode));
            if (treeCol != null) {
                treeCode = ",treeCode:" + treeCol.TreeCode;
            }
            foreach (var col in cols)
            {
                namecell.SetCellValue(col.Name);
                if (filedcell.ColumnIndex == 0)
                    filedcell.SetCellValue("#{each:$root,value:" + col.Filed + treeCode+"}");
                else
                    filedcell.SetCellValue("#{" + col.Filed + "}");
                sheet.SetColumnWidth(filedcell.ColumnIndex, col.Width * 32);
                namecell = namecell.CopyCellTo(namecell.ColumnIndex + 1);
                filedcell = filedcell.CopyCellTo(filedcell.ColumnIndex + 1);
            }
            var meta = ExcelMetaData.FromExcel(workbook);
            return ExportExcel(meta, data);


        }

        public static T Import<T>(string filePath) {

            if (filePath.StartsWith("/"))
                filePath = HostingEnvironment.MapPath(filePath);
            IWorkbook workbook = WorkbookFactory.Create(filePath);
            ExcelMetaData meta=new ExcelMetaData();
            for (int i = 0; i < workbook.NumberOfSheets; i++)
            {
                ISheet sheet = workbook.GetSheetAt(i);
                if (sheet.SheetName == "模版元数据") {
                  
                    var json = sheet.GetRow(0).GetCell(0).ToString();
                    meta = JsonConvert.DeserializeObject<ExcelMetaData>(json);
                    break;
                }
                
            }

          
         
           
            
            //判断是否是数组或者对象
            JToken token=JToken.Parse("{}");
            if (meta.IsArray)
                token = JToken.Parse("[]");

            foreach (var sheetMeta in meta.Sheets)
            {
                var sheet = workbook.GetSheetAt(sheetMeta.SheetIndex);
                foreach (var cellTemplate in sheetMeta.CellTemplates)
                {
                    cellTemplate.GetValue(sheet, token);

                }
                foreach (var tableTemplate in sheetMeta.TableTemplates)
                {
                    tableTemplate.GetValue(sheet, token);
                }


            }
            var data = token.ToObject<T>();
            return data;
        }

    }
}