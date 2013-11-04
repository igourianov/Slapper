using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using Slapper.Reflection;

namespace Slapper
{
	public static class IDbConnectionExtensions
	{
		public static IDbCommand CreateCommand(this IDbConnection conn, string sql, IDbTransaction transaction = null)
		{
			var cmd = conn.CreateCommand();
			cmd.Connection = conn;
			cmd.Transaction = transaction;
			cmd.CommandText = sql;
			return cmd;
		}

		public static int ExecuteNonQuery(this IDbConnection conn, string sql, object args = null)
		{
			using (var cmd = conn.CreateCommand(sql))
				return cmd.ExecuteNonQuery(args);
		}

		public static T ExecuteScalar<T>(this IDbConnection conn, string sql, object args = null)
		{
			using (var cmd = conn.CreateCommand(sql))
				return cmd.ExecuteScalar<T>(args);
		}

		public static IDataReader ExecuteReader(this IDbConnection conn, string sql, object args = null, CommandBehavior behavior = CommandBehavior.SingleResult)
		{
			using (var cmd = conn.CreateCommand(sql))
				return cmd.ExecuteReader(args, behavior);
		}
	}
}
