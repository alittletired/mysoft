
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Data.Common;
using System.Data;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq.Expressions;
using System.ComponentModel;
using Mysoft.Project.Core.DataAnnotations;

namespace Mysoft.Project.Core
{
    public enum DBType
    {
        SqlServer,
        SqlServerCE,
        MySql,
        PostgreSQL,
        Oracle,
        SQLite
    }
    public class Database : IDisposable
    {
        public Database(IDbConnection connection)
        {
            _sharedConnection = connection;
            _connectionString = connection.ConnectionString;
            _sharedConnectionDepth = 2;		// Prevent closing external connection
            CommonConstruct();
        }

        public Database(string connectionString, string providerName)
        {
            _connectionString = connectionString;
            _providerName = providerName;
            CommonConstruct();
        }

        public Database(string connectionString, DbProviderFactory provider)
        {
            _connectionString = connectionString;
            _factory = provider;
            CommonConstruct();
        }
        public Database(string connectionString, DbProviderFactory provider,DBType dbType)
        {
            _connectionString = connectionString;
            _factory = provider;
            _dbType = dbType;           
            SetParamPrefix();
        }
        private void SetParamPrefix(){
            EnableNamedParams = true;
            if (_dbType == DBType.MySql && _connectionString != null && _connectionString.IndexOf("Allow User Variables=true") >= 0)
                _paramPrefix = "?";
            if (_dbType == DBType.Oracle)
                _paramPrefix = ":";
        }
        DBType _dbType = DBType.SqlServer;

        // Common initialization
        private void CommonConstruct()
        {
            _transactionDepth = 0;
            EnableAutoSelect = true;
            EnableNamedParams = true;
            ForceDateTimesToUtc = true;

            if (_providerName != null)
                _factory = DbProviderFactories.GetFactory(_providerName);

            string dbtype = (_factory == null ? _sharedConnection.GetType() : _factory.GetType()).Name;

            // Try using type name first (more reliable)
            if (dbtype.StartsWith("MySql")) _dbType = DBType.MySql;
            else if (dbtype.StartsWith("SqlCe")) _dbType = DBType.SqlServerCE;
            else if (dbtype.StartsWith("Npgsql")) _dbType = DBType.PostgreSQL;
            else if (dbtype.StartsWith("Oracle")) _dbType = DBType.Oracle;
            else if (dbtype.StartsWith("SQLite")) _dbType = DBType.SQLite;
            else if (dbtype.StartsWith("System.Data.SqlClient.")) _dbType = DBType.SqlServer;
            // else try with provider name
            else if (_providerName.IndexOf("MySql", StringComparison.InvariantCultureIgnoreCase) >= 0) _dbType = DBType.MySql;
            else if (_providerName.IndexOf("SqlServerCe", StringComparison.InvariantCultureIgnoreCase) >= 0) _dbType = DBType.SqlServerCE;
            else if (_providerName.IndexOf("Npgsql", StringComparison.InvariantCultureIgnoreCase) >= 0) _dbType = DBType.PostgreSQL;
            else if (_providerName.IndexOf("Oracle", StringComparison.InvariantCultureIgnoreCase) >= 0) _dbType = DBType.Oracle;
            else if (_providerName.IndexOf("SQLite", StringComparison.InvariantCultureIgnoreCase) >= 0) _dbType = DBType.SQLite;

            SetParamPrefix();
        }

        // Automatically close one open shared connection
        public void Dispose()
        {
            // Automatically close one open connection reference
            //  (Works with KeepConnectionAlive and manually opening a shared connection)
            CloseSharedConnection();
            
        }

        // Set to true to keep the first opened connection alive until this object is disposed
        public bool KeepConnectionAlive { get; set; }

        // Open a connection (can be nested)
        public void OpenSharedConnection()
        {
            if (_sharedConnectionDepth == 0)
            {
                _sharedConnection = _factory.CreateConnection();
                _sharedConnection.ConnectionString = _connectionString;
                _sharedConnection.Open();

                _sharedConnection = OnConnectionOpened(_sharedConnection);

                if (KeepConnectionAlive)
                    _sharedConnectionDepth++;		// Make sure you call Dispose
            }
            _sharedConnectionDepth++;
        }

        // Close a previously opened connection
        public void CloseSharedConnection()
        {
            if (_sharedConnectionDepth > 0)
            {
                _sharedConnectionDepth--;
                if (_sharedConnectionDepth == 0)
                {
                    OnConnectionClosing(_sharedConnection);
                    _sharedConnection.Dispose();
                    _sharedConnection = null;
                }
            }
        }

        // Access to our shared connection
        public IDbConnection Connection
        {
            get { return _sharedConnection; }
        }

        // Start a new transaction, can be nested, every call must be
        //	matched by a call to AbortTransaction or CompleteTransaction
        // Use `using (var scope=db.Transaction) { scope.Complete(); }` to ensure correct semantics
        public Transaction BeginTransaction()
        {           
            _transactionDepth++;
            if (_transactionDepth == 1)
            {
                OpenSharedConnection();
                _transaction = _sharedConnection.BeginTransaction();
                _transactionCancelled = false;
            }
            return new Transaction(this);
        }


       
      
        // Internal helper to cleanup transaction stuff
        void CleanupTransaction()
        {           
            if (_transactionCancelled)
                _transaction.Rollback();
            else
                _transaction.Commit();

            _transaction.Dispose();
            _transaction = null;

            CloseSharedConnection();
        }

        // Abort the entire outer most transaction scope
         void AbortTransaction()
        {
            _transactionCancelled = true;
            if ((--_transactionDepth) == 0)
                CleanupTransaction();
        }

        // Complete the transaction
         void CompleteTransaction()
        {
            if ((--_transactionDepth) == 0)
                CleanupTransaction();
        }

        // Helper to handle named parameters from object properties
        static Regex rxParams = new Regex(@"(?<!@)@\w+", RegexOptions.Compiled);
        public static string ProcessParams(string _sql, object[] args_src, List<object> args_dest)
        {
            return rxParams.Replace(_sql, m =>
            {
                string param = m.Value.Substring(1);

                object arg_val;

                int paramIndex;
                if (int.TryParse(param, out paramIndex))
                {
                    // Numbered parameter
                    if (paramIndex < 0 || paramIndex >= args_src.Length)
                        throw new ArgumentOutOfRangeException(string.Format("Parameter '@{0}' specified but only {1} parameters supplied (in `{2}`)", paramIndex, args_src.Length, _sql));
                    arg_val = args_src[paramIndex];
                }
                else
                {
                    // Look for a property on one of the arguments with this name
                    bool found = false;
                    arg_val = null;
                    foreach (var o in args_src)
                    {
                        var pi = o.GetType().GetProperty(param);
                        if (pi != null)
                        {
                            arg_val = pi.GetValue(o, null);
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                        throw new ArgumentException(string.Format("Parameter '@{0}' specified but none of the passed arguments have a property with this name (in '{1}')", param, _sql));
                }

                // Expand collections to parameter lists
                if ((arg_val as System.Collections.IEnumerable) != null &&
                    (arg_val as string) == null &&
                    (arg_val as byte[]) == null)
                {
                    var sb = new StringBuilder();
                    foreach (var i in arg_val as System.Collections.IEnumerable)
                    {
                        sb.Append((sb.Length == 0 ? "@" : ",@") + args_dest.Count.ToString());
                        args_dest.Add(i);
                    }
                    return sb.ToString();
                }
                else
                {
                    args_dest.Add(arg_val);
                    return "@" + (args_dest.Count - 1).ToString();
                }
            }
            );
        }

        // Add a parameter to a DB command
        void AddParam(IDbCommand cmd, object item, string ParameterPrefix)
        {
            // Convert value to from poco type to db type
            if (Database.Mapper != null && item != null)
            {
                var fn = Database.Mapper.GetToDbConverter(item.GetType());
                if (fn != null)
                    item = fn(item);
            }

            // Support passed in parameters
            var idbParam = item as IDbDataParameter;
            if (idbParam != null)
            {
                idbParam.ParameterName = string.Format("{0}{1}", ParameterPrefix, cmd.Parameters.Count);
                cmd.Parameters.Add(idbParam);
                return;
            }

            var p = cmd.CreateParameter();
            p.ParameterName = string.Format("{0}{1}", ParameterPrefix, cmd.Parameters.Count);
            if (item == null)
            {
                p.Value = DBNull.Value;
            }
            else
            {
                var t = item.GetType();
                if (t.IsEnum)		// PostgreSQL .NET driver wont cast enum to int
                {
                    p.Value = (int)item;
                }
                else if (t == typeof(Guid))
                {
                    p.Value = item.ToString();
                    p.DbType = DbType.String;
                    p.Size = 40;
                }
                else if (t == typeof(string))
                {
                    p.Size = Math.Max((item as string).Length + 1, 4000);		// Help query plan caching by using common size
                    p.Value = item;
                }
                else if (t == typeof(AnsiString))
                {
                    // Thanks @DataChomp for pointing out the SQL Server indexing performance hit of using wrong string type on varchar
                    p.Size = Math.Max((item as AnsiString).Value.Length + 1, 4000);
                    p.Value = (item as AnsiString).Value;
                    p.DbType = DbType.AnsiString;
                }
                else if (t == typeof(bool) && _dbType != DBType.PostgreSQL)
                {
                    p.Value = ((bool)item) ? 1 : 0;
                }
                else if (item.GetType().Name == "SqlGeography") //SqlGeography is a CLR Type
                {
                    p.GetType().GetProperty("UdtTypeName").SetValue(p, "geography", null); //geography is the equivalent SQL Server Type
                    p.Value = item;
                }

                else if (item.GetType().Name == "SqlGeometry") //SqlGeometry is a CLR Type
                {
                    p.GetType().GetProperty("UdtTypeName").SetValue(p, "geometry", null); //geography is the equivalent SQL Server Type
                    p.Value = item;
                }
                else
                {
                    p.Value = item;
                }
            }

            cmd.Parameters.Add(p);
        }

        // Create a command
        static Regex rxParamsPrefix = new Regex(@"(?<!@)@\w+", RegexOptions.Compiled);
        public IDbCommand CreateCommand(IDbConnection connection, string sql, params object[] args)
        {
            // Perform named argument replacements
            if (EnableNamedParams)
            {
                var new_args = new List<object>();
                sql = ProcessParams(sql, args, new_args);
                args = new_args.ToArray();
            }

            // Perform parameter prefix replacements
            if (_paramPrefix != "@")
                sql = rxParamsPrefix.Replace(sql, m => _paramPrefix + m.Value.Substring(1));
            sql = sql.Replace("@@", "@");		   // <- double @@ escapes a single @

            // Create the command and add parameters
            IDbCommand cmd = connection.CreateCommand();
            cmd.Connection = connection;
            cmd.CommandText = sql;
            cmd.Transaction = _transaction;
            foreach (var item in args)
            {
                AddParam(cmd, item, _paramPrefix);
            }

            if (_dbType == DBType.Oracle)
            {
                cmd.GetType().GetProperty("BindByName").SetValue(cmd, true, null);
            }

            if (!String.IsNullOrEmpty(sql))
                DoPreExecute(cmd);

            return cmd;
        }

        // Override this to log/capture exceptions
        public virtual void OnException(Exception x)
        {
            System.Diagnostics.Debug.WriteLine(x.ToString());
            System.Diagnostics.Debug.WriteLine(LastCommand);
        }

        // Override this to log commands, or modify command before execution
        public virtual IDbConnection OnConnectionOpened(IDbConnection conn) { return conn; }
        public virtual void OnConnectionClosing(IDbConnection conn) { }
        public virtual void OnExecutingCommand(IDbCommand cmd) { }
        public virtual void OnExecutedCommand(IDbCommand cmd) { }

        public DataTable GetDataTable(string sql, params object[] args)
        {
            try
            {
                OpenSharedConnection();
                try
                {
                    using (var cmd = CreateCommand(_sharedConnection, sql, args))
                    {
                        DbDataAdapter da = _factory.CreateDataAdapter();
                        da.SelectCommand = cmd as DbCommand;
                        cmd.CommandTimeout = 300;
                        DataSet ds = new DataSet();
                        da.Fill(ds, "table");                      
                        OnExecutedCommand(cmd);
                        return ds.Tables[0];
                    }
                }
                finally
                {
                    CloseSharedConnection();
                }
            }
            catch (Exception x)
            {
                OnException(x);
                throw;
            }
        }


        public int Execute(string sql, params object[] args)
        {
            try
            {
                OpenSharedConnection();
                try
                {
                    using (var cmd = CreateCommand(_sharedConnection, sql, args))
                    {
                        var retv = cmd.ExecuteNonQuery();
                        OnExecutedCommand(cmd);
                        return retv;
                    }
                }
                finally
                {
                    CloseSharedConnection();
                }
            }
            catch (Exception x)
            {
                OnException(x);
                throw;
            }
        }

        public int Execute(SqlJointer sql)
        {
            return Execute(sql.SQL, sql.Arguments);
        }
       
        public T ExecuteScalar<T>(string sql, params object[] args)
        {
            try
            {
                OpenSharedConnection();
                try
                {
                    using (var cmd = CreateCommand(_sharedConnection, sql, args))
                    {
                        object val = cmd.ExecuteScalar();
                        if(val==null ||val==DBNull.Value)
                        return default(T);
                        OnExecutedCommand(cmd);
                        return val.ChangeType<T>();
                    }
                }
                finally
                {
                    CloseSharedConnection();
                }
            }
            catch (Exception x)
            {
                OnException(x);
                throw;
            }
        }

        public T ExecuteScalar<T>(SqlJointer sql) where T : struct
        {
            return ExecuteScalar<T>(sql.SQL, sql.Arguments);
        }

        Regex rxSelect = new Regex(@"\A\s*(SELECT|EXECUTE|CALL)\s", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Multiline);
        Regex rxFrom = new Regex(@"\A\s*FROM\s", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Multiline);
        string AddSelectClause<T>(string sql)
        {
            if (sql.StartsWith(";"))
                return sql.Substring(1);

            if (!rxSelect.IsMatch(sql))
            {
                var pd = PocoData.ForType(typeof(T));
                var tableName = EscapeTableName(pd.TableInfo.TableName);
                string cols = string.Join(", ", (from c in pd.QueryColumns select tableName + "." + EscapeSqlIdentifier(c)).ToArray());
                if (!rxFrom.IsMatch(sql))
                    sql = string.Format("SELECT {0} FROM {1} {2}", cols, tableName, sql);
                else
                    sql = string.Format("SELECT {0} {1}", cols, sql);
            }
            return sql;
        }

        public bool EnableAutoSelect { get; set; }
        public bool EnableNamedParams { get; set; }
        public bool ForceDateTimesToUtc { get; set; }

    
        public List<T> Fetch<T>(string sql, params object[] args)
        {
            return Query<T>(sql, args).ToList();
        }

        public List<T> Fetch<T>(SqlJointer sql)
        {
            return Fetch<T>(sql.SQL, sql.Arguments);
        }

        static Regex rxColumns = new Regex(@"\A\s*SELECT\s+((?:\((?>\((?<depth>)|\)(?<-depth>)|.?)*(?(depth)(?!))\)|.)*?)(?<!,\s+)\bFROM\b", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.Compiled);
        static Regex rxOrderBy = new Regex(@"\bORDER\s+BY\s+(?:\((?>\((?<depth>)|\)(?<-depth>)|.?)*(?(depth)(?!))\)|[\w\(\)\.])+(?:\s+(?:ASC|DESC))?(?:\s*,\s*(?:\((?>\((?<depth>)|\)(?<-depth>)|.?)*(?(depth)(?!))\)|[\w\(\)\.])+(?:\s+(?:ASC|DESC))?)*", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.Compiled);
        static Regex rxDistinct = new Regex(@"\ADISTINCT\s", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.Compiled);
        public static bool SplitSqlForPaging(string sql, out string sqlCount, out string sqlSelectRemoved, out string sqlOrderBy)
        {
            sqlSelectRemoved = null;
            sqlCount = null;
            sqlOrderBy = null;

            // Extract the columns from "SELECT <whatever> FROM"
            var m = rxColumns.Match(sql);
            if (!m.Success)
                return false;

            // Save column list and replace with COUNT(*)
            Group g = m.Groups[1];
            sqlSelectRemoved = sql.Substring(g.Index);

            if (rxDistinct.IsMatch(sqlSelectRemoved))
                sqlCount = sql.Substring(0, g.Index) + "COUNT(" + m.Groups[1].ToString().Trim() + ") " + sql.Substring(g.Index + g.Length);
            else
                sqlCount = sql.Substring(0, g.Index) + "COUNT(*) " + sql.Substring(g.Index + g.Length);


            // Look for an "ORDER BY <whatever>" clause
            m = rxOrderBy.Match(sqlCount);
            if (!m.Success)
            {
                sqlOrderBy = null;
            }
            else
            {
                g = m.Groups[0];
                sqlOrderBy = g.ToString();
                sqlCount = sqlCount.Substring(0, g.Index) + sqlCount.Substring(g.Index + g.Length);
            }

            return true;
        }

        public void BuildPageQueries<T>(long skip, long take, string sql, ref object[] args, out string sqlCount, out string sqlPage)
        {
            // Add auto select clause
            if (EnableAutoSelect)
                sql = AddSelectClause<T>(sql);

            // Split the SQL into the bits we need
            string sqlSelectRemoved, sqlOrderBy;
            if (!SplitSqlForPaging(sql, out sqlCount, out sqlSelectRemoved, out sqlOrderBy))
                throw new Exception("Unable to parse SQL statement for paged query");
            if (_dbType == DBType.Oracle && sqlSelectRemoved.StartsWith("*"))
                throw new Exception("Query must alias '*' when performing a paged query.\neg. select t.* from table t order by t.id");

            // Build the SQL for the actual final result
            if (_dbType == DBType.SqlServer || _dbType == DBType.Oracle)
            {
                sqlSelectRemoved = rxOrderBy.Replace(sqlSelectRemoved, "");
                if (rxDistinct.IsMatch(sqlSelectRemoved))
                {
                    sqlSelectRemoved = "peta_inner.* FROM (SELECT " + sqlSelectRemoved + ") peta_inner";
                }
                sqlPage = string.Format("SELECT * FROM (SELECT ROW_NUMBER() OVER ({0}) peta_rn, {1}) peta_paged WHERE peta_rn>@{2} AND peta_rn<=@{3}",
                                        sqlOrderBy == null ? "ORDER BY (SELECT NULL)" : sqlOrderBy, sqlSelectRemoved, args.Length, args.Length + 1);
                args = args.Concat(new object[] { skip, skip + take }).ToArray();
            }
            else if (_dbType == DBType.SqlServerCE)
            {
                sqlPage = string.Format("{0}\nOFFSET @{1} ROWS FETCH NEXT @{2} ROWS ONLY", sql, args.Length, args.Length + 1);
                args = args.Concat(new object[] { skip, take }).ToArray();
            }
            else
            {
                sqlPage = string.Format("{0}\nLIMIT @{1} OFFSET @{2}", sql, args.Length, args.Length + 1);
                args = args.Concat(new object[] { take, skip }).ToArray();
            }

        }

        // Fetch a page	
        public Pager<T> Page<T>(long page, long itemsPerPage, string sql, params object[] args)
        {
            string sqlCount, sqlPage;
            BuildPageQueries<T>((page - 1) * itemsPerPage, itemsPerPage, sql, ref args, out sqlCount, out sqlPage);

            // Save the one-time command time out and use it for both queries
            int saveTimeout = OneTimeCommandTimeout;

            // Setup the paged result
            var result = new Pager<T>();
            result.PageIndex = page;
            result.PageSize = itemsPerPage;
            result.TotalCount = ExecuteScalar<long>(sqlCount, args);          
            OneTimeCommandTimeout = saveTimeout;

            // Get the records
            result.Items = Fetch<T>(sqlPage, args);

            // Done
            return result;
        }

        public Pager<T> Page<T>(long page, long itemsPerPage, SqlJointer sql)
        {
            return Page<T>(page, itemsPerPage, sql.SQL, sql.Arguments);
        }


        public List<T> Fetch<T>(long page, long itemsPerPage, string sql, params object[] args)
        {
            return SkipTake<T>((page - 1) * itemsPerPage, itemsPerPage, sql, args);
        }

        public List<T> Fetch<T>(long page, long itemsPerPage, SqlJointer sql)
        {
            return SkipTake<T>((page - 1) * itemsPerPage, itemsPerPage, sql.SQL, sql.Arguments);
        }

        public List<T> SkipTake<T>(long skip, long take, string sql, params object[] args)
        {
            string sqlCount, sqlPage;
            BuildPageQueries<T>(skip, take, sql, ref args, out sqlCount, out sqlPage);
            return Fetch<T>(sqlPage, args);
        }

        public List<T> SkipTake<T>(long skip, long take, SqlJointer sql)
        {
            return SkipTake<T>(skip, take, sql.SQL, sql.Arguments);
        }

        // Return an enumerable collection of pocos
        public IEnumerable<T> Query<T>(string sql, params object[] args)
        {
            if (EnableAutoSelect)
                sql = AddSelectClause<T>(sql);

            OpenSharedConnection();
            try
            {
                using (var cmd = CreateCommand(_sharedConnection, sql, args))
                {
                    cmd.CommandTimeout = 360;
                    IDataReader r;
                    var pd = PocoData.ForType(typeof(T));
                    try
                    {
                        r = cmd.ExecuteReader();
                        OnExecutedCommand(cmd);
                    }
                    catch (Exception x)
                    {
                        OnException(x);
                        throw;
                    }
                    var factory = pd.GetFactory(cmd.CommandText, _sharedConnection.ConnectionString, ForceDateTimesToUtc, 0, r.FieldCount, r) as Func<IDataReader, T>;
                    using (r)
                    {
                        while (true)
                        {
                            T poco;
                            try
                            {
                                if (!r.Read())
                                    yield break;
                                poco = factory(r);
                            }
                            catch (Exception x)
                            {
                                OnException(x);
                                throw;
                            }

                            yield return poco;
                        }
                    }
                }
            }
            finally
            {
                CloseSharedConnection();
            }
        }

        // Multi Fetch
        public List<TRet> Fetch<T1, T2, TRet>(Func<T1, T2, TRet> cb, string sql, params object[] args) { return Query<T1, T2, TRet>(cb, sql, args).ToList(); }
        public List<TRet> Fetch<T1, T2, T3, TRet>(Func<T1, T2, T3, TRet> cb, string sql, params object[] args) { return Query<T1, T2, T3, TRet>(cb, sql, args).ToList(); }
        public List<TRet> Fetch<T1, T2, T3, T4, TRet>(Func<T1, T2, T3, T4, TRet> cb, string sql, params object[] args) { return Query<T1, T2, T3, T4, TRet>(cb, sql, args).ToList(); }

        // Multi Query
        public IEnumerable<TRet> Query<T1, T2, TRet>(Func<T1, T2, TRet> cb, string sql, params object[] args) { return Query<TRet>(new Type[] { typeof(T1), typeof(T2) }, cb, sql, args); }
        public IEnumerable<TRet> Query<T1, T2, T3, TRet>(Func<T1, T2, T3, TRet> cb, string sql, params object[] args) { return Query<TRet>(new Type[] { typeof(T1), typeof(T2), typeof(T3) }, cb, sql, args); }
        public IEnumerable<TRet> Query<T1, T2, T3, T4, TRet>(Func<T1, T2, T3, T4, TRet> cb, string sql, params object[] args) { return Query<TRet>(new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) }, cb, sql, args); }

        // Multi Fetch (SQL builder)
        public List<TRet> Fetch<T1, T2, TRet>(Func<T1, T2, TRet> cb, SqlJointer sql) { return Query<T1, T2, TRet>(cb, sql.SQL, sql.Arguments).ToList(); }
        public List<TRet> Fetch<T1, T2, T3, TRet>(Func<T1, T2, T3, TRet> cb, SqlJointer sql) { return Query<T1, T2, T3, TRet>(cb, sql.SQL, sql.Arguments).ToList(); }
        public List<TRet> Fetch<T1, T2, T3, T4, TRet>(Func<T1, T2, T3, T4, TRet> cb, SqlJointer sql) { return Query<T1, T2, T3, T4, TRet>(cb, sql.SQL, sql.Arguments).ToList(); }

        // Multi Query (SQL builder)
        public IEnumerable<TRet> Query<T1, T2, TRet>(Func<T1, T2, TRet> cb, SqlJointer sql) { return Query<TRet>(new Type[] { typeof(T1), typeof(T2) }, cb, sql.SQL, sql.Arguments); }
        public IEnumerable<TRet> Query<T1, T2, T3, TRet>(Func<T1, T2, T3, TRet> cb, SqlJointer sql) { return Query<TRet>(new Type[] { typeof(T1), typeof(T2), typeof(T3) }, cb, sql.SQL, sql.Arguments); }
        public IEnumerable<TRet> Query<T1, T2, T3, T4, TRet>(Func<T1, T2, T3, T4, TRet> cb, SqlJointer sql) { return Query<TRet>(new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) }, cb, sql.SQL, sql.Arguments); }

        // Multi Fetch (Simple)
        public List<T1> Fetch<T1, T2>(string sql, params object[] args) { return Query<T1, T2>(sql, args).ToList(); }
        public List<T1> Fetch<T1, T2, T3>(string sql, params object[] args) { return Query<T1, T2, T3>(sql, args).ToList(); }
        public List<T1> Fetch<T1, T2, T3, T4>(string sql, params object[] args) { return Query<T1, T2, T3, T4>(sql, args).ToList(); }

        // Multi Query (Simple)
        public IEnumerable<T1> Query<T1, T2>(string sql, params object[] args) { return Query<T1>(new Type[] { typeof(T1), typeof(T2) }, null, sql, args); }
        public IEnumerable<T1> Query<T1, T2, T3>(string sql, params object[] args) { return Query<T1>(new Type[] { typeof(T1), typeof(T2), typeof(T3) }, null, sql, args); }
        public IEnumerable<T1> Query<T1, T2, T3, T4>(string sql, params object[] args) { return Query<T1>(new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) }, null, sql, args); }

        // Multi Fetch (Simple) (SQL builder)
        public List<T1> Fetch<T1, T2>(SqlJointer sql) { return Query<T1, T2>(sql.SQL, sql.Arguments).ToList(); }
        public List<T1> Fetch<T1, T2, T3>(SqlJointer sql) { return Query<T1, T2, T3>(sql.SQL, sql.Arguments).ToList(); }
        public List<T1> Fetch<T1, T2, T3, T4>(SqlJointer sql) { return Query<T1, T2, T3, T4>(sql.SQL, sql.Arguments).ToList(); }

        // Multi Query (Simple) (SQL builder)
        public IEnumerable<T1> Query<T1, T2>(SqlJointer sql) { return Query<T1>(new Type[] { typeof(T1), typeof(T2) }, null, sql.SQL, sql.Arguments); }
        public IEnumerable<T1> Query<T1, T2, T3>(SqlJointer sql) { return Query<T1>(new Type[] { typeof(T1), typeof(T2), typeof(T3) }, null, sql.SQL, sql.Arguments); }
        public IEnumerable<T1> Query<T1, T2, T3, T4>(SqlJointer sql) { return Query<T1>(new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) }, null, sql.SQL, sql.Arguments); }

        // Automagically guess the property relationships between various POCOs and create a delegate that will set them up
        object GetAutoMapper(Type[] types)
        {
            // Build a key
            var kb = new StringBuilder();
            foreach (var t in types)
            {
                kb.Append(t.ToString());
                kb.Append(":");
            }
            var key = kb.ToString();

            // Check cache
            RWLock.EnterReadLock();
            try
            {
                object mapper;
                if (AutoMappers.TryGetValue(key, out mapper))
                    return mapper;
            }
            finally
            {
                RWLock.ExitReadLock();
            }

            // Create it
            RWLock.EnterWriteLock();
            try
            {
                // Try again
                object mapper;
                if (AutoMappers.TryGetValue(key, out mapper))
                    return mapper;

                // Create a method
                var m = new DynamicMethod("petapoco_automapper", types[0], types, true);
                var il = m.GetILGenerator();

                for (int i = 1; i < types.Length; i++)
                {
                    bool handled = false;
                    for (int j = i - 1; j >= 0; j--)
                    {
                        // Find the property
                        var candidates = from p in types[j].GetProperties() where p.PropertyType == types[i] select p;
                        if (candidates.Count() == 0)
                            continue;
                        if (candidates.Count() > 1)
                            throw new InvalidOperationException(string.Format("Can't auto join {0} as {1} has more than one property of type {0}", types[i], types[j]));

                        // Generate code
                        il.Emit(OpCodes.Ldarg_S, j);
                        il.Emit(OpCodes.Ldarg_S, i);
                        il.Emit(OpCodes.Callvirt, candidates.First().GetSetMethod(true));
                        handled = true;
                    }

                    if (!handled)
                        throw new InvalidOperationException(string.Format("Can't auto join {0}", types[i]));
                }

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ret);

                // Cache it
                var del = m.CreateDelegate(Expression.GetFuncType(types.Concat(types.Take(1)).ToArray()));
                AutoMappers.Add(key, del);
                return del;
            }
            finally
            {
                RWLock.ExitWriteLock();
            }
        }

        // Find the split point in a result set for two different pocos and return the poco factory for the first
        Delegate FindSplitPoint(Type typeThis, Type typeNext, string sql, IDataReader r, ref int pos)
        {
            // Last?
            if (typeNext == null)
                return PocoData.ForType(typeThis).GetFactory(sql, _sharedConnection.ConnectionString, ForceDateTimesToUtc, pos, r.FieldCount - pos, r);

            // Get PocoData for the two types
            PocoData pdThis = PocoData.ForType(typeThis);
            PocoData pdNext = PocoData.ForType(typeNext);

            // Find split point
            int firstColumn = pos;
            var usedColumns = new Dictionary<string, bool>();
            for (; pos < r.FieldCount; pos++)
            {
                // Split if field name has already been used, or if the field doesn't exist in current poco but does in the next
                string fieldName = r.GetName(pos);
                if (usedColumns.ContainsKey(fieldName) || (!pdThis.Columns.ContainsKey(fieldName) && pdNext.Columns.ContainsKey(fieldName)))
                {
                    return pdThis.GetFactory(sql, _sharedConnection.ConnectionString, ForceDateTimesToUtc, firstColumn, pos - firstColumn, r);
                }
                usedColumns.Add(fieldName, true);
            }

            throw new InvalidOperationException(string.Format("Couldn't find split point between {0} and {1}", typeThis, typeNext));
        }

        // Instance data used by the Multipoco factory delegate - essentially a list of the nested poco factories to call
        class MultiPocoFactory
        {
            public List<Delegate> m_Delegates;
            public Delegate GetItem(int index) { return m_Delegates[index]; }
        }

        // Create a multi-poco factory
        Func<IDataReader, object, TRet> CreateMultiPocoFactory<TRet>(Type[] types, string sql, IDataReader r)
        {
            var m = new DynamicMethod("petapoco_multipoco_factory", typeof(TRet), new Type[] { typeof(MultiPocoFactory), typeof(IDataReader), typeof(object) }, typeof(MultiPocoFactory));
            var il = m.GetILGenerator();

            // Load the callback
            il.Emit(OpCodes.Ldarg_2);

            // Call each delegate
            var dels = new List<Delegate>();
            int pos = 0;
            for (int i = 0; i < types.Length; i++)
            {
                // Add to list of delegates to call
                var del = FindSplitPoint(types[i], i + 1 < types.Length ? types[i + 1] : null, sql, r, ref pos);
                dels.Add(del);

                // Get the delegate
                il.Emit(OpCodes.Ldarg_0);													// callback,this
                il.Emit(OpCodes.Ldc_I4, i);													// callback,this,Index
                il.Emit(OpCodes.Callvirt, typeof(MultiPocoFactory).GetMethod("GetItem"));	// callback,Delegate
                il.Emit(OpCodes.Ldarg_1);													// callback,delegate, datareader

                // Call Invoke
                var tDelInvoke = del.GetType().GetMethod("Invoke");
                il.Emit(OpCodes.Callvirt, tDelInvoke);										// Poco left on stack
            }

            // By now we should have the callback and the N pocos all on the stack.  Call the callback and we're done
            il.Emit(OpCodes.Callvirt, Expression.GetFuncType(types.Concat(new Type[] { typeof(TRet) }).ToArray()).GetMethod("Invoke"));
            il.Emit(OpCodes.Ret);

            // Finish up
            return (Func<IDataReader, object, TRet>)m.CreateDelegate(typeof(Func<IDataReader, object, TRet>), new MultiPocoFactory() { m_Delegates = dels });
        }

        // Various cached stuff
        static Dictionary<string, object> MultiPocoFactories = new Dictionary<string, object>();
        static Dictionary<string, object> AutoMappers = new Dictionary<string, object>();
        static System.Threading.ReaderWriterLockSlim RWLock = new System.Threading.ReaderWriterLockSlim();

        // Get (or create) the multi-poco factory for a query
        Func<IDataReader, object, TRet> GetMultiPocoFactory<TRet>(Type[] types, string sql, IDataReader r)
        {
            // Build a key string  (this is crap, should address this at some point)
            var kb = new StringBuilder();
            kb.Append(typeof(TRet).ToString());
            kb.Append(":");
            foreach (var t in types)
            {
                kb.Append(":");
                kb.Append(t.ToString());
            }
            kb.Append(":"); kb.Append(_sharedConnection.ConnectionString);
            kb.Append(":"); kb.Append(ForceDateTimesToUtc);
            kb.Append(":"); kb.Append(sql);
            string key = kb.ToString().GetHashCode().ToString(); ;

            // Check cache
            RWLock.EnterReadLock();
            try
            {
                object oFactory;
                if (MultiPocoFactories.TryGetValue(key, out oFactory))
                    return (Func<IDataReader, object, TRet>)oFactory;
            }
            finally
            {
                RWLock.ExitReadLock();
            }

            // Cache it
            RWLock.EnterWriteLock();
            try
            {
                // Check again
                object oFactory;
                if (MultiPocoFactories.TryGetValue(key, out oFactory))
                    return (Func<IDataReader, object, TRet>)oFactory;
                MultiPocoFactories.Clear();
                // Create the factory
                var Factory = CreateMultiPocoFactory<TRet>(types, sql, r);

                MultiPocoFactories.Add(key, Factory);
                return Factory;
            }
            finally
            {
                RWLock.ExitWriteLock();
            }

        }

        // Actual implementation of the multi-poco query
        public IEnumerable<TRet> Query<TRet>(Type[] types, object cb, string sql, params object[] args)
        {
            OpenSharedConnection();
            try
            {
                using (var cmd = CreateCommand(_sharedConnection, sql, args))
                {
                    IDataReader r;
                    try
                    {
                        r = cmd.ExecuteReader();
                        OnExecutedCommand(cmd);
                    }
                    catch (Exception x)
                    {
                        OnException(x);
                        throw;
                    }
                    var factory = GetMultiPocoFactory<TRet>(types, sql, r);
                    if (cb == null)
                        cb = GetAutoMapper(types.ToArray());
                    bool bNeedTerminator = false;
                    using (r)
                    {
                        while (true)
                        {
                            TRet poco;
                            try
                            {
                                if (!r.Read())
                                    break;
                                poco = factory(r, cb);
                            }
                            catch (Exception x)
                            {
                                OnException(x);
                                throw;
                            }

                            if (poco != null)
                                yield return poco;
                            else
                                bNeedTerminator = true;
                        }
                        if (bNeedTerminator)
                        {
                            var poco = (TRet)(cb as Delegate).DynamicInvoke(new object[types.Length]);
                            if (poco != null)
                                yield return poco;
                            else
                                yield break;
                        }
                    }
                }
            }
            finally
            {
                CloseSharedConnection();
            }
        }


        public IEnumerable<T> Query<T>(SqlJointer sql)
        {
            return Query<T>(sql.SQL, sql.Arguments);
        }

        public bool Exists<T>(object primaryKey)
        {
            return FirstOrDefault<T>(string.Format("WHERE {0}=@0", EscapeSqlIdentifier(PocoData.ForType(typeof(T)).TableInfo.PrimaryKey)), primaryKey) != null;
        }
        public T Single<T>(object primaryKey)
        {
            return Single<T>(string.Format("WHERE {0}=@0", EscapeSqlIdentifier(PocoData.ForType(typeof(T)).TableInfo.PrimaryKey)), primaryKey);
        }
        public T SingleOrDefault<T>(object primaryKey)
        {
            return SingleOrDefault<T>(string.Format("WHERE {0}=@0", EscapeSqlIdentifier(PocoData.ForType(typeof(T)).TableInfo.PrimaryKey)), primaryKey);
        }
        public T Single<T>(string sql, params object[] args)
        {
            return Query<T>(sql, args).Single();
        }
        public T SingleOrDefault<T>(string sql, params object[] args)
        {
            return Query<T>(sql, args).SingleOrDefault();
        }
        public T First<T>(string sql, params object[] args)
        {
            return Query<T>(sql, args).First();
        }
        public T FirstOrDefault<T>(string sql, params object[] args)
        {
            return Query<T>(sql, args).FirstOrDefault();
        }

        public T Single<T>(SqlJointer sql)
        {
            return Query<T>(sql).Single();
        }
        public T SingleOrDefault<T>(SqlJointer sql)
        {
            return Query<T>(sql).SingleOrDefault();
        }
        public T First<T>(SqlJointer sql)
        {
            return Query<T>(sql).First();
        }
        public T FirstOrDefault<T>(SqlJointer sql)
        {
            return Query<T>(sql).FirstOrDefault();
        }

        public string EscapeTableName(string str)
        {
            // Assume table names with "dot" are already escaped
            return str.IndexOf('.') >= 0 ? str : EscapeSqlIdentifier(str);
        }
        public string EscapeSqlIdentifier(string str)
        {
            switch (_dbType)
            {
                case DBType.MySql:
                    return string.Format("`{0}`", str);

                case DBType.PostgreSQL:
                    return string.Format("\"{0}\"", str);

                case DBType.Oracle:
                    return string.Format("\"{0}\"", str.ToUpperInvariant());

                default:
                    return string.Format("[{0}]", str);
            }
        }


        // Insert an annotated poco object
        public object Insert(object poco)
        {
         
            return Insert(poco,null);
        }
        public object Insert( object poco,PocoData pd)
        {
            try
            {
                OpenSharedConnection();
                 pd =pd?? PocoData.ForObject(poco);
                 var opd = PocoData.ForObject(poco);
                 var primaryKeyName = pd.TableInfo.PrimaryKey;
                 var autoIncrement = false;
                 var tableName = pd.TableInfo.TableName;
                try
                {
                    using (var cmd = CreateCommand(_sharedConnection, ""))
                    {
                        
                        var names = new List<string>();
                        var values = new List<string>();
                        var index = 0;
                        foreach (var i in opd.Columns)
                        {
                            if (!pd.Columns.ContainsKey(i.Key))
                                continue;
                            if (i.Value.ResultColumn)
                                continue;
                            // Don't insert ignore columns
                            if (pd.IgnoreColumns.ContainsKey(i.Key))
                                continue;
                            // Don't insert the primary key (except under oracle where we need bring in the next sequence value)
                            if (autoIncrement && primaryKeyName != null && string.Compare(i.Key, primaryKeyName, true) == 0)
                            {
                                if (_dbType == DBType.Oracle && !string.IsNullOrEmpty(pd.TableInfo.SequenceName))
                                {
                                    names.Add(i.Key);
                                    values.Add(string.Format("{0}.nextval", pd.TableInfo.SequenceName));
                                }
                                continue;
                            }

                            names.Add(EscapeSqlIdentifier(i.Key));
                            values.Add(string.Format("{0}{1}", _paramPrefix, index++));
                            AddParam(cmd, opd.Columns[i.Key].GetValue(poco), _paramPrefix);
                        }

                        cmd.CommandText = string.Format("INSERT INTO {0} ({1}) VALUES ({2})",
                                EscapeTableName(tableName),
                                string.Join(",", names.ToArray()),
                                string.Join(",", values.ToArray())
                                );

                        if (!autoIncrement)
                        {
                            DoPreExecute(cmd);
                            cmd.ExecuteNonQuery();
                            OnExecutedCommand(cmd);
                            return true;
                        }


                        object id;
                        switch (_dbType)
                        {
                            case DBType.SqlServerCE:
                                DoPreExecute(cmd);
                                cmd.ExecuteNonQuery();
                                OnExecutedCommand(cmd);
                                id = ExecuteScalar<object>("SELECT @@@IDENTITY AS NewID;");
                                break;
                            case DBType.SqlServer:
                                cmd.CommandText += ";\nSELECT SCOPE_IDENTITY() AS NewID;";
                                DoPreExecute(cmd);
                                id = cmd.ExecuteScalar();
                                OnExecutedCommand(cmd);
                                break;
                            case DBType.PostgreSQL:
                                if (primaryKeyName != null)
                                {
                                    cmd.CommandText += string.Format("returning {0} as NewID", EscapeSqlIdentifier(primaryKeyName));
                                    DoPreExecute(cmd);
                                    id = cmd.ExecuteScalar();
                                }
                                else
                                {
                                    id = -1;
                                    DoPreExecute(cmd);
                                    cmd.ExecuteNonQuery();
                                }
                                OnExecutedCommand(cmd);
                                break;
                            case DBType.Oracle:
                                if (primaryKeyName != null)
                                {
                                    cmd.CommandText += string.Format(" returning {0} into :newid", EscapeSqlIdentifier(primaryKeyName));
                                    var param = cmd.CreateParameter();
                                    param.ParameterName = ":newid";
                                    param.Value = DBNull.Value;
                                    param.Direction = ParameterDirection.ReturnValue;
                                    param.DbType = DbType.Int64;
                                    cmd.Parameters.Add(param);
                                    DoPreExecute(cmd);
                                    cmd.ExecuteNonQuery();
                                    id = param.Value;
                                }
                                else
                                {
                                    id = -1;
                                    DoPreExecute(cmd);
                                    cmd.ExecuteNonQuery();
                                }
                                OnExecutedCommand(cmd);
                                break;
                            case DBType.SQLite:
                                if (primaryKeyName != null)
                                {
                                    cmd.CommandText += ";\nSELECT last_insert_rowid();";
                                    DoPreExecute(cmd);
                                    id = cmd.ExecuteScalar();
                                }
                                else
                                {
                                    id = -1;
                                    DoPreExecute(cmd);
                                    cmd.ExecuteNonQuery();
                                }
                                OnExecutedCommand(cmd);
                                break;
                            default:
                                cmd.CommandText += ";\nSELECT @@IDENTITY AS NewID;";
                                DoPreExecute(cmd);
                                id = cmd.ExecuteScalar();
                                OnExecutedCommand(cmd);
                                break;
                        }


                        // Assign the ID back to the primary key property
                        if (primaryKeyName != null)
                        {
                            PocoColumn pc;
                            if (opd.Columns.TryGetValue(primaryKeyName, out pc))
                            {
                                pc.SetValue(poco, pc.ChangeType(id));
                            }
                        }

                        return id;
                    }
                }
                finally
                {
                    CloseSharedConnection();
                }
            }
            catch (Exception x)
            {
                OnException(x);
                throw;
            }
        }
       

      
        public int Update(object poco) {
            return Update(poco, null);
        }
        // Update a record with values from a poco.  primary key value can be either supplied or read from the poco
        public int Update(object poco, PocoData pd)
        {
            try
            {
                pd = pd ?? PocoData.ForObject(poco);
                var opd = PocoData.ForObject(poco);
                var primaryKeyName = pd.TableInfo.PrimaryKey;

                OpenSharedConnection();
                try
                {
                    using (var cmd = CreateCommand(_sharedConnection, ""))
                    {
                        var sb = new StringBuilder();
                        var index = 0;
                        var pc = opd.Columns[primaryKeyName];
                        var primaryKeyValue = pc.GetValue(poco);
                        foreach (var i in opd.Columns)
                        {
                            if (!pd.Columns.ContainsKey(i.Key))
                                continue;
                            // Dont update Ignore  columns
                            if (pd.IgnoreColumns.ContainsKey(i.Key))
                                continue;
                            // Don't update the primary key, but grab the value if we don't have it
                            if (string.Compare(i.Key, primaryKeyName, true) == 0)
                            {
                                continue;
                            }

                            // Dont update result only columns
                            if (i.Value.ResultColumn)
                                continue;

                            // Build the sql
                            if (index > 0)
                                sb.Append(", ");
                            sb.AppendFormat("{0} = {1}{2}", EscapeSqlIdentifier(i.Key), _paramPrefix, index++);

                            // Store the parameter in the command
                            AddParam(cmd, opd.Columns[i.Key].GetValue(poco), _paramPrefix);
                        }


                        cmd.CommandText = string.Format("UPDATE {0} SET {1} WHERE {2} = {3}{4}",
                                            EscapeTableName(pd.TableInfo.TableName), sb.ToString(), EscapeSqlIdentifier(primaryKeyName), _paramPrefix, index++);
                        AddParam(cmd, primaryKeyValue, _paramPrefix);

                        DoPreExecute(cmd);

                        // Do it
                        var retv = cmd.ExecuteNonQuery();
                        OnExecutedCommand(cmd);
                        return retv;
                    }
                }
                finally
                {
                    CloseSharedConnection();
                }
            }
            catch (Exception x)
            {
                OnException(x);
                throw;
            }
        }



       
       

       
        public int Update<T>(string sql, params object[] args)
        {
            var pd = PocoData.ForType(typeof(T));
            return Execute(string.Format("UPDATE {0} {1}", EscapeTableName(pd.TableInfo.TableName), sql), args);
        }

        public int Update<T>(SqlJointer sql)
        {
            var pd = PocoData.ForType(typeof(T));
            return Execute(new SqlJointer(string.Format("UPDATE {0}", EscapeTableName(pd.TableInfo.TableName))).Append(sql));
        }

        public int Delete(string tableName, string primaryKeyName, object poco)
        {
            return Delete(tableName, primaryKeyName, poco, null);
        }

        public int Delete(string tableName, string primaryKeyName, object poco, object primaryKeyValue)
        {
            // If primary key value not specified, pick it up from the object
            if (primaryKeyValue == null)
            {
                var pd = PocoData.ForObject(poco);
                PocoColumn pc;
                if (pd.Columns.TryGetValue(primaryKeyName, out pc))
                {
                    primaryKeyValue = pc.GetValue(poco);
                }
            }

            // Do it
            var sql = string.Format("DELETE FROM {0} WHERE {1}=@0", EscapeTableName(tableName), EscapeSqlIdentifier(primaryKeyName));
            return Execute(sql, primaryKeyValue);
        }

        public int Delete(object poco)
        {
            var pd = PocoData.ForType(poco.GetType());
            return Delete(pd.TableInfo.TableName, pd.TableInfo.PrimaryKey, poco);
        }

        public int Delete<T>(object pocoOrPrimaryKey)
        {
            if (pocoOrPrimaryKey.GetType() == typeof(T))
                return Delete(pocoOrPrimaryKey);
            var pd = PocoData.ForType(typeof(T));
            return Delete(pd.TableInfo.TableName, pd.TableInfo.PrimaryKey, null, pocoOrPrimaryKey);
        }

        public int Delete<T>(string sql, params object[] args)
        {
            var pd = PocoData.ForType(typeof(T));
            return Execute(string.Format("DELETE FROM {0} {1}", EscapeTableName(pd.TableInfo.TableName), sql), args);
        }

        public int Delete<T>(SqlJointer sql)
        {
            var pd = PocoData.ForType(typeof(T));
            return Execute(new SqlJointer(string.Format("DELETE FROM {0}", EscapeTableName(pd.TableInfo.TableName))).Append(sql));
        }

        // Check if a poco represents a new record
        public bool IsNew(string primaryKeyName, object poco)
        {
            var pd = PocoData.ForObject(poco);
            object pk;
            PocoColumn pc;
            if (pd.Columns.TryGetValue(primaryKeyName, out pc))
            {
                pk = pc.GetValue(poco);
            }
//#if !PETAPOCO_NO_DYNAMIC
//            else if (poco.GetType() == typeof(System.Dynamic.ExpandoObject))
//            {
//                return true;
//            }
//#endif
            else
            {
                var pi = poco.GetType().GetProperty(primaryKeyName);
                if (pi == null)
                    throw new ArgumentException(string.Format("The object doesn't have a property matching the primary key column name '{0}'", primaryKeyName));
                pk = pi.GetValue(poco, null);
            }

            if (pk == null)
                return true;

            var type = pk.GetType();

            if (type.IsValueType)
            {
                // Common primary key types
                if (type == typeof(long))
                    return (long)pk == 0;
                else if (type == typeof(ulong))
                    return (ulong)pk == 0;
                else if (type == typeof(int))
                    return (int)pk == 0;
                else if (type == typeof(uint))
                    return (uint)pk == 0;

                // Create a default instance and compare
                return pk == Activator.CreateInstance(pk.GetType());
            }
            else
            {
                return pk == null;
            }
        }

        public bool IsNew(object poco)
        {
            var pd = PocoData.ForType(poco.GetType());
            if (!pd.TableInfo.AutoIncrement)
                throw new InvalidOperationException("IsNew() and Save() are only supported on tables with auto-increment/identity primary key columns");
            return IsNew(pd.TableInfo.PrimaryKey, poco);
        }

        //// Insert new record or Update existing record
        //public void Save(string tableName, string primaryKeyName, object poco)
        //{
        //    if (IsNew(primaryKeyName, poco))
        //    {
        //        Insert(tableName, primaryKeyName, true, poco);
        //    }
        //    else
        //    {
        //        Update(tableName, primaryKeyName, poco);
        //    }
        //}

        //public void Save(object poco)
        //{
        //    var pd = PocoData.ForType(poco.GetType());
        //    Save(pd.TableInfo.TableName, pd.TableInfo.PrimaryKey, poco);
        //}

        public int CommandTimeout { get; set; }
        public int OneTimeCommandTimeout { get; set; }

        void DoPreExecute(IDbCommand cmd)
        {
            // Setup command timeout
            if (OneTimeCommandTimeout != 0)
            {
                cmd.CommandTimeout = OneTimeCommandTimeout;
                OneTimeCommandTimeout = 0;
            }
            else if (CommandTimeout != 0)
            {
                cmd.CommandTimeout = CommandTimeout;
            }

            // Call hook
            OnExecutingCommand(cmd);

            // Save it
            _lastSql = cmd.CommandText;
            _lastArgs = (from IDataParameter parameter in cmd.Parameters select parameter.Value).ToArray();
        }

        public string LastSQL { get { return _lastSql; } }
        public object[] LastArgs { get { return _lastArgs; } }
        public string LastCommand
        {
            get { return FormatCommand(_lastSql, _lastArgs); }
        }

        public string FormatCommand(IDbCommand cmd)
        {
            return FormatCommand(cmd.CommandText, (from IDataParameter parameter in cmd.Parameters select parameter.Value).ToArray());
        }

        public string FormatCommand(string sql, object[] args)
        {
            var sb = new StringBuilder();
            if (sql == null)
                return "";
            sb.Append(sql);
            if (args != null && args.Length > 0)
            {
                sb.Append("\n");
                for (int i = 0; i < args.Length; i++)
                {
                    sb.AppendFormat("\t -> {0}{1} [{2}] = \"{3}\"\n", _paramPrefix, i, args[i].GetType().Name, args[i]);
                }
                sb.Remove(sb.Length - 1, 1);
            }
            return sb.ToString();
        }


        public static IMapper Mapper
        {
            get;
            set;
        }

      
     


        // Member variables
        string _connectionString;
        string _providerName;
        DbProviderFactory _factory;
        IDbConnection _sharedConnection;
        IDbTransaction _transaction;
        int _sharedConnectionDepth;
        int _transactionDepth;
        bool _transactionCancelled;
        string _lastSql;
        object[] _lastArgs;
        string _paramPrefix = "@";
    }
  

}
