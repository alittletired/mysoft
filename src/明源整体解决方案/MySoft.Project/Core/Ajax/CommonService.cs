using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mysoft.Project.Core;
namespace Mysoft.Project.Expand
{
    public class CommonService
    {
        public virtual string GetProcessGUID(string BusinessGUID, string BusinessType)
        {
            string sql = @"select ProcessGUID from myWorkflowProcessEntity
WHERE BusinessGUID=@BusinessGUID AND BusinessType=@BusinessType  AND ISNULL(IsHistory,0) = 0";
            return DBHelper.ExecuteScalarString(sql, new { BusinessGUID = BusinessGUID, BusinessType = BusinessType });


        }
    }
}
