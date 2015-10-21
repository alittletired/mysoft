using System;
using System.Data;
using System.IO;
using NPOI.HSSF.UserModel;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NPOI.SS.UserModel;
using System.Text;

namespace Mysoft.Project.Excel
{
    public class ExcelHelper
    {
        /// <summary>
        /// 表及字段信息匹配:如$table:myUser,column:userguid#
        /// </summary>
        private Regex REG_EXPR = new Regex(@"\$table:(\w+),column:(\w+)\#");
        /// <summary>
        /// 循环方式匹配:如$each:row#或$each:col#
        /// </summary>
        private Regex REG_Each = new Regex(@"\$each:(\w+)\#");
        /// <summary>
        /// 锁定字段匹配:如$lock:IsDisabeld# 其中IsDisabled用1or0赋值
        /// </summary>
        private Regex REG_Lock = new Regex(@"\$lock:(\w+)\#");
        /// <summary>
        /// 导出Excel
        /// </summary>
        /// <param name="templateFilePath">Excel模版文件路径:如D:\map\upfiles\temp.xls</param>
        /// <param name="data">匹配Exceml模版的数据源</param>
        /// <returns></returns>
        public string ExportExcel(string templateFilePath, DataSet data)
        {
            string dirPath = AppDomain.CurrentDomain.BaseDirectory;
            var randCode = Guid.NewGuid().ToString().Replace("-", "");
            var dir = "/tempfiles/excel/";
            var excelPath = dir + "/" + randCode + ".xls";
            HSSFWorkbook hssfworkbook = WriteToFile(templateFilePath);
            for (int i = 0, j = hssfworkbook.NumberOfSheets; i < j; i++)
            {
                HSSFSheet sheet = hssfworkbook.GetSheetAt(i) as HSSFSheet;
                HSSFRow row = sheet.GetRow(0) as HSSFRow;
                if (row == null)
                {
                    break;
                }

                DrawSheet(sheet, i, data);

            }
            CreateRoot(dirPath + dir);
            using (FileStream file = new FileStream(dirPath + excelPath, FileMode.Create))
            {
                hssfworkbook.Write(file);
            }
            return excelPath;
        }
        /// <summary>
        /// 创建文件夹路径
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        private static string CreateRoot(string root)
        {
            string[] roots = root.Split('\\');
            StringBuilder dirClip = new StringBuilder();
            for (int n = 0; n < roots.Length; n++)
            {
                if (n == 0)
                {
                    dirClip.Append(roots[n]);
                }
                else
                {
                    dirClip.Append(roots[n]);
                }
                if (!Directory.Exists(dirClip.ToString()))
                {
                    Directory.CreateDirectory(dirClip.ToString());
                }
                dirClip.Append("\\");
            }
            if (!Directory.Exists(dirClip.ToString()))
            {
                Directory.CreateDirectory(dirClip.ToString());
            }
            return dirClip.ToString();
        }

        /// <summary>
        /// 插入行
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="rowIndex"></param>
        /// <param name="rows"></param>
        /// <param name="row"></param>
        private void MyInsertRow(HSSFSheet sheet, int rowIndex, DataRowCollection rows, HSSFRow row)
        {
            int rowCount = rows.Count;
            sheet.ShiftRows(rowIndex + 1, sheet.LastRowNum, rowCount - 1, true, false);
            short rowHeight = row.Height;
            for (int m = row.FirstCellNum; m < row.LastCellNum; m++)
            {
                string standValue = row.Cells[m].ToString();
                int dtRowIndex = 0;
                HSSFCell sourceCell = row.GetCell(m) as HSSFCell;
                bool isLock = false;
                for (int i = rowIndex + 1; i < rowIndex + rowCount; i++)
                {
                    dtRowIndex++;
                    HSSFRow targetRow = sheet.GetRow(i) as HSSFRow;
                    if (targetRow == null)
                    {
                        targetRow = sheet.CreateRow(i) as HSSFRow;
                    }
                    targetRow.Height = rowHeight;
                    if (sourceCell == null)
                    {
                        continue;
                    }
                    HSSFCell targetCell = targetRow.GetCell(m) as HSSFCell;
                    if (targetCell == null)
                    {
                        targetCell = targetRow.CreateCell(m) as HSSFCell;
                    }
                    targetCell.CellStyle = sourceCell.CellStyle;

                    isLock = false;
                    targetCell.SetCellValue(GetCellValue(standValue, rows[dtRowIndex], ref isLock));
                    if (isLock)
                    {
                        ICellStyle style = sheet.Workbook.CreateCellStyle();
                        style.CloneStyleFrom(targetCell.CellStyle);
                        style.IsLocked = true;
                        targetCell.CellStyle = style;
                    }
                }
                isLock = false;
                sourceCell.SetCellValue(GetCellValue(standValue, rows[0], ref isLock));
                if (isLock)
                {
                    ICellStyle style = sheet.Workbook.CreateCellStyle();
                    style.CloneStyleFrom(sourceCell.CellStyle);
                    style.IsLocked = true;
                    sourceCell.CellStyle = style;
                }
            }
        }

