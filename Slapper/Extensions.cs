using System;
using System.Linq;
using System.Data;
using Slapper.Reflection;
using System.Collections.Generic;

namespace Slapper
{
	public static class Extensions
	{
		#region IDbCommand

		public static int ExecuteNonQuery(this IDbCommand command, object parameters)
		{
			SetParams(command, parameters);
			return command.ExecuteNonQuery();
		}

		public static T ExecuteScalar<T>(this IDbCommand command)
		{
			var ret = command.ExecuteScalar();
			if (ret is DBNull)
				ret = null;
			return (T)ret;
		}

		public static T ExecuteScalar<T>(this IDbCommand command, object parameters)
		{
			SetParams(command, parameters);
			return command.ExecuteScalar<T>();
		}

		public static IDataReader ExecuteReader(this IDbCommand command, object parameters, CommandBehavior behavior = CommandBehavior.SingleResult)
		{
			SetParams(command, parameters);
			return command.ExecuteReader(behavior);
		}

		public static IEnumerable<T> Query<T>(this IDbCommand command)
		{
			using (var reader = command.ExecuteReader(CommandBehavior.SingleResult))
			{
				var map = DataReaderMapper.GetMapper<T>(command.CommandText, reader);
				while (reader.Read())
					yield return map(reader);
			}
		}

		public static IEnumerable<T> Query<T>(this IDbCommand command, object parameters)
		{
			SetParams(command, parameters);
			return command.Query<T>();
		}

		static void SetParams(IDbCommand command, object parameters)
		{
			command.Parameters.Clear();

			if (parameters == null)
				return;

			foreach (var p in ParameterMapper.Read(parameters).Where(x => command.CommandText.Contains("@" + x.Name)))
				command.Parameters.Add(CreateParameter(command, p.Name, p.Value));
		}

		static IDbDataParameter CreateParameter(IDbCommand command, string name, object value)
		{
			var p = command.CreateParameter();
			p.ParameterName = name;
			p.Value = value ?? DBNull.Value;
			return p;
		}

		#endregion

		#region IDbConnection

		public static IDbCommand CreateCommand(this IDbConnection connection, string sql, IDbTransaction transaction = null)
		{
			var cmd = connection.CreateCommand();
			cmd.Connection = connection;
			cmd.Transaction = transaction;
			cmd.CommandText = sql;
			return cmd;
		}

		public static int ExecuteNonQuery(this IDbConnection connection, string sql, object parameters = null)
		{
			using (var cmd = connection.CreateCommand(sql))
				return cmd.ExecuteNonQuery(parameters);
		}

		public static T ExecuteScalar<T>(this IDbConnection connection, string sql, object parameters = null)
		{
			using (var cmd = connection.CreateCommand(sql))
				return cmd.ExecuteScalar<T>(parameters);
		}

		public static IDataReader ExecuteReader(this IDbConnection connection, string sql, object parameters = null, CommandBehavior behavior = CommandBehavior.SingleResult)
		{
			using (var cmd = connection.CreateCommand(sql))
				return cmd.ExecuteReader(parameters, behavior);
		}

		public static IEnumerable<T> Query<T>(this IDbConnection connection, string sql, object parameters = null)
		{
			using (var cmd = connection.CreateCommand(sql))
				return cmd.Query<T>(parameters);
		}

		#endregion

		#region IDbTransaction

		public static int ExecuteNonQuery(this IDbTransaction transaction, string sql, object parameters = null)
		{
			using (var cmd = transaction.Connection.CreateCommand(sql, transaction))
				return cmd.ExecuteNonQuery(parameters);
		}

		public static T ExecuteScalar<T>(this IDbTransaction transaction, string sql, object parameters = null)
		{
			using (var cmd = transaction.Connection.CreateCommand(sql, transaction))
				return cmd.ExecuteScalar<T>(parameters);
		}

		public static IDataReader ExecuteReader(this IDbTransaction transaction, string sql, object parameters = null, CommandBehavior behavior = CommandBehavior.SingleResult)
		{
			using (var cmd = transaction.Connection.CreateCommand(sql, transaction))
				return cmd.ExecuteReader(parameters, behavior);
		}

		public static IEnumerable<T> Query<T>(this IDbTransaction transaction, string sql, object parameters = null)
		{
			using (var cmd = transaction.Connection.CreateCommand(sql, transaction))
				return cmd.Query<T>(parameters);
		}

		#endregion
	}
}
