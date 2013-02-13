using System;
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
		static Type[] Types;
		static MethodInfo[] Getters;
		static MethodInfo IsDBNull;
		static BindingFlags MemberFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

		static DataReaderMapper()
		{
			Types = new Type[] {
				typeof(bool),
				typeof(byte),
				typeof(byte[]),
				typeof(char),
				typeof(DateTime),
				typeof(decimal),
				typeof(double),
				typeof(float),
				typeof(Guid),
				typeof(short),
				typeof(int),
				typeof(long),
				typeof(string)
			};
			IsDBNull = typeof(IDataRecord).GetMethod("IsDBNull", new Type[] { typeof(int) });
			Getters = typeof(IDataRecord).GetMethods(BindingFlags.Public | BindingFlags.Instance)
				.Where(x => x.Name.StartsWith("Get"))
				.Where(x => { var prms = x.GetParameters(); return prms.Length == 1 && prms[0].ParameterType == typeof(int); })
				.OrderBy(x => x.Name)
				.ToArray();
		}

		public static Func<IDataReader, T, T> CreateMapper<T>(IDataReader reader, bool filterByTable)
		{
			var t = typeof(T);
			var entity = t.GetCustomAttributes<Entity>().FirstOrDefault();
			var explicitLayout = entity != null ? entity.ExplicitLayout : false;
			var tableName = (entity != null && entity.Table != null ? entity.Table : t.Name).ToLower();

			var members = GetMembers(t).ToList();
			var schema = GetSchema(reader, filterByTable).ToList();

			List<Expression> assignments = new List<Expression>();
			var readerInstance = Expression.Parameter(typeof(IDataReader), "reader");
			var objectInstance = Expression.Parameter(t, "obj");

			foreach (var m in members)
			{
				var field = m.GetCustomAttributes<EntityField>().FirstOrDefault();
				if (!explicitLayout || field != null)
				{
					var name = (field != null && field.Name != null ? field.Name : m.Name).ToLower();
					var col = schema.FirstOrDefault(x => x.Name == name && (!filterByTable || x.Table == tableName || x.Table == null));

					if (col != null)
						assignments.Add(AssignValue(objectInstance, m, readerInstance, col.Index));
				}
			}

			assignments.Add(objectInstance); // return value

			return Expression.Lambda<Func<IDataReader, T, T>>(Expression.Block(t, assignments), readerInstance, objectInstance).Compile();
		}

		static Expression AssignValue(Expression obj, MemberInfo member, Expression reader, int index)
		{
			Expression left = member is FieldInfo
				? Expression.Field(obj, (FieldInfo)member)
				: Expression.Property(obj, (PropertyInfo)member);

			var right = ReadValue(reader, index, left.Type);
			if (!left.Type.IsAssignableFrom(right.Type))
				right = Expression.Convert(right, left.Type);

			return Expression.Assign(left, right);
		}

		static Expression ReadValue(Expression reader, int index, Type type)
		{
			var getterName = "Get" + (!type.IsNullable() ? type.Name : type.GetGenericArguments()[0].Name);
			if (getterName == "GetSingle")
				getterName = "GetFloat";
			if (getterName == "GetByte[]")
				getterName = "GetValue";
			var method = Getters.FirstOrDefault(x => x.Name == getterName);
			return Expression.Call(reader, method, Expression.Constant(index));
		}

		static IEnumerable<MemberInfo> GetMembers(Type t)
		{
			return t.GetProperties(MemberFlags)
				.Where(x => x.CanWrite && x.GetIndexParameters().Length == 0 && Types.Contains(x.PropertyType))
				.Cast<MemberInfo>()
				.Concat(t.GetFields(MemberFlags).Where(x => !x.IsInitOnly && Types.Contains(x.FieldType)))
				.Where(x => !x.GetCustomAttributes<IgnoreField>().Any());
		}

		static IEnumerable<ReaderColumn> GetSchema(IDataReader reader, bool includeTable)
		{
			if (!includeTable)
			{
				for (int i = 0; i < reader.FieldCount; i++)
					yield return new ReaderColumn { Index = i, Name = reader.GetName(i) };
			}
			else
			{
				var schema = reader.GetSchemaTable();
				int ordinalIndex = schema.Columns.IndexOf("ColumnOrdinal");
				int nameIndex = schema.Columns.IndexOf("ColumnName");
				int tableIndex = schema.Columns.IndexOf("BaseTableName");

				for (int i = 0; i < schema.Rows.Count; i++)
				{
					var row = schema.Rows[i];
					yield return new ReaderColumn { 
						Index = row.Field<int>(ordinalIndex),
						Name = row.Field<string>(nameIndex).ToLower(),
						Table = tableIndex > -1 ? row.Field<string>(tableIndex).ToLower() : null
					};
				}
			}
		}

		private class ReaderColumn
		{
			public int Index;
			public string Name;
			public string Table;
		}
	}
}
