using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Slapper.Reflection
{
	public static class ParameterMapper
	{
		static ConcurrentDictionary<Type, Func<object, IEnumerable<IParameter>>> Cache = new ConcurrentDictionary<Type, Func<object, IEnumerable<IParameter>>>();
		static BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.Public;

		public static IEnumerable<IParameter> Read(object obj)
		{
			return GetReader(obj.GetType())(obj);
		}

		public static IEnumerable<IParameter> Read<T>(T obj)
		{
			return GetReader(typeof(T))(obj);
		}

		static Func<object, IEnumerable<IParameter>> GetReader(Type t)
		{
			return Cache.GetOrAdd(t, CreateObjectReader);
		}

		static Func<object, IEnumerable<IParameter>> CreateObjectReader(Type t)
		{
			var param = Expression.Parameter(typeof(object), "param");
			var obj = Expression.Variable(t, "obj");
			var members = new List<Expression>();

			foreach (var p in t.GetProperties(InstanceFlags).Where(o => o.GetIndexParameters().Length == 0 && o.CanRead))
			{
				members.Add(Expression.New(GetParamCtor(p.PropertyType), Expression.Constant(p.Name), Expression.Constant(p.PropertyType), Expression.Property(obj, p)));
			}

			foreach (var f in t.GetFields(InstanceFlags))
			{
				members.Add(Expression.New(GetParamCtor(f.FieldType), Expression.Constant(f.Name), Expression.Constant(f.FieldType), Expression.Field(obj, f)));
			}

			var block = Expression.Block(
				typeof(IParameter[]),
				new ParameterExpression[] { obj },
				Expression.Assign(obj, Expression.Convert(param, t)),
				Expression.NewArrayInit(typeof(IParameter), members)
			);

			return Expression.Lambda<Func<object, IEnumerable<IParameter>>>(block, param).Compile();
		}

		static ConstructorInfo GetParamCtor(Type type)
		{
			return typeof(Parameter<>).MakeGenericType(type).GetConstructor(new Type[] { typeof(string), typeof(Type), type });
		}
	}

}
