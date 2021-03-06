﻿using System;
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
        static Dictionary<Type, object> typeDefaultValues = new Dictionary<Type, object>();
        static readonly MethodInfo _TypeDefaultValueMethod = typeof(ConverterExtensions).GetMethod("GetTypeDefaultValue", BindingFlags.NonPublic | BindingFlags.Static);

        static public T ChangeType<T>(this object obj)
        {
            return (T)ChangeType(obj, typeof(T));
        }
        static object GetTypeDefaultValue<T>()
        {
            return default(T);
        }

        static public object ChangeType(this object obj, Type type)
        {
            //对象为空则返回目标类型的默认值
            if (obj == null)
            {
                if (type.IsClass)
                    return null;
                var method = _TypeDefaultValueMethod.MakeGenericMethod(new[] { type });
                return method.Invoke(null, null);
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                NullableConverter nullableConverter = new NullableConverter(type);
                type = nullableConverter.UnderlyingType;
            }
            if (type == typeof(Guid)) return new Guid(obj.ToString());
            if (type == typeof(Version)) return new Version(obj.ToString());
            var val = obj;
            if (obj.GetType() == typeof(Guid))
                val = obj.ToString();
            return Convert.ChangeType(val, type);
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
                try
                {
                    _connectionString = (string)ReflectionHelper.InvokeMethod("Mysoft.Map.Data.MyDB.GetSqlConnectionString", "Mysoft.Map.Core");
                }
                catch { }
                return _connectionString;
            }
             set { _connectionString = value; }
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
                //Batch size= 65,536 * Network Packet Size
                //65,536 *1000
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < sqls.Count; i++)
                {
                    if (i > 0 && i % 5000 == 0)
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
            if (string.IsNullOrEmpty(ConnectionString))
                return new Transaction(null);
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
            return GetDatabase().Update(param, meta);

        }
        public static int Update<Entity>(Entity poco)
        {
            var meta = PocoData.ForType(typeof(Entity));
            return GetDatabase().Update(poco, meta); 
        }


        public static int Save<Entity>(object poco)
        {
            var meta = PocoData.ForType(typeof(Entity));
            var keyValue = meta.Columns[meta.TableInfo.PrimaryKey].GetValue(poco);
            if (keyValue == null || keyValue.ToString() == string.Empty)
            {
                keyValue = Guid.NewGuid().ToString();
                meta.Columns[meta.TableInfo.PrimaryKey].SetValue(poco, keyValue);
                return Insert(poco,meta);
            }

            return GetDatabase().Update(poco, meta);

        }
        public static int Save<Entity>(Entity poco)
        {
            var meta = PocoData.ForType(typeof(Entity));
            var keyValue = meta.Columns[meta.TableInfo.PrimaryKey].GetValue(poco);
            if (keyValue == null || keyValue.ToString() == string.Empty)
            {
                keyValue = Guid.NewGuid().ToString();
                meta.Columns[meta.TableInfo.PrimaryKey].SetValue(poco, keyValue);
                return Insert(poco,meta);
            }
            return GetDatabase().Update(poco, meta);
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
            return Insert(poco,meta);
        }

        public static int Insert<Entity>(object poco)
        {
            var meta = PocoData.ForType(typeof(Entity));

            return Insert(poco,meta);
        }
        private static int Insert(object poco, PocoData meta)
        {
            string sql = "insert into {0}({1}) values ( {2}) ";
            var columns = meta.Columns.Keys.ToArray();
            var pocoMeta = PocoData.ForType(poco.GetType());
            var pocoClos = columns.Intersect(pocoMeta.Columns.Keys, StringComparer.OrdinalIgnoreCase);
            pocoClos = pocoClos.Where(col => !pocoMeta.IgnoreColumns.ContainsKey(col)).ToList();
            var strFileds = string.Join(",", pocoClos.ToArray());
         
            var strValues = string.Join(",@", pocoClos.ToArray());
            sql = string.Format(sql, meta.TableInfo.TableName, strFileds, "@" + strValues);
            return Execute(sql, poco);
        }      
     
        public static Entity GetByID<Entity>(object id)
        {
            var meta = PocoData.ForType(typeof(Entity));
            string sql = "select {2} from {0} where {1}=@0";
            var columns = meta.Columns.Keys.Where(col => !meta.IgnoreColumns.ContainsKey(col)).ToArray();
            var strFileds = string.Join(",", columns.ToArray());
            sql = string.Format(sql, meta.TableInfo.TableName, meta.TableInfo.PrimaryKey, strFileds);
            return GetDatabase().FirstOrDefault<Entity>(sql, id);
        }
    }
}
