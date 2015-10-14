using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
namespace MySoft.Project.Core.Entity
{
    public class EntityGenerator
    {
        public string ConnectionString { get; set; }
        public string NameSpace { get; set; }
        public string TableName { get; set; }
        public EntityGenerator(string connectionString, string ns, string tableName)
        {
            ConnectionString = connectionString;
            NameSpace = ns;
            TableName = tableName;

        }
        StringBuilder _sb = new StringBuilder();
        string _identityColumn;
        int _indent = 0;
        public string Generate()
        {

            SqlConnection conn = new SqlConnection(ConnectionString);
            conn.Open();
            //如果需要database中全部table，则使用conn.GetSchema("Tables")即可
            string[] restrictions = new string[4];
            restrictions[1] = "dbo";
            //修改table名称
            restrictions[2] = TableName;
            System.Data.DataTable schema = conn.GetSchema("TABLES", restrictions);
            string selectQuery = "select * from @tableName";
            SqlCommand command = new SqlCommand(selectQuery, conn);
            SqlDataAdapter ad = new SqlDataAdapter(command);
            System.Data.DataSet ds = new DataSet();
            if (schema.Rows.Count == 0)
                return "";
            DataRow row = schema.Rows[0];
            var tableName = row["TABLE_NAME"].ToString();

            var cmd = new SqlCommand("SELECT name FROM SysColumns WHERE id=Object_Id('" + tableName + "') and colid=(select top 1 colid from sysindexkeys where id=Object_Id('" + tableName + "'))", conn);
            var pk = cmd.ExecuteScalar();
            if (pk != null)
                _identityColumn = pk.ToString();
            SqlDataAdapter ad3 = new SqlDataAdapter();
            DataSet ds3 = new DataSet();
            ad3.SelectCommand = new SqlCommand("SELECT table_name_c,field_name,field_name_c,b_pk FROM dbo.data_dict WHERE table_name='" + tableName + "'", conn);
            ad3.Fill(ds3);
            Dictionary<string, string> descDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (DataRow row2 in ds3.Tables[0].Rows)
            {
                descDict[tableName] = row2["table_name_c"].ToString();
                descDict[row2["field_name"].ToString()] = row2["field_name_c"].ToString();
            }
            string tabledesc = "";
            descDict.TryGetValue(tableName, out tabledesc);

            //namespace
            AddNameSpace();
            AddLine("namespace " + NameSpace);
            AddLine("{");

            //class
            _indent++;
            AddSummary(tabledesc);

            AddLine("public partial class " + tableName);
            AddLine("{");
            _indent++;
            #region 构造函数
            ds.Tables.Clear();
            command.CommandText = selectQuery.Replace("@tableName", row["TABLE_NAME"].ToString());
            ad.FillSchema(ds, SchemaType.Mapped, row["TABLE_NAME"].ToString());
            string defualtValueSql = @"SELECT  COLUMN_NAME ,                      
                    COLUMN_DEFAULT ,                                          
                    DATA_TYPE , 
                    CAST(CHARACTER_MAXIMUM_LENGTH AS INT) MAX_LENTH , 
                    CASE WHEN ic.object_id IS NULL THEN 0
                         ELSE 1
                    END AS IdentityColumn ,
                    CAST(ISNULL(ic.seed_value, 0) AS INT) AS IdentitySeed ,
                    CAST(ISNULL(ic.increment_value, 0) AS INT) AS IdentityIncrement ,
                    CASE WHEN st.collation_name IS NOT NULL THEN 1
                         ELSE 0
                    END AS IsCharColumn
            FROM    INFORMATION_SCHEMA.COLUMNS c
                    JOIN sys.columns sc ON c.TABLE_NAME = OBJECT_NAME(sc.object_id)
                                           AND c.COLUMN_NAME = sc.Name
                    LEFT JOIN sys.identity_columns ic ON c.TABLE_NAME = OBJECT_NAME(ic.object_id)
                                                         AND c.COLUMN_NAME = ic.Name
                    JOIN sys.types st ON COALESCE(c.DOMAIN_NAME,
                                                  c.DATA_TYPE) = st.name
                    LEFT OUTER JOIN sys.objects dobj ON dobj.object_id = sc.default_object_id
                                                        AND dobj.type = 'D'
            WHERE   c.TABLE_NAME = '{0}' 
            ORDER BY c.TABLE_NAME ,
                    c.ORDINAL_POSITION";
            SqlDataAdapter adapter = new SqlDataAdapter();
            DataSet dataset = new DataSet();
            adapter.SelectCommand = new SqlCommand(string.Format(defualtValueSql, tableName), conn);
            adapter.Fill(dataset);

            Dictionary<string, string> defValDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, int> lenDict = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (DataRow row1 in dataset.Tables[0].Rows)
            {

                if (row1["COLUMN_DEFAULT"] != DBNull.Value)
                    defValDict[row1["COLUMN_NAME"].ToString()] = row1["COLUMN_DEFAULT"].ToString().Replace("((", "").Replace("))", "").Replace("'", "");
                var len = row1["MAX_LENTH"];
                if (len != DBNull.Value)
                    lenDict[row1["COLUMN_NAME"].ToString()] = Convert.ToInt32(len);
            }


            AddLine("public " + tableName + "()");
            AddLine("{");
            _indent++;

            foreach (DataColumn dc1 in ds.Tables[0].Columns)
            {
                string columnName = dc1.ColumnName;
                string defVal = null;
                if (defValDict.TryGetValue(columnName, out defVal))
                {

                    if (dc1.DataType.IsPrimitive || dc1.DataType == typeof(decimal))
                    {

                        AddLine("this." + columnName + "  =  " + defVal.Replace("(", "").Replace(")", "") + " ;");
                    }
                    else if (dc1.DataType == typeof(string))
                    {

                        AddLine("this." + columnName + "  =  \"" + defVal.Replace("(", "").Replace(")", "") + "\";");
                    }

                }
            }


            _indent--;
            AddLine("}");

            #endregion
            AddLine("");
            #region 属性

            foreach (DataColumn dc in ds.Tables[0].Columns)
            {
                string allowDBNull = ((dc.DataType.IsPrimitive || dc.DataType == typeof(DateTime)) && dc.AllowDBNull && !(defValDict.ContainsKey(dc.ColumnName))) ? "?" : "";
                string desc;
                descDict.TryGetValue(dc.ColumnName, out desc);
                AddSummary(desc);
                if (_identityColumn == dc.ColumnName)
                {
                    AddLine("[ID]");
                }
                if (dc.DataType == typeof(string) && lenDict.ContainsKey(dc.ColumnName))
                {
                    var len = lenDict[dc.ColumnName];
                    if (len > 0)
                    {
                        AddLine("[StringLength(" + len + ")]");
                    }
                }
                AddLine("public " + GetFiledType(dc.DataType) + allowDBNull + "  " + dc.ColumnName + " { get; set; }");
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

        void AddNameSpace()
        {
            AddLine("using System;");
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
            else if (typeof(bool) == type)
            {
                return typeof(short).Name;
            }
            return type.Name;
        }

    }

}
