using Slapper.Reflection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slapper
{
	public static class QueryExtensions
	{
		public static IEnumerable<T> Query<T>(this IDbConnection conn, string sql, object args = null)
		{
			var behavior = CommandBehavior.SingleResult;
			if (typeof(T).IsTuple())
				behavior |= CommandBehavior.KeyInfo;

			using (var reader = conn.ExecuteReader(sql, args, behavior))
			{
				var map = DataReaderMapper.GetMapper<T>(sql, reader);
				while (reader.Read())
					yield return map(reader);
			}
		}
	}
}

