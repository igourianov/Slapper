using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using KeyValue = System.Collections.Generic.KeyValuePair<string, object>;

namespace Slapper.Reflection
{
	public static class ParameterReader
	{
		static ConcurrentDictionary<string, object> Cache = new ConcurrentDictionary<string, object>();
		static ConstructorInfo KeyValueCtor = typeof(KeyValue).GetConstructor(new Type[] { typeof(string), typeof(object) });
		static BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.Public;

		public static IEnumerable<KeyValue> Read(object obj)
		{
			return Bind(obj.GetType())(obj);
		}

		public static IEnumerable<KeyValue> Read<T>(T obj)
		{
			return Bind<T>()(obj);
		}

		public static Func<object, IEnumerable<KeyValue>> Bind(Type t)
		{
			return BindImpl<object>(t);
		}

		public static Func<T, IEnumerable<KeyValue>> Bind<T>()
		{
			return BindImpl<T>(typeof(T));
		}

		static Func<T, IEnumerable<KeyValue>> BindImpl<T>(Type t)
		{
			if (typeof(IDictionary).IsAssignableFrom(t))
				return (T obj) => {
					var dict = (IDictionary)obj;
					return dict.Keys.Cast<object>().Select(o => new KeyValue(o.ToString(), dict[o]));
				};

			if (typeof(IEnumerable).IsAssignableFrom(t))
				return (T obj) => ((IEnumerable)obj).Cast<object>().Select((o, i) => new KeyValue(i.ToString(), o));

			string key = t.FullName + ":::" + typeof(T).FullName;

			object tmp;
			if (!Cache.TryGetValue(key, out tmp))
				Cache[key] = tmp = BindObject<T>(t);

			return (Func<T, IEnumerable<KeyValue>>)tmp;
		}

		static Func<T, IEnumerable<KeyValue>> BindObject<T>(Type t)
		{
			var param = Expression.Parameter(typeof(T), "param");
			var obj = Expression.Variable(t, "obj");
			var body = new List<Expression>();

			if (t.IsAssignableFrom(typeof(T)))
				body.Add(Expression.Assign(obj, param));
			else
				body.Add(Expression.Assign(obj, Expression.Convert(param, t)));

			var members = t.GetProperties(InstanceFlags)
				.Where(o => o.GetIndexParameters().Length == 0 && o.CanRead)
				.Cast<MemberInfo>()
				.Concat(t.GetFields(InstanceFlags))
				.Select(x => NewKeyValue(x.Name, obj, x))
				.ToList();

			body.Add(Expression.NewArrayInit(typeof(KeyValue), members));

			var block = Expression.Block(typeof(KeyValue[]), new ParameterExpression[] { obj }, body);
			return Expression.Lambda<Func<T, IEnumerable<KeyValue>>>(block, param).Compile();
		}

		static Expression NewKeyValue(string key, Expression instance, MemberInfo member)
		{
			Expression value = member is FieldInfo
				? Expression.Field(instance, (FieldInfo)member)
				: Expression.Property(instance, (PropertyInfo)member);
			value = Expression.Convert(value, typeof(Object));
			return Expression.New(KeyValueCtor, Expression.Constant(key), value);
		}

	}

}
