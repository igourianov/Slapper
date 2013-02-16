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

		static Func<IDataRecord, T> GetOrCreateObjectMap<T>(string key, IDataReader reader)
		{
			var t = typeof(T);
			if (t.IsValueType || t == typeof(String))
				return (Func<IDataRecord, T>)ValueMapCache.GetOrAdd(key, (s) => DataReaderMapper.CreateValueMapper<T>(reader));
			return (Func<IDataRecord, T>)ObjectMapCache.GetOrAdd(key, (s) => DataReaderMapper.CreateObjectMapper<T>(reader));
		}

		public static IEnumerable<T> Query<T>(this IDbConnection conn, string sql, object args = null)
			where T : new()
		{
			using (var reader = conn.ExecuteReader(sql, args))
			{
				var map = GetOrCreateObjectMap<T>(sql, reader);
				while (reader.Read())
					yield return map(reader);
			}
		}
	}
}

