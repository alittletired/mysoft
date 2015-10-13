using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Threading;
using System.Web;
using System.Data.Common;
using System.Data;
using System.Diagnostics;
using Mysoft.Project.Core.DataAnnotations;
using System.ComponentModel;
using System.Reflection;
using System.IO;

namespace Mysoft.Project.Core
{
   
    public static class ConverterExtensions
    {
        static public T ChangeType<T>(this object obj)
        {
            return (T)ChangeType(obj, typeof(T));
        }
        static public object ChangeType(this object obj, Type type)
        {
            if (obj == null && !type.IsValueType)
                return null;
            if (obj == null && type.IsValueType && !type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
                throw new NullReferenceException("无法转换为空的值");

            if (type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                if (obj == null)
                {
                    return null;
                }
                NullableConverter nullableConverter = new NullableConverter(type);
                type = nullableConverter.UnderlyingType;
            }

            if (type == typeof(Guid)) return new Guid(obj.ToString());
            if (type == typeof(Version)) return new Version(obj.ToString());

            return Convert.ChangeType(obj.ToString(), type);
        }
    }

    public static class DBHelper
    {
        private static string getProviderName(DBType dbType)
        {

            switch (dbType)
            {
                case DBType.SqlServer:
                    return "System.Data.SqlClient";

                case DBType.Oracle:
                    return "System.Data.OracleClient";

                default:
                    return "MySql.Data.MySqlClient";

            }

        }

        static DBHelper()
        {
        
          
        }
        static string _connectionString;
        public static string ConnectionString
        {
            get
            {
                if (!string.IsNullOrEmpty(_connectionString))
                    return _connectionString;  
                _connectionString = (string)ReflectionHelper.InvokeMethod("Mysoft.Map.Data.MyDB.GetSqlConnectionString", "Mysoft.Map.Core");
                return _connectionString;
            }
            private set { _connectionString = value; }
        }

        public static DBType DBType
        {
            get;
            private set;
        }
        public static DbProviderFactory DbProviderFactory { get; private set; }
        public static void Execute(List<string> sqls)
        {
            var db = GetDatabase();
            using (var trans = db.BeginTransaction())
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < sqls.Count; i++)
                {
                    if (i > 0 && i % 50 == 0)
                    {
                        db.Execute(sb.ToString());
                        sb = new StringBuilder();
                    }

                    sb.Append(sqls[i]);
                }
                if (sb.Length > 0)
                    db.Execute(sb.ToString());               
                trans.Complete();
            }
          

        }
        public static int Execute(string sql, params  object[] args)
        {

            return GetDatabase().Execute(sql, args);
        }

        public static T ExecuteScalar<T>(string sql, params  object[] args)
        {
            var db = GetDatabase();
            return db.ExecuteScalar<T>(sql, args);

        }
        public static int ExecuteScalarInt(string sql, params  object[] args)
        {
            var db = GetDatabase();
            return db.ExecuteScalar<int>(sql, args);

        }
        public static DataTable GetDataTable(string sql, params object[] args) {
            var db = GetDatabase();
        
            return db.GetDataTable(sql, args);
            
        }
        public static string ExecuteScalarString(string sql, params  object[] args)
        {
            var db = GetDatabase();
            return db.ExecuteScalar<string>(sql, args);

        }
        public static List<T> GetList<T>(string strSql, params  object[] args)
        {
            var db = GetDatabase();
            return db.Fetch<T>(strSql, args);
        }

        public static T First<T>(string strSql, params  object[] args)
        {
            var db = GetDatabase();
            return db.FirstOrDefault<T>(strSql, args);
        }

        [ThreadStatic]
        static Database _db;
        public static Transaction BeginTransaction() {
            return GetDatabase().BeginTransaction();
        }
        static Database GetDatabase()
        {
            if (_db == null)
            {
            
                DBType = DBType.SqlServer;
                string providerName = getProviderName(DBType);
                DbProviderFactory = DbProviderFactories.GetFactory(providerName);
                _db = new Database(ConnectionString, DbProviderFactory, DBType);
            }           
            return _db;
        }
      
        public static int Update<Entity>(object param)
        {
            var meta = PocoData.ForType(typeof(Entity));
            return GetDatabase().Update(meta.TableInfo.TableName, meta.TableInfo.PrimaryKey, param, null);

        }
        public static int Update<Entity>(Entity poco)
        {
            var meta = PocoData.ForType(poco.GetType());
            return GetDatabase().Update(meta.TableInfo.TableName, meta.TableInfo.PrimaryKey, poco, null);

        }
        
        public static int Delete<Entity>(object idropoco)
        {
            return GetDatabase().Delete<Entity>(idropoco);
        }

        public static int Delete(object idropoco)
        {
            return GetDatabase().Delete(idropoco);
        }
        public static object Insert<Entity>(Entity poco)
        {
            var meta = PocoData.ForType(typeof(Entity));
            return Insert(meta, poco);
        }

        public static object Insert<Entity>(object poco)
        {
            var meta = PocoData.ForType(typeof(Entity));

            return Insert(meta, poco);
        }
        private static object Insert(PocoData meta, object poco) {
            string sql = "insert into {0}({1}) values ( {2}) ";
            var columns=meta.Columns.Keys.ToArray();
            var pocoMeta = PocoData.ForType(poco.GetType());
           var pocoClos = columns.Intersect(pocoMeta.Columns.Keys, StringComparer.OrdinalIgnoreCase);
           var strFileds = string.Join(",", pocoClos.ToArray());
           var strValues = string.Join(",@", pocoClos.ToArray());
             sql = string.Format(sql, meta.TableInfo.TableName, strFileds, "@" + strValues);
            return Execute(sql, poco);
            
        }
        // Insert a poco into a table.  If the poco has a property with the same name 
        // as the primary key the id of the new record is assigned to it.  Either way,
        // the new id is returned.
      
        public static string BuildCopySql<src, desc>(string srcAlias)
        {
            var srcMeta = PocoData.ForType(typeof(src));
            var descMeta = PocoData.ForType(typeof(desc));
            var columns = srcMeta.Columns.Keys.Intersect(descMeta.Columns.Keys, StringComparer.OrdinalIgnoreCase);
            string sql = "insert into {0}({1}) select {3} from {2} {4} ";
            var descColumns = string.Join(",", columns.ToArray());
            var srcColumns = descColumns;
            if (!string.IsNullOrEmpty(srcAlias))
            {
                srcColumns = srcAlias + "." + string.Join("," + srcAlias + ".", columns.ToArray());
            }
            else
            {
                srcAlias = "";
            }
            return string.Format(sql, descMeta.TableInfo.TableName, descColumns, srcMeta.TableInfo.TableName, srcColumns, srcAlias);

        }
        public static Entity GetByID<Entity>(object id)
        {
            var meta = PocoData.ForType(typeof(Entity));
            string sql = "select * from {0} where {1}=@0";
            sql = string.Format(sql, meta.TableInfo.TableName, meta.TableInfo.PrimaryKey);
            return GetDatabase().FirstOrDefault<Entity>(sql, id);
        }
    }
}
