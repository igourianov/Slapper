using Slapper.Reflection;
using System.Collections.Generic;
using System.Data;

namespace Slapper
{
	public static class QueryExtensions
	{
		public static IEnumerable<T> Query<T>(this IDbConnection connection, string sql, object parameters = null)
		{
			return Query<T>(connection, null, sql, parameters);
		}

		public static IEnumerable<T> Query<T>(IDbTransaction transaction, string sql, object parameters = null)
		{
			return Query<T>(transaction.Connection, transaction, sql, parameters);
		}

		static IEnumerable<T> Query<T>(IDbConnection connection, IDbTransaction transaction, string sql, object parameters = null)
		{
			var behavior = CommandBehavior.SingleResult;
			if (typeof(T).IsTuple())
				behavior |= CommandBehavior.KeyInfo;


			using (var command = connection.CreateCommand(sql, transaction))
			using (var reader = command.ExecuteReader(parameters, behavior))
			{
				var map = DataReaderMapper.GetMapper<T>(sql, reader);
				while (reader.Read())
					yield return map(reader);
			}
		}
	}
}

