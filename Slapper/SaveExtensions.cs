using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slapper.Reflection;
using System.IO;
using Slapper.Attributes;

namespace Slapper
{
	public static class SaveExtensions
	{
		static ConcurrentDictionary<Type, object> EntityMapCache = new ConcurrentDictionary<Type, object>();

		static EntityMapper.Map<T> FindOrCreateMap<T>()
		{
			return (EntityMapper.Map<T>)EntityMapCache.GetOrAdd(typeof(T), (t) => EntityMapper.CreateMap<T>());
		}

		public static int Update<T>(this IDbConnection conn, T obj)
		{
			var map = FindOrCreateMap<T>();
			var fields = map.FieldReader(obj);
			var args = new List<object>();

			using (var sql = new StringWriter())
			{
				sql.WriteLine("update [{0}]", map.Table);
				sql.Write("set");

				bool first = true;
				foreach (var f in fields.Where(x => (x.Flags & FieldFlags.ReadOnly) == 0 && x.Modified))
				{
					sql.Write("{0}[{1}]=@p{2}", first ? " " : ", ", f.Name, args.Count);
					args.Add(f.Value);
					first = false;
				}
				sql.WriteLine();

				if (args.Count == 0) // nothing changed
					return 0;

				sql.Write("where");
				first = true;
				foreach (var f in fields.Where(x => (x.Flags & FieldFlags.Key) > 0))
				{
					sql.Write("{0}[{1}]=@p{2}", first ? " " : " and ", f.Name, args.Count);
					args.Add(f.Value);
					first = false;
				}

				return conn.ExecuteNonQuery(sql.ToString(), args);
			}
		}
	}
}
