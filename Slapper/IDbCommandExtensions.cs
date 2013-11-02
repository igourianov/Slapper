using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using Slapper.Reflection;

namespace Slapper
{
	public static class IDbCommandExtensions
	{
		public static int ExecuteNonQuery(this IDbCommand cmd, object args = null)
		{
			SetParams(cmd, args);
			return cmd.ExecuteNonQuery();
		}

		public static T ExecuteScalar<T>(this IDbCommand cmd, object args = null)
		{
			SetParams(cmd, args);
			var ret = cmd.ExecuteScalar();
			if (ret is DBNull)
				ret = null;
			return (T)ret;
		}

		public static IDataReader ExecuteReader(this IDbCommand cmd, object args = null, CommandBehavior behavior = CommandBehavior.SingleResult)
		{
			SetParams(cmd, args);
			return cmd.ExecuteReader(behavior);
		}

		static void SetParams(IDbCommand cmd, object args)
		{
			cmd.Parameters.Clear();
			if (args == null)
				return;

			foreach (var p in ParameterMapper.Read(args))
				cmd.Parameters.Add(CreateParameter(cmd, p.Name, p.Value));
		}

		static IDbDataParameter CreateParameter(IDbCommand cmd, string name, object value, DbType? type = null)
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