        /// <summary>
        /// 插入单元格
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="rowIndex"></param>
        /// <param name="cellIndex"></param>
        /// <param name="cellCount"></param>
        private void MyInsertCell(HSSFSheet sheet, int rowIndex, int cellIndex, int cellCount, DataRowCollection dtRow)
        {
            HSSFRow row = sheet.GetRow(rowIndex) as HSSFRow;
            HSSFCell cell = row.GetCell(cellIndex) as HSSFCell;
            int width = sheet.GetColumnWidth(cellIndex);
            for (int n = cellIndex + 1; n <= cellCount + cellIndex; n++)
            {
                sheet.SetColumnWidth(n, width);
            }
            int lastNum = row.LastCellNum;
            int cellNum = lastNum + cellCount;
            for (int n = cellIndex + 1; n < cellNum; n++)
            {
                HSSFCell cellR = row.GetCell(n) as HSSFCell;
                if (cellR == null)
                {
                    cellR = row.CreateCell(n) as HSSFCell;
                }
            }
            for (int n = lastNum; n > cellIndex; n--)
            {
                row.CopyCell(n, n + cellCount);
            }

            for (int n = cellIndex; n < cellIndex + cellCount; n++)
            {
                row.CopyCell(cellIndex, n + 1);
            }
            for (int n = cellIndex; n < cellIndex + cellCount + 1; n++)
            {
                bool isLock = false;
                row.Cells[n].SetCellValue(GetCellValue(row.Cells[n].ToString(), dtRow[n - cellIndex], ref isLock));
                if (isLock)
                {
                    ICellStyle style = sheet.Workbook.CreateCellStyle();
                    style.CloneStyleFrom(row.Cells[n].CellStyle);
                    style.IsLocked = true;
                    row.Cells[n].CellStyle = style;
                }
            }
        }

        /// <summary>
        /// 正则后的值
        /// </summary>
        /// <param name="expressValue"></param>
        /// <param name="dtVal"></param>
        /// <returns></returns>
        private string GetCellValue(string standValue, DataRow dtVal, ref bool isLock)
        {
            string[] express = null;
            string[] table = null;
            string[] column = null;
            TbColExpress(standValue, ref express, ref table, ref column);
            GroupCollection groups2 = REG_Each.Match(standValue).Groups;
            GroupCollection groups3 = REG_Lock.Match(standValue).Groups;
            string result = standValue;
            for (int n = 0; n < express.Length; n++)
            {
                if (!string.IsNullOrEmpty(express[n]))
                {
                    if (!string.IsNullOrEmpty(column[n]) && dtVal != null)
                    {
                        result = result.Replace(express[n], dtVal[column[n]].ToString());
                    }
                    else
                    {
                        result = result.Replace(express[n], "");
                    }
                }
            }
            if (!string.IsNullOrEmpty(groups2[0].Value))
            {
                result = result.Replace(groups2[0].Value, "");
            }
            if (!string.IsNullOrEmpty(groups3[0].Value))
            {

                result = result.Replace(groups3[0].Value, "");
                if (groups3.Count > 1)
                {
                    string lockInfo = groups3[1].Value;
                    if (lockInfo != "1" && lockInfo != "0")
                    {
                        isLock = dtVal[lockInfo].ToString() == "1";
                    }
                    else
                    {
                        isLock = lockInfo == "1";
                    }
                }

            }
            return result;
        }

        /// <summary>
        /// 获取单元格匹配的Table Column信息
        /// </summary>
        /// <param name="cellValue"></param>
        /// <param name="express"></param>
        /// <param name="table"></param>
        /// <param name="column"></param>
        private void TbColExpress(string cellValue, ref string[] express, ref string[] table, ref string[] column)
        {
            List<string> expressCol = new List<string>();
            List<string> tableCol = new List<string>();
            List<string> columnCol = new List<string>();
            int index = 0;
            Match match = REG_EXPR.Match(cellValue, index);
            index = match.Index + 1;
            GroupCollection groups = match.Groups;
            if (groups.Count > 1)
            {
                expressCol.Add(groups[0].Value);
                tableCol.Add(groups[1].Value);
                columnCol.Add(groups[2].Value);
                while (index > 0)
                {
                    match = REG_EXPR.Match(cellValue, index + 1);
                    index = match.Index;
                    groups = match.Groups;
                    expressCol.Add(groups[0].Value);
                    tableCol.Add(groups[1].Value);
                    columnCol.Add(groups[2].Value);
                }
            }
            express = expressCol.ToArray();
            table = tableCol.ToArray();
            column = columnCol.ToArray();
        }

