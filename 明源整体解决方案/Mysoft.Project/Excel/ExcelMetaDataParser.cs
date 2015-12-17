using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mysoft.Project.NPOI.SS.UserModel;
using System.Web.Hosting;
using System.IO;
using System.Text.RegularExpressions;

namespace Mysoft.Project.Excel
{
    internal class ExcelMetaDataParser
    {
        #region 无用
        //public object GetCellValue(ICell cell)
        //{
        //    object value = null;
        //    try
        //    {
        //        if (cell.CellType != CellType.Blank)
        //        {
        //            switch (cell.CellType)
        //            {
        //                case CellType.Numeric:
        //                    // Date comes here
        //                    if (DateUtil.IsCellDateFormatted(cell))
        //                    {
        //                        value = cell.DateCellValue;
        //                    }
        //                    else
        //                    {
        //                        // Numeric type
        //                        value = cell.NumericCellValue;
        //                    }
        //                    break;
        //                case CellType.Boolean:
        //                    // Boolean type
        //                    value = cell.BooleanCellValue;
        //                    break;
        //                case CellType.Formula:
        //                    value = cell.CellFormula;
        //                    break;
        //                default:
        //                    // String type
        //                    value = cell.StringCellValue;
        //                    break;
        //            }
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        value = "";
        //    }

        //    return value;
        //}

        ////根据数据类型设置不同类型的cell
        //public static void SetCellValue(ICell cell, object obj)
        //{
        //    if (obj.GetType() == typeof(int))
        //    {
        //        cell.SetCellValue((int)obj);
        //    }
        //    else if (obj.GetType() == typeof(double))
        //    {
        //        cell.SetCellValue((double)obj);
        //    }
        //    else if (obj.GetType() == typeof(IRichTextString))
        //    {
        //        cell.SetCellValue((IRichTextString)obj);
        //    }
        //    else if (obj.GetType() == typeof(string))
        //    {
        //        cell.SetCellValue(obj.ToString());
        //    }
        //    else if (obj.GetType() == typeof(DateTime))
        //    {
        //        cell.SetCellValue((DateTime)obj);
        //    }
        //    else if (obj.GetType() == typeof(bool))
        //    {
        //        cell.SetCellValue((bool)obj);
        //    }
        //    else
        //    {
        //        cell.SetCellValue(obj.ToString());
        //    }
        //}

        #endregion

        static readonly Regex REG_Template = new Regex(@"\#\{([^\}]+)\}");

        static readonly string PROTECT_PASSWROD = "1";

        public ExcelMetaDataParser()
        {



        }

        void ParseTable(ICell cell, string template, ExcelSheetMetaData sheetMeta)
        {
            var tokens = template.Split(',');
            ExcelTable tableTemplate = new ExcelTable()
            {
                RowIndex = cell.RowIndex,
                ColIndex = cell.ColumnIndex,

            };


            foreach (var token in tokens)
            {
                var pair = token.Split(':');

                switch (pair[0].ToLower())
                {
                    case "each":
                        tableTemplate.Each = pair[1];
                        break;
                    case "direction":
                        tableTemplate.Direction = pair[1];
                        break;
                    case "treecode":
                        tableTemplate.TreeCode = pair[1];
                        break;
                }

            }

            var row = cell.Row;
            for (int i = cell.ColumnIndex; i < row.LastCellNum; i++)
            {
                ParseCell(row.Cells[i], tableTemplate.Cells);
            }
            if (tableTemplate.Cells.FirstOrDefault(o => o.IsHide || o.IsLock) != null)
            {
                cell.Sheet.ProtectSheet(PROTECT_PASSWROD);
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
            var expr = cell.ToString().Trim();
            if (string.IsNullOrEmpty(expr))
                return null;
            var match = REG_Template.Match(expr);
            if (!match.Success)
                return null;
            var template = match.Groups[1].Value;
            return template;
        }


        void ParseCell(ICell cell, List<ExcelCell> listCell)
        {

            ParseCell(cell, null, listCell);
        }


        void ParseCell(ICell cell, string template, List<ExcelCell> listCell)
        {
            if (cell == null)
                return;
            template = template ?? GetTemplate(cell);
            if (string.IsNullOrEmpty(template))
                return;
            var cellTemplate = new ExcelCell() { RowIndex = cell.RowIndex, ColIndex = cell.ColumnIndex };
            var templates = template.Split(new char[] { ',', '，' });
            var cellstyle = cell.CellStyle;
            if (cellstyle == null)
                cellstyle = cell.Sheet.Workbook.CreateCellStyle();
            else
            {
                var cellstyle1 = cell.Sheet.Workbook.CreateCellStyle();
                cellstyle1.CloneStyleFrom(cellstyle);
                cellstyle = cellstyle1;
            }
               
            cellstyle.IsLocked = false;
            foreach (var rawtemp in templates)
            {
                var temp = rawtemp.Trim();
                if (temp.IndexOf(":") > 0)
                {
                    var pair = temp.Split(':');
                    switch (pair[0].ToLower())
                    {

                        case "islock":
                            var islock = pair[1].ToLower();
                            cellstyle.IsLocked = cellTemplate.IsLock = islock == "true" || islock == "1";
                            break;
                        case "ishide":
                            var ishide = pair[1].ToLower();
                            cellTemplate.IsHide = ishide == "true" || ishide == "1";
                            cell.Sheet.SetColumnHidden(cell.ColumnIndex, true);
                            break;
                        case "bind":
                            cellTemplate.Bind = pair[1];
                            break;
                        case "width":
                            cellTemplate.Width = Convert.ToInt32(pair[1]);
                            cell.Sheet.SetColumnWidth(cell.ColumnIndex, cellTemplate.Width * 32);
                            break;
                    }
                }
                else
                {
                    cellTemplate.Bind = temp;
                }
            }
            if (cellstyle.IsLocked)
            {
                cellstyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index;
                cellstyle.FillPattern = FillPattern.ThickBackwardDiagonals;// NPOI.SS.UserModel.FillPatternType.THICK_BACKWARD_DIAG
                cellstyle.FillBackgroundColor = NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index;
                //cellstyle.BorderLeft = NPOI.SS.UserModel.CellBorderType.NONE
                //cellstyle.BorderTop = NPOI.SS.UserModel.CellBorderType.NONE
                //cellstyle.BorderRight = NPOI.SS.UserModel.CellBorderType.NONE
                //cellstyle.BorderBottom = NPOI.SS.UserModel.CellBorderType.NONE

            }


            cell.CellStyle = cellstyle;
            listCell.Add(cellTemplate);

        }

        ExcelSheetMetaData ParseSheet(ISheet sheet)
        {

            ExcelSheetMetaData sheetMeta = new ExcelSheetMetaData();
            sheetMeta.SheetName = sheet.SheetName;
           
            foreach (IRow row in sheet)
            {
                ParseRow(row, sheetMeta);
            }
            return sheetMeta;

        }

        public ExcelMetaData Parse(IWorkbook workbook)
        {
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
            using (FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                IWorkbook workbook = WorkbookFactory.Create(file);
                //sheet1.ForceFormulaRecalculation = true;
                return Parse(workbook);
            }

        }




    }
}
