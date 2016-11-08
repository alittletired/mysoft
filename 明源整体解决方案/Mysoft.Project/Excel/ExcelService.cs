
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
using Mysoft.Project.Json.Linq;
using System.Collections;

namespace Mysoft.Project.Excel
{

    public class TemplateExportlDTO
    {
        public string ServiceMethod { get; set; }
        public string CustomerParam { get; set; }
        public string FileName { get; set; }
        public string TemplateFile { get; set; }
    }
    public class TemplateImportlDTO
    {
        public string ServiceMethod { get; set; }
        public string CustomerParam { get; set; }    
        public string Id { get; set; }
        public string DocType { get; set; }
    }
    public class ExcelService
    {
        public HttpResult ExportExcel(TemplateExportlDTO dto)
        {
            object excelData = ReflectionHelper.Invoke(dto.ServiceMethod, dto.CustomerParam);
            var url = ExcelHelper.ExportExcel(dto.TemplateFile, excelData);
            return HttpResult.Ok(url);
        }
        public HttpResult ImportExcel(TemplateImportlDTO dto)
        {

            var methodInfo = ReflectionHelper.GetMethod(dto.ServiceMethod, "");
            ParameterInfo[] paramterInfos = methodInfo.GetParameters();
            if (paramterInfos.Length == 0)
            {
                throw new Exception(dto.ServiceMethod + "方法第一个参数必须实现了IEnumerable,用来接受excel导入的数据");

            }
            var path = DBHelper.ExecuteScalarString("SELECT Location+FileName FROM p_Documents WHERE FkGUID=@0 AND DocType=@1", dto.Id, dto.DocType);
            if (string.IsNullOrEmpty(path)) {
             return   HttpResult.Ok("");
            }
            var data = ExcelHelper.Import(path);
            JObject jobj = JObject.Parse(dto.CustomerParam ?? "{}");
            jobj[paramterInfos[0].Name] = data;
            var res = ReflectionHelper.Invoke(methodInfo, jobj);
            return HttpResult.Ok(res);

        }


    }
}
