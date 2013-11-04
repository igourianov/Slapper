using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Slapper.Attributes;

namespace Slapper.Reflection
{
	public static class DataReaderMapper
	{
		static Dictionary<string, MethodInfo> TypedGetters;
		static MethodInfo IsDBNullGetter;
		static MethodInfo ObjectValueGetter;

		static BindingFlags MemberFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
		static ConcurrentDictionary<string, object> ObjectMapCache = new ConcurrentDictionary<string, object>();
		static ConcurrentDictionary<string, object> ValueMapCache = new ConcurrentDictionary<string, object>();
		static DataRow[] EmptyDataRows = new DataRow[0];

		static DataReaderMapper()
		{
			var getters = typeof(IDataRecord).GetMethods(BindingFlags.Public | BindingFlags.Instance)
				.Where(x => {
					var prms = x.GetParameters();
					return prms.Length == 1 && prms[0].ParameterType == typeof(int);
				})
				.ToList();

			IsDBNullGetter = getters.First(x => x.Name == "IsDBNull");
			ObjectValueGetter = getters.FirstOrDefault(x => x.Name == "GetValue");
			TypedGetters = getters.Where(x => x.Name.StartsWith("Get"))
				.ToDictionary(x => x.Name == "GetFloat" ? "GetSingle" : x.Name);
		}

		public static Func<IDataRecord, T> CreateMapper<T>(string sql, IDataReader reader)
		{
			var t = typeof(T);
			var key = t.FullName + ":" + sql;

			if (t.IsDbPrimitive())
				return (Func<IDataRecord, T>)ValueMapCache.GetOrAdd(key, (s) => CreateValueMapper<T>(reader));
			return (Func<IDataRecord, T>)ObjectMapCache.GetOrAdd(key, (s) => CreateObjectMapper<T>(reader));
		}

		public static Func<IDataRecord, T> CreateValueMapper<T>(IDataReader reader)
		{
			var record = Expression.Parameter(typeof(IDataRecord), "record");
			return Expression.Lambda<Func<IDataRecord, T>>(GetColumnValue(record, GetColumns(reader, false).First(), typeof(T)), record).Compile();
		}

		public static Func<IDataRecord, T> CreateObjectMapper<T>(IDataReader reader)
		{
			var t = typeof(T);
			var attr = t.GetCustomAttributes<SlapperEntityAttribute>().FirstOrDefault();
			var tableName = (attr != null && attr.Table != null ? attr.Table : t.Name).ToLower();
			var schema = GetColumns(reader, false).OrderBy(x => x.Table != tableName).ToList();
			var members = GetMembers(t).ToList();

			var record = Expression.Parameter(typeof(IDataRecord), "record");
			var init = new List<MemberBinding>();

			foreach (var m in members)
			{
				var col = schema.FirstOrDefault(x => x.Name == m.Name);
				if (col != null)
				{
					try
					{
						init.Add(Expression.Bind(m.Info, GetColumnValue(record, col, m.Type)));
					}
					catch (Exception e)
					{
						throw new MemberMappingException(t, m.Info, e);
					}
				}
			}

			var block = Expression.Block(t, Expression.MemberInit(Expression.New(t), init));
			return Expression.Lambda<Func<IDataRecord, T>>(block, record).Compile();
		}

		static Expression GetColumnValue(Expression record, RecordColumn col, Type expectedType)
		{
			var typeName = (col.Type.IsNullable() ? col.Type.GetGenericArguments()[0] : col.Type).Name;
			var idx = Expression.Constant(col.Index);

			MethodInfo getter;
			if (!TypedGetters.TryGetValue(typeName, out getter))
				getter = ObjectValueGetter;

			Expression expr = Expression.Call(record, getter, idx);
			if (expr.Type == typeof(Object))
				expr = Expression.Unbox(expr, col.Type);
			if (expr.Type != expectedType)
				expr = Expression.Convert(expr, expectedType);
			if (!expectedType.IsValueType || expectedType.IsNullable())
				expr = Expression.Condition(Expression.Call(record, IsDBNullGetter, idx), Expression.Default(expr.Type), expr);

			return expr;
		}

		static IEnumerable<RecordColumn> GetColumns(IDataReader reader, bool tableInfo)
		{
			var tableMap = (!tableInfo ? EmptyDataRows : reader.GetSchemaTable().Rows.Cast<DataRow>())
				.Select(x => new { Index = x.Field<int>("ColumnOrdinal"), Table = x.Field<string>("BaseTableName") })
				.ToList();

			for (int i = 0; i < reader.FieldCount; i++)
			{
				yield return new RecordColumn {
					Index = i,
					Name = reader.GetName(i).ToLower(),
					Type = reader.GetFieldType(i),
					Table = !tableInfo ? null : (tableMap.Where(x => x.Index == i).Select(x => x.Table).FirstOrDefault() ?? "").ToLower(),
				};
			}
		}

		private class RecordColumn
		{
			public int Index;
			public string Name;
			public string Table;
			public Type Type;
		}

		static IEnumerable<ObjectMember> GetMembers(Type t)
		{
			foreach (var p in t.GetProperties(MemberFlags)
				.Where(x => x.CanWrite && x.GetIndexParameters().Length == 0)
				.Where(x => x.GetCustomAttribute<SlapperIgnoreAttribute>() == null))
			{
				var attr = p.GetCustomAttribute<SlapperFieldAttribute>();
				yield return new ObjectMember {
					Name = (attr != null && attr.Name != null ? attr.Name : p.Name).ToLower(),
					Type = p.PropertyType,
					Info = p,
				};
			}

			foreach (var f in t.GetFields(MemberFlags)
				.Where(x => !x.IsInitOnly)
				.Where(x => x.GetCustomAttribute<SlapperIgnoreAttribute>() == null))
			{
				var attr = f.GetCustomAttribute<SlapperFieldAttribute>();
				yield return new ObjectMember {
					Name = (attr != null && attr.Name != null ? attr.Name : f.Name).ToLower(),
					Type = f.FieldType,
					Info = f,
				};
			}
		}

		private class ObjectMember
		{
			public string Name;
			public Type Type;
			public MemberInfo Info;
		}


		public class MemberMappingException : Exception
		{
			public MemberMappingException(Type obj, MemberInfo member, Exception e)
				: base(String.Format("Error mapping {0}.{1}", obj.FullName, member.Name), e)
			{
			}
		}
	}
}
