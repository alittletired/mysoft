using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Mysoft.Project.Core;
using Mysoft.Project.Ajax;

namespace Mysoft.Project.Control
{
    public class myRepeaterService
    {
        //获取分页数据
        public object GetPageData(int pagesize, int pageindex, string sortfield, string servicemethod, string customerParam)
        {

           object rtnData= ReflectionHelper.Invoke(servicemethod, customerParam);

           DataTable dt = (DataTable)rtnData;

           DataTable dtPageData = SplitDataTable(dt, pageindex, pagesize, sortfield);

           var data = new { total = dt.Rows.Count, pageindex = pageindex, pagesize = pagesize, items = dtPageData };

           return data;

        }

        private DataTable SortDesc(DataTable dt,string sortfield)
        {
            DataView dv = new DataView();
            dv.Table = dt;
            dv.Sort = sortfield;
            return dv.ToTable();
        }

        /**/
        /// <summary>
        /// 根据索引和pagesize返回记录
        /// </summary>
        /// <param name="dt">记录集 DataTable</param>
        /// <param name="PageIndex">当前页</param>
        /// <param name="pagesize">一页的记录数</param>
        /// <returns></returns>
        public  DataTable SplitDataTable(DataTable dt, int PageIndex, int PageSize,string sortfield)
        {
            dt = SortDesc(dt, sortfield);
            if (PageIndex == 0)
                return dt;
            DataTable newdt = dt.Clone();
            //newdt.Clear();
            int rowbegin = (PageIndex - 1) * PageSize;
            int rowend = PageIndex * PageSize;

            if (rowbegin >= dt.Rows.Count)
                return newdt;

            if (rowend > dt.Rows.Count)
                rowend = dt.Rows.Count;
            for (int i = rowbegin; i <= rowend - 1; i++)
            {
                DataRow newdr = newdt.NewRow();
                DataRow dr = dt.Rows[i];
                foreach (DataColumn column in dt.Columns)
                {
                    newdr[column.ColumnName] = dr[column.ColumnName];
                }
                newdt.Rows.Add(newdr);
            }

            return newdt;
        }


        public DataTable GetRepeaterDemoData()
        {
            return DBHelper.GetDataTable("select top 100 RoomGUID,Room,RoomCode,HuXing,Total,Price,Status,SLControlDate from dbo.p_Room order by  Roomcode asc");
        }

    }
}
