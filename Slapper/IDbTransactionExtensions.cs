using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using Slapper.Reflection;

namespace Slapper
{
	public static class IDbTransactionExtensions
	{
		public static int ExecuteNonQuery(this IDbTransaction transaction, string sql, object args = null)
		{
			using (var cmd = transaction.Connection.CreateCommand(sql, transaction))
				return cmd.ExecuteNonQuery(args);
		}

		public static T ExecuteScalar<T>(this IDbTransaction transaction, string sql, object args = null)
		{
			using (var cmd = transaction.Connection.CreateCommand(sql, transaction))
				return cmd.ExecuteScalar<T>(args);
		}

		public static IDataReader ExecuteReader(this IDbTransaction transaction, string sql, object args = null, CommandBehavior behavior = CommandBehavior.SingleResult)
		{
			using (var cmd = transaction.Connection.CreateCommand(sql, transaction))
				return cmd.ExecuteReader(args, behavior);
		}
	}
}
