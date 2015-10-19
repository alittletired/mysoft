using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;
using System.IO;
using Newtonsoft.Json;
using Toxy;
using System.Web.Hosting;

namespace Mysoft.Project.Excel
{
    public class ExcelHelper {
        public string BuildExcel(string templateFilePath, object data) {
            var randCode = Guid.NewGuid().ToString();
             var dir = "/tempfiles/excel/";
             var ext = Path.GetExtension(templateFilePath);
             var excelPath = dir + "/" + randCode + ext;

             return excelPath;
        }

    }
   
}