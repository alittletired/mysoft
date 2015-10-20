using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mysoft.Project.Core
{
    // Simple helper class for building SQL statments
    public class SqlJointer
    {
        public SqlJointer()
        {
        }

        public SqlJointer(string sql, params object[] args)
        {
            _sql = sql;
            _args = args;
        }

        public static SqlJointer Builder
        {
            get { return new SqlJointer(); }
        }

        string _sql;
        object[] _args;
        SqlJointer _rhs;
        string _sqlFinal;
        object[] _argsFinal;

        private void Build()
        {
            // already built?
            if (_sqlFinal != null)
                return;

            // Build it
            var sb = new StringBuilder();
            var args = new List<object>();
            Build(sb, args, null);
            _sqlFinal = sb.ToString();
            _argsFinal = args.ToArray();
        }

        public string SQL
        {
            get
            {
                Build();
                return _sqlFinal;
            }
        }

        public object[] Arguments
        {
            get
            {
                Build();
                return _argsFinal;
            }
        }

        public SqlJointer Append(SqlJointer sql)
        {
            if (_rhs != null)
                _rhs.Append(sql);
            else
                _rhs = sql;

            return this;
        }

        public SqlJointer Append(string sql, params object[] args)
        {
            return Append(new SqlJointer(sql, args));
        }

        static bool Is(SqlJointer sql, string sqltype)
        {
            return sql != null && sql._sql != null && sql._sql.StartsWith(sqltype, StringComparison.InvariantCultureIgnoreCase);
        }

        private void Build(StringBuilder sb, List<object> args, SqlJointer lhs)
        {
            if (!String.IsNullOrEmpty(_sql))
            {
                // Add SQL to the string
                if (sb.Length > 0)
                {
                    sb.Append("\n");
                }

                var sql = Database.ProcessParams(_sql, _args, args);

                if (Is(lhs, "WHERE ") && Is(this, "WHERE "))
                    sql = "AND " + sql.Substring(6);
                if (Is(lhs, "ORDER BY ") && Is(this, "ORDER BY "))
                    sql = ", " + sql.Substring(9);

                sb.Append(sql);
            }

            // Now do rhs
            if (_rhs != null)
                _rhs.Build(sb, args, this);
        }

        public SqlJointer Where(string sql, params object[] args)
        {
            return Append(new SqlJointer("WHERE (" + sql + ")", args));
        }

        public SqlJointer OrderBy(params object[] columns)
        {
            return Append(new SqlJointer("ORDER BY " + String.Join(", ", (from x in columns select x.ToString()).ToArray())));
        }

        public SqlJointer Select(params object[] columns)
        {
            return Append(new SqlJointer("SELECT " + String.Join(", ", (from x in columns select x.ToString()).ToArray())));
        }

        public SqlJointer From(params object[] tables)
        {
            return Append(new SqlJointer("FROM " + String.Join(", ", (from x in tables select x.ToString()).ToArray())));
        }

        public SqlJointer GroupBy(params object[] columns)
        {
            return Append(new SqlJointer("GROUP BY " + String.Join(", ", (from x in columns select x.ToString()).ToArray())));
        }

        private SqlJoinClause Join(string JoinType, string table)
        {
            return new SqlJoinClause(Append(new SqlJointer(JoinType + table)));
        }

        public SqlJoinClause InnerJoin(string table) { return Join("INNER JOIN ", table); }
        public SqlJoinClause LeftJoin(string table) { return Join("LEFT JOIN ", table); }

        public class SqlJoinClause
        {
            private readonly SqlJointer _sql;

            public SqlJoinClause(SqlJointer sql)
            {
                _sql = sql;
            }

            public SqlJointer On(string onClause, params object[] args)
            {
                return _sql.Append("ON " + onClause, args);
            }
        }
    }
}