        /// <summary>
        /// 页签描绘
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="sheetIndex"></param>
        /// <param name="data"></param>
        private void DrawSheet(HSSFSheet sheet, int sheetIndex, DataSet data)
        {
            for (int i = 0; i <= sheet.LastRowNum; i++)
            {
                HSSFRow row = sheet.GetRow(i) as HSSFRow;
                if (row != null)
                {
                    bool addedRow = false;
                    for (int a = 0; a < row.Cells.Count; a++)
                    {
                        HSSFCell cel = row.Cells[a] as HSSFCell;
                        string Tablename = "";
                        string Column = "";
                        string Each = "";
                        string Lock = "";
                        string tbcolExpress = "", eachExpress = "", lockExpress = "";
                        string cellValue = cel.ToString().Replace("$", ".$");
                        string[] vals = cellValue.Split('.');
                        for (int n = 0; n < vals.Length; n++)
                        {
                            if (vals[n].Trim() == "")
                            {
                                continue;
                            }
                            GroupCollection groups = REG_EXPR.Match(vals[n]).Groups;
                            if (groups.Count == 1)
                            {
                                groups = REG_Each.Match(vals[n]).Groups;
                                if (groups.Count == 1)
                                {
                                    groups = REG_Lock.Match(vals[n]).Groups;
                                    if (groups.Count == 1)
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        lockExpress = groups[0].Value;
                                        Lock = groups[1].Value;
                                    }
                                }
                                else
                                {
                                    eachExpress = groups[0].Value;
                                    Each = groups[1].Value;
                                }
                            }
                            else
                            {
                                tbcolExpress = groups[0].Value;
                                Tablename = groups[1].Value;
                                Column = groups[2].Value;
                            }
                        }
                        if (!string.IsNullOrEmpty(Tablename) && !addedRow)
                        {
                            DataTable dt = null;
                            int tableIndex = 0;
                            if (int.TryParse(Tablename, out tableIndex))
                            {
                                dt = data.Tables[tableIndex];
                            }
                            else
                            {
                                dt = data.Tables[Tablename];
                            }
                            cellValue = cel.ToString();
                            if (Each == "row")
                            {
                                if (!addedRow)
                                {
                                    MyInsertRow(sheet, cel.RowIndex, dt.Rows, row);
                                    i += dt.Rows.Count - 1;
                                    addedRow = true;
                                }
                            }
                            else if (Each == "col")
                            {
                                MyInsertCell(sheet, cel.RowIndex, cel.ColumnIndex, dt.Rows.Count - 1, dt.Rows);
                                a += dt.Rows.Count;
                            }
                            else
                            {
                                DataRow dtRow = null;
                                if (dt.Rows.Count > 0)
                                {
                                    dtRow = dt.Rows[0];
                                }
                                bool isLock = false;
                                cel.SetCellValue(GetCellValue(cel.ToString(), dtRow, ref isLock));
                                if (isLock)
                                {
                                    ICellStyle style = sheet.Workbook.CreateCellStyle();
                                    style.CloneStyleFrom(cel.CellStyle);
                                    style.IsLocked = true;
                                    cel.CellStyle = style;
                                }
                            }
                        }
                    }
                }
            }
            sheet.ProtectSheet("mysoft");
        }

        /// <summary>
        /// 模版Excel内容读取
        /// </summary>
        /// <param name="filePath">模版路径</param>
        /// <returns></returns>
        private HSSFWorkbook WriteToFile(string filePath)
        {
            FileStream file = new FileStream(filePath, FileMode.Open);
            HSSFWorkbook hssfworkbook = new HSSFWorkbook(file);
            file.Close();
            return hssfworkbook;
        }

        /// <summary>
        /// 根据Excel流获取DataTable
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public DataSet GetDataFromExcel(Stream s)
        {
            DataSet result = new DataSet();
            HSSFWorkbook workbook = new HSSFWorkbook(s);
            for (int n = 0; n < workbook.NumberOfSheets; n++)
            {
                HSSFSheet sheet = workbook.GetSheetAt(n) as HSSFSheet;
                DataTable table = new DataTable();
                HSSFRow headerRow = sheet.GetRow(0) as HSSFRow;
                if (headerRow == null)
                {
                    continue;
                }
                int cellCount = headerRow.LastCellNum;
                for (int i = headerRow.FirstCellNum; i < cellCount; i++)
                {
                    DataColumn column = new DataColumn(headerRow.GetCell(i).StringCellValue);
                    table.Columns.Add(column);
                }
                int rowCount = sheet.LastRowNum;
                for (int i = (sheet.FirstRowNum + 1); i < sheet.LastRowNum; i++)
                {
                    HSSFRow row = sheet.GetRow(i) as HSSFRow;
                    DataRow dataRow = table.NewRow();
                    for (int j = row.FirstCellNum; j < cellCount; j++)
                    {
                        if (row.GetCell(j) != null)
                        {
                            dataRow[j] = row.GetCell(j).ToString();
                        }
                    }

                    table.Rows.Add(dataRow);
                }
                result.Tables.Add(table);
                sheet = null;
            }
            workbook = null;
            return result;
        }
    }
}