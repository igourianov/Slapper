using System;
using System.Data;
using Slapper.Reflection;

namespace Slapper
{
	public static class SqlExtensions
	{
		public static int ExecuteNonQuery(this IDbConnection conn, string sql, object args = null)
		{
			using (var cmd = conn.CreateCommand(sql, args))
				return cmd.ExecuteNonQuery();
		}

		public static T ExecuteScalar<T>(this IDbConnection conn, string sql, object args = null)
		{
			using (var cmd = conn.CreateCommand(sql, args))
			{
				var ret = cmd.ExecuteScalar();
				return ret is DBNull ? default(T) : (T)ret;
			}
		}

		public static IDataReader ExecuteReader(this IDbConnection conn, string sql, object args = null, CommandBehavior behavior = CommandBehavior.SingleResult)
		{
			using (var cmd = conn.CreateCommand(sql, args))
				return cmd.ExecuteReader(behavior);
		}

		public static IDbCommand CreateCommand(this IDbConnection conn, string sql, object args = null, bool checkSql = true)
		{
			var cmd = conn.CreateCommand();
			cmd.CommandText = sql;
			cmd.Connection = conn; // just in case

			if (args != null)
			{
				foreach (var m in ParameterReader.Read(args))
				{
					var p = m.Value as IDbDataParameter ?? cmd.CreateParameter(m.Key, m.Value);
					if (!checkSql || sql.Contains(p.ParameterName))
						cmd.Parameters.Add(p);
				}
			}

			return cmd;
		}

		public static IDbDataParameter CreateParameter(this IDbCommand cmd, string name, object value, DbType? type = null)
		{
			var p = cmd.CreateParameter();
			p.ParameterName = name.StartsWith("@") ? name : "@" + name;
			p.Value = value ?? DBNull.Value;
			if (type != null)
				p.DbType = type.Value;
			return p;
		}

		public static IDbDataParameter CreateParameter(this IDbConnection conn, string name, object value, DbType? type = null)
		{
			using (var cmd = conn.CreateCommand())
				return cmd.CreateParameter(name, value, type);
		}
	}
}
