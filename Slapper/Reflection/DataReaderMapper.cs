﻿using System;
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
		static MethodInfo[] Getters;
		static MethodInfo IsDBNull;
		static BindingFlags MemberFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

		static DataReaderMapper()
		{
			IsDBNull = typeof(IDataRecord).GetMethod("IsDBNull", new Type[] { typeof(int) });
			Getters = typeof(IDataRecord).GetMethods(BindingFlags.Public | BindingFlags.Instance)
				.Where(x => x.Name.StartsWith("Get"))
				.Where(x => {
					var prms = x.GetParameters();
					return prms.Length == 1 && prms[0].ParameterType == typeof(int);
				})
				.OrderBy(x => x.Name)
				.ToArray();
		}

		public static Func<IDataRecord, T> CreateValueMapper<T>(IDataReader reader)
		{
			var record = Expression.Parameter(typeof(IDataRecord), "record");
			return Expression.Lambda<Func<IDataRecord, T>>(RecordValue(record, GetSchema(reader).First(), typeof(T)), record).Compile();
		}

		public static Func<IDataRecord, T> CreateObjectMapper<T>(IDataReader reader, bool strictTableRules = false)
		{
			var t = typeof(T);
			var attr = t.GetCustomAttributes<Entity>().FirstOrDefault();
			var tableName = (attr != null && attr.Table != null ? attr.Table : t.Name).ToLower();
			var schema = GetSchema(reader)
				.Where(x => !strictTableRules || String.IsNullOrEmpty(x.Table) || x.Table == tableName)
				.OrderBy(x => String.IsNullOrEmpty(x.Table))
				.ToList();
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
						init.Add(Expression.Bind(m.Info, RecordValue(record, col, m.Type)));
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

		static Expression RecordValue(Expression record, RecordColumn col, Type returnType)
		{
			Expression value = Expression.Call(record, FindGetter(col.Type), Expression.Constant(col.Index));
			if (value.Type != returnType)
				value = Expression.Convert(value, returnType);
			if (!returnType.IsValueType || returnType.IsNullable())
				value = Expression.Condition(Expression.Call(record, IsDBNull, Expression.Constant(col.Index)), Expression.Default(value.Type), value);
			return value;
		}

		static MethodInfo FindGetter(Type type)
		{
			var getterName = "Get" + (!type.IsNullable() ? type.Name : type.GetGenericArguments()[0].Name);
			if (getterName == "GetSingle")
				getterName = "GetFloat";
			return Getters.FirstOrDefault(x => x.Name == getterName) ?? Getters.FirstOrDefault(x => x.Name == "GetValue");
		}

		static IEnumerable<RecordColumn> GetSchema(IDataReader reader)
		{
			var tableMap = reader.GetSchemaTable().Rows.Cast<DataRow>()
				.Select(x => new { Index = x.Field<int>("ColumnOrdinal"), Table = x.Field<string>("BaseTableName") })
				.ToList();

			for (int i = 0; i < reader.FieldCount; i++)
			{
				yield return new RecordColumn {
					Index = i,
					Name = reader.GetName(i).ToLower(),
					Type = reader.GetFieldType(i),
					Table = (tableMap.Where(x => x.Index == i).Select(x => x.Table).FirstOrDefault() ?? "").ToLower(),
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
				.Where(x => x.GetCustomAttribute<Ignore>() == null))
			{
				var attr = p.GetCustomAttribute<Field>();
				yield return new ObjectMember {
					Name = (attr != null && attr.Name != null ? attr.Name : p.Name).ToLower(),
					Type = p.PropertyType,
					Info = p,
				};
			}

			foreach (var f in t.GetFields(MemberFlags)
				.Where(x => !x.IsInitOnly)
				.Where(x => x.GetCustomAttribute<Ignore>() == null))
			{
				var attr = f.GetCustomAttribute<Field>();
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
