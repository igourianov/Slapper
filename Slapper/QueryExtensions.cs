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
		static ConcurrentDictionary<string, object> ObjectMapCache = new ConcurrentDictionary<string, object>();
		static ConcurrentDictionary<string, object> ValueMapCache = new ConcurrentDictionary<string, object>();

		static Func<IDataRecord, T> FindOrCreateMap<T>(string sql, IDataReader reader)
		{
			var t = typeof(T);
			var key = t.FullName + ":" + sql;
			
			if (t.IsValueType || t == typeof(String) || t == typeof(byte[]))
				return (Func<IDataRecord, T>)ValueMapCache.GetOrAdd(key, (s) => DataReaderMapper.CreateValueMapper<T>(reader));
			return (Func<IDataRecord, T>)ObjectMapCache.GetOrAdd(key, (s) => DataReaderMapper.CreateObjectMapper<T>(reader));
		}

		public static IEnumerable<T> Query<T>(this IDbConnection conn, string sql, object args = null)
		{
			using (var reader = conn.ExecuteReader(sql, args))
			{
				var map = FindOrCreateMap<T>(sql, reader);
				while (reader.Read())
					yield return map(reader);
			}
		}

		public static IEnumerable<Tuple<T1,T2>> Query<T1, T2>(this IDbConnection conn, string sql, object args = null)
			where T1 : class, new()
			where T2 : class, new()
		{
			using (var reader = conn.ExecuteReader(sql, args, CommandBehavior.SingleResult | CommandBehavior.KeyInfo))
			{
				var map1 = FindOrCreateMap<T1>(sql, reader);
				var map2 = FindOrCreateMap<T2>(sql, reader);
				while (reader.Read())
					yield return Tuple.Create(map1(reader), map2(reader));
			}
		}

		public static IEnumerable<Tuple<T1, T2, T3>> Query<T1, T2, T3>(this IDbConnection conn, string sql, object args = null)
			where T1 : class, new()
			where T2 : class, new()
			where T3 : class, new()
		{
			using (var reader = conn.ExecuteReader(sql, args, CommandBehavior.SingleResult | CommandBehavior.KeyInfo))
			{
				var map1 = FindOrCreateMap<T1>(sql, reader);
				var map2 = FindOrCreateMap<T2>(sql, reader);
				var map3 = FindOrCreateMap<T3>(sql, reader);
				while (reader.Read())
					yield return Tuple.Create(map1(reader), map2(reader), map3(reader));
			}
		}
	}
}

