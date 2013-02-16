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
		static MethodInfo[] Getters;
		static MethodInfo IsDBNull;
		static BindingFlags MemberFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

		static DataReaderMapper()
		{
			IsDBNull = typeof(IDataRecord).GetMethod("IsDBNull", new Type[] { typeof(int) });
			Getters = typeof(IDataRecord).GetMethods(BindingFlags.Public | BindingFlags.Instance)
				.Where(x => x.Name.StartsWith("Get"))
				.Where(x => { var prms = x.GetParameters(); return prms.Length == 1 && prms[0].ParameterType == typeof(int); })
				.OrderBy(x => x.Name)
				.ToArray();
		}

		public static Func<IDataRecord, T> CreateValueMapper<T>(IDataReader reader)
		{
			var readerInstance = Expression.Parameter(typeof(IDataRecord), "reader");
			Expression expr = Expression.Call(readerInstance, GetSchema(reader).First().Getter, Expression.Constant(0));
			if (!typeof(T).IsAssignableFrom(expr.Type))
				expr = Expression.Convert(expr, typeof(T));
			return Expression.Lambda<Func<IDataRecord, T>>(expr, readerInstance).Compile();
		}

		public static Func<IDataRecord, T> CreateObjectMapper<T>(IDataReader reader)
		{
			var t = typeof(T);
			var entity = t.GetCustomAttributes<Entity>().FirstOrDefault();
			var tableName = (entity != null && entity.Table != null ? entity.Table : t.Name).ToLower();
			var explicitLayout = entity != null ? entity.ExplicitLayout : false;
			var schema = GetSchema(reader).ToList();
			var members = t.GetProperties(MemberFlags)
				.Where(x => x.CanWrite && x.GetIndexParameters().Length == 0)
				.Cast<MemberInfo>()
				.Concat(t.GetFields(MemberFlags).Where(x => !x.IsInitOnly))
				.Where(x => !x.GetCustomAttributes<IgnoreField>().Any())
				.ToList();

			var readerInstance = Expression.Parameter(typeof(IDataRecord), "reader");
			var objectInstance = Expression.Parameter(t, "obj");
			List<Expression> body = new List<Expression>();
			body.Add(Expression.Assign(objectInstance, Expression.New(t)));

			foreach (var m in members)
			{
				var attr = m.GetCustomAttributes<EntityField>().FirstOrDefault();
				var name = (attr != null && attr.Name != null ? attr.Name : m.Name).ToLower();
				var col = schema.FirstOrDefault(x => x.Name == name && x.Table == tableName)
					?? schema.FirstOrDefault(x => x.Name == name && String.IsNullOrEmpty(x.Table));

				if (col != null && (!explicitLayout || attr != null))
					body.Add(AssignValue(objectInstance, m, readerInstance, col.Getter, col.Index));
			}

			body.Add(objectInstance); // return value

			var block = Expression.Block(t, new ParameterExpression[] { objectInstance }, body);
			return Expression.Lambda<Func<IDataRecord, T>>(block, readerInstance).Compile();
		}

		static Expression AssignValue(Expression obj, MemberInfo member, Expression reader, MethodInfo getter, int index)
		{
			Expression left = member is FieldInfo
				? Expression.Field(obj, (FieldInfo)member)
				: Expression.Property(obj, (PropertyInfo)member);

			Expression right = Expression.Call(reader, getter, Expression.Constant(index));

			try
			{
				if (left.Type != right.Type)
					right = Expression.Convert(right, left.Type);
			}
			catch (InvalidOperationException e)
			{
				throw new MemberMappingException(obj.Type, member, e);
			}

			return Expression.Assign(left, right);
		}

		static IEnumerable<ReaderColumn> GetSchema(IDataReader reader)
		{
			var tableMap = reader.GetSchemaTable().Rows.Cast<DataRow>()
				.Select(x => new { Index = x.Field<int>("ColumnOrdinal"), Table = x.Field<string>("BaseTableName") })
				.ToList();

			for (int i = 0; i < reader.FieldCount; i++)
			{
				yield return new ReaderColumn {
					Index = i,
					Name = reader.GetName(i).ToLower(),
					Getter = FindGetter(reader.GetFieldType(i)),
					Table = (tableMap.Where(x => x.Index == i).Select(x => x.Table).FirstOrDefault() ?? "").ToLower(),
				};
			}
		}

		static MethodInfo FindGetter(Type type)
		{
			var getterName = "Get" + (!type.IsNullable() ? type.Name : type.GetGenericArguments()[0].Name);
			if (getterName == "GetSingle")
				getterName = "GetFloat";
			return Getters.FirstOrDefault(x => x.Name == getterName) ?? Getters.FirstOrDefault(x => x.Name == "GetValue");
		}

		private class ReaderColumn
		{
			public int Index;
			public string Name;
			public string Table;
			public MethodInfo Getter;
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
