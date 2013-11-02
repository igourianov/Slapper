using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

		static IDbCommand CreateCommand(this IDbConnection conn, string sql, object args = null)
		{
			var cmd = conn.CreateCommand();
			cmd.Connection = conn; // just in case

			if (args != null)
			{
				foreach (var m in ParameterMapper.Read(args).Where(x => sql.Contains("@" + x.Name)))
				{
					if (m.Value is IEnumerable && !m.Value.GetType().IsScalar())
					{
						var values = ((IEnumerable)m.Value).Cast<Object>()
							.Select((x, i) => cmd.CreateParameter(String.Format("@{0}_{1}", m.Name, i), x))
							.ToList();

						sql = sql.Replace("@" + m.Name, String.Join(",", values.Select(x => x.ParameterName)));

						foreach (var val in values)
							cmd.Parameters.Add(val);
					}
					else
						cmd.Parameters.Add(cmd.CreateParameter(m.Name, m.Value));
				}
			}

			cmd.CommandText = sql;
			return cmd;
		}

		static IDbDataParameter CreateParameter(this IDbCommand cmd, string name, object value, DbType? type = null)
		{
			var p = cmd.CreateParameter();
			p.ParameterName = name;
			p.Value = value ?? DBNull.Value;
			if (type != null)
				p.DbType = type.Value;
			return p;
		}
	}
}
