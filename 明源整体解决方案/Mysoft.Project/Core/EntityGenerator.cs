using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
namespace Mysoft.Project.Core.Entity
{
    public class EntityGenerator
    {
        public string ConnectionString { get; set; }
        public string NameSpace { get; set; }
        public string TableName { get; set; }
        Database _db;
        public EntityGenerator(string connectionString, string ns, string tableName)
        {
            ConnectionString = connectionString;
            NameSpace = ns;
            TableName = tableName;

        }
        StringBuilder _sb = new StringBuilder();      
        int _indent = 0;
        class ColumnInfo {
            public string ColumnName { get; set; }
            public string DbType { get; set; }
            public string DefaultValue { get; set; }
            public int? MaxLength { get; set; }
            public Type DataType { get; set; }
            public int IsNullable { get; set; }
        }
        /// <summary>
        /// 获取大小写敏感的数据库表名
        /// </summary>
        /// <returns></returns>
        string GetTableName() {
          return  _db.ExecuteScalar<string>("SELECT * FROM  sys.objects WHERE object_id=object_id('" + TableName + "')");
        }
        /// <summary>
        /// 获取表的主键
        /// </summary>
        /// <returns></returns>
        string GetPKColumn()
        {
            return _db.ExecuteScalar<string>("SELECT name FROM SysColumns WHERE id=Object_Id('" + TableName + "') and colid=(select top 1 colid from sysindexkeys where id=Object_Id('" + TableName + "'))");
        }
        Dictionary<string, string> GetTableDesc() { 
             Dictionary<string, string> descDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
             var sql = "SELECT table_name_c,field_name,field_name_c,b_pk FROM dbo.data_dict WHERE table_name='" + TableName + "'";
             var tabel = _db.GetDataTable(sql);
             foreach (DataRow row in tabel.Rows)
             {
                 descDict[TableName] = row["table_name_c"].ToString();
                 descDict[row["field_name"].ToString()] = row["field_name_c"].ToString();
             }
             return descDict;
        }
        /// <summary>
        /// 生成实体类方法
        /// </summary>
        /// <returns></returns>
        public string Generate()
        {
            _db = new Database(ConnectionString, "System.Data.SqlClient");
           
            var tableName = GetTableName();
            if (string.IsNullOrEmpty(tableName)) return "";          
             var    pkColumn =GetPKColumn();          

            var descDict = GetTableDesc();
            string tabledesc = "";
            descDict.TryGetValue(tableName, out tabledesc);

            //namespace
            AddImport();
            AddLine("namespace " + NameSpace);
            AddLine("{");

            //class
            _indent++;
            AddSummary(tabledesc);

            AddLine("public partial class " + tableName);
            AddLine("{");
            _indent++;
            #region 构造函数

            string coloinfoSql = @"SELECT  COLUMN_NAME ColumnName ,
        COLUMN_DEFAULT DefaultValue ,
        DATA_TYPE DbType ,
        CAST(CHARACTER_MAXIMUM_LENGTH AS INT) MaxLength ,
        CASE WHEN is_nullable = 'yes' THEN 1
             ELSE 0
        END isnullable
FROM    information_schema.COLUMNS
WHERE   TABLE_NAME = @0 ";
            var columns = _db.Query<ColumnInfo>(coloinfoSql, tableName).ToList();

          


            AddLine("public " + tableName + "()");
            AddLine("{");
            _indent++;

            foreach (var col in columns)
            {
                col.DataType = SqlServerType.SqlType2CsharpType(SqlServerType.SqlTypeString2SqlType(col.DbType));
                string columnName = col.ColumnName;

                if (!string.IsNullOrEmpty(col.DefaultValue)) {
                    var defVal = col.DefaultValue.Replace("(", "").Replace(")", "");
                    if (col.DataType.IsPrimitive || col.DataType == typeof(decimal))
                    {

                        AddLine("this." + columnName + " = " + defVal + " ;");
                    }
                    else if (col.DataType == typeof(string))
                    {

                        AddLine("this." + columnName + " = \"" + defVal + "\";");
                    }
                }
               
            }


            _indent--;
            AddLine("}");

            #endregion
            AddLine("");
            #region 属性

            foreach (var col in columns)
            {
                string allowDBNull = ((col.DataType.IsPrimitive || col.DataType == typeof(DateTime)) && col.IsNullable == 1 && string.IsNullOrEmpty(col.DefaultValue)) ? "?" : "";
                string desc;
                descDict.TryGetValue(col.ColumnName, out desc);
                AddSummary(desc);
                if (pkColumn == col.ColumnName)
                {
                    AddLine("[ID]");
                }
                if (col.DataType == typeof(string) && col.MaxLength.HasValue && col.MaxLength > 0)
                {
                    AddLine("[StringLength(" + col.MaxLength.Value + ")]");
                }

                AddLine("[DbType(SqlDbType." + SqlServerType.SqlTypeString2SqlType(col.DbType) + ")]");
                AddLine("public " + GetFiledType(col.DataType) + allowDBNull + " " + col.ColumnName + " { get; set; }");
                AddLine("");
            }

            #endregion

            //end
            _indent--;
            AddLine("}");
            _indent--;
            AddLine("}");

            return _sb.ToString();
        }

        /// <summary>
        /// 引入名字空间
        /// </summary>
        void AddImport()
        {
            AddLine("using System;");
            AddLine("using System.Data;");
            AddLine("using Mysoft.Project.Core.DataAnnotations;");
            AddLine("using System.ComponentModel.DataAnnotations;");

        }

        void AddLine(string text)
        {
            text = text ?? "";
            var whitespace = "";
            for (int i = 0; i < _indent; i++)
            {
                whitespace += "\t";
            }
            _sb.AppendLine(whitespace + text);
        }

        void AddSummary(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }
            AddLine("/// <summary>");
            AddLine("///" + text);
            AddLine("/// </summary>");
        }
        string GetFiledType(Type type)
        {
            if (typeof(System.Guid) == type)
            {
                return typeof(string).Name;
            }          
            return type.Name;
        }

    }

}
