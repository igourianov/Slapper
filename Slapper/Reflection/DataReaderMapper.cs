﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Slapper.Attributes;
using System.Runtime.Caching;

namespace Slapper.Reflection
{
	public static class DataReaderMapper
	{
		static Dictionary<string, MethodInfo> TypedGetters;
		static MethodInfo IsDBNullGetter;
		static MethodInfo ObjectValueGetter;

		static CacheItemPolicy CachePolicy = new CacheItemPolicy { SlidingExpiration = new TimeSpan(0, 5, 0) };
		static ObjectCache MapCache = new MemoryCache("SlapperDataReaderMapCache");

		static BindingFlags MemberFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
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

		public static Func<IDataRecord, T> GetMapper<T>(string sql, IDataReader reader)
		{
			var t = typeof(T);
			var key = t.FullName + ":" + sql;

			if (t.IsTuple())
				return (Func<IDataRecord, T>)GetOrAdd(key, (s) => CreateTupleMapper<T>(reader));
			if (t.IsDbPrimitive())
				return (Func<IDataRecord, T>)GetOrAdd(key, (s) => CreateValueMapper<T>(reader));
			return (Func<IDataRecord, T>)GetOrAdd(key, (s) => CreateObjectMapper<T>(reader));
		}

		static object GetOrAdd(string key, Func<string, object> factory)
		{
			var ret = MapCache.Get(key);
			if (ret == null)
			{
				ret = factory(key);
				MapCache.Set(key, ret, CachePolicy);
			}
			return ret;
		}

		static Func<IDataRecord, T> CreateTupleMapper<T>(IDataReader reader)
		{
			var record = Expression.Parameter(typeof(IDataRecord), "record");
			var columns = GetColumns(reader).ToList();
			var types = typeof(T).GetGenericArguments();
			var values = new List<Expression>(types.Length);

			int index = 0;
			foreach (var t in types)
			{
				if (t.IsDbPrimitive())
				{
					values.Add(MapColumn(record, index, columns[index].Type, t));
					index++;
				}
				else
				{
					var table = GetTableName(t);
					values.Add(MapObject(t, record, columns, ref index));
				}
			}

			return Expression.Lambda<Func<IDataRecord, T>>(Expression.New(typeof(T).GetConstructor(types), values), record).Compile();
		}

		static Func<IDataRecord, T> CreateValueMapper<T>(IDataReader reader)
		{
			var record = Expression.Parameter(typeof(IDataRecord), "record");
			var col = GetColumns(reader).First();
			return Expression.Lambda<Func<IDataRecord, T>>(MapColumn(record, col.Index, col.Type, typeof(T)), record).Compile();
		}

		static Func<IDataRecord, T> CreateObjectMapper<T>(IDataReader reader)
		{
			int index = 0;
			var t = typeof(T);
			var record = Expression.Parameter(typeof(IDataRecord), "record");
			var columns = GetColumns(reader).ToList();
			return Expression.Lambda<Func<IDataRecord, T>>(MapObject(t, record, columns, ref index), record).Compile();
		}

		static Expression MapObject(Type t, Expression record, IEnumerable<RecordColumn> columns, ref int index)
		{
			var table = GetTableName(t);
			var members = GetMembers(t).ToList();
			var init = new List<MemberBinding>(members.Count);
			columns = columns.Skip(index);

			foreach (var m in members)
			{
				var col = columns.FirstOrDefault(x => x.Name == m.Name);
				if (col != null)
				{
					init.Add(Expression.Bind(m.Info, MapColumn(record, col.Index, col.Type, m.Type)));
					index = Math.Max(index, col.Index + 1);
				}
			}

			if (init.Count == 0)
				return Expression.New(t);

			return Expression.MemberInit(Expression.New(t), init);
		}

		static Expression MapColumn(Expression record, int index, Type dbType, Type expectedType)
		{
			MethodInfo getter;
			if (!TypedGetters.TryGetValue("Get" + dbType.Name, out getter))
				getter = ObjectValueGetter;

			var idx = Expression.Constant(index);
			var expr = (Expression)Expression.Call(record, getter, idx);

			if (expr.Type == typeof(Object))
			{
				if (!dbType.IsValueType)
					expr = Expression.Convert(expr, dbType);
				else if (expectedType.IsNullableOf(dbType))
					expr = Expression.Unbox(expr, expectedType);
				else
					expr = Expression.Unbox(expr, dbType);
			}

			try
			{
				// this is likely to throw if field type doesn't match column type
				if (expr.Type != expectedType)
					expr = Expression.Convert(expr, expectedType);
			}
			catch (Exception e)
			{
				throw new MemberMappingException(e);
			}

			if (!expectedType.IsValueType)
				expr = Expression.Condition(Expression.Call(record, IsDBNullGetter, idx), Expression.Constant(null, expr.Type), expr);
			else if (expectedType.IsNullable())
				expr = Expression.Condition(Expression.Call(record, IsDBNullGetter, idx), Expression.Default(expr.Type), expr);

			return expr;
		}

		static string GetTableName(Type t)
		{
			var attr = t.GetCustomAttributes<EntityAttribute>().FirstOrDefault();
			return (attr != null && !String.IsNullOrEmpty(attr.Table) ? attr.Table : t.Name).ToLower();
		}

		static IEnumerable<RecordColumn> GetColumns(IDataReader reader)
		{
			for (int i = 0; i < reader.FieldCount; i++)
			{
				yield return new RecordColumn {
					Index = i,
					Name = reader.GetName(i).ToLower(),
					Type = reader.GetFieldType(i),
				};
			}
		}

		private class RecordColumn
		{
			public int Index;
			public string Name;
			public Type Type;
		}

		static IEnumerable<ObjectMember> GetMembers(Type t)
		{
			foreach (var p in t.GetProperties(MemberFlags)
				.Where(x => x.CanWrite && x.GetIndexParameters().Length == 0)
				.Where(x => x.GetCustomAttribute<IgnoreAttribute>() == null))
			{
				var attr = p.GetCustomAttribute<FieldAttribute>();
				yield return new ObjectMember {
					Name = (attr != null && attr.Name != null ? attr.Name : p.Name).ToLower(),
					Type = p.PropertyType,
					Info = p,
				};
			}

			foreach (var f in t.GetFields(MemberFlags)
				.Where(x => !x.IsInitOnly)
				.Where(x => x.GetCustomAttribute<IgnoreAttribute>() == null))
			{
				var attr = f.GetCustomAttribute<FieldAttribute>();
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
			public MemberMappingException(Exception e)
				: base("Error mapping", e)
			{
			}
		}
	}
}
