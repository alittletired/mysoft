
using System.Web;
using System;
using System.Data;

using System.Data.SqlClient;
using System.Xml;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Web.Hosting;
using System.Reflection;

using Mysoft.Project.Core;
using System.Linq;

namespace Mysoft.Project.Excel
{


    public class ExcelDemoService
    {
        
        public string TestExportDatatable(string fileTemplate)
        {

            var data = DBHelper.GetDataTable(@"select top 300 RoomGUID,Room,RoomCode,HuXing,Total,Price,Status,SLControlDate 
                from p_Room where SLControlDate is not null  order by  Roomcode ");
            var str = ExcelHelper.ExportExcel(fileTemplate, data);
            return str;
        }

        public string TestExportList(string fileTemplate)
        {
            var data = DBHelper.GetDataTable(@"SELECT * FROM dbo.cb_Cost WHERE ProjectCode='C01BJ01'  order by  CostCode ");
            var str = ExcelHelper.ExportExcel(fileTemplate, data);
            
            return str;
        }

       
        public DataTable TestImportDatatable(string filePath)
        {

            return ExcelHelper.Import<DataTable>(filePath);

        }
      
        //        public string ExportWithoutTemplate()
        //        {

        //            var data = DBHelper.GetDataTable(@"select top 300 RoomGUID,Room,RoomCode,HuXing,Total,Price,Status,SLControlDate 
        //from p_Room  order by  Roomcode asc");
        //            ExcelHelper.Export(fileTemplate, data);
        //        }

    }
}
