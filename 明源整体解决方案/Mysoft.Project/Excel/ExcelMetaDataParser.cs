using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NPOI.SS.UserModel;
using System.Web.Hosting;
using System.IO;
using System.Text.RegularExpressions;

namespace Mysoft.Project.Excel
{
    internal class ExcelMetaDataParser
    {
        static readonly Regex REG_Template = new Regex(@"\#\{([^\}]+)\}");



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
                        tableTemplate.BindName = pair[1];
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
            IWorkbook workbook = WorkbookFactory.Create(filePath);
            return Parse(workbook);

        }




    }
}
