using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using KeyValue = System.Collections.Generic.KeyValuePair<string, object>;
using Reader = System.Func<object, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, object>>>;

namespace Slapper.Reflection
{
	public static class ParameterReader
	{
		static ConcurrentDictionary<Type, Reader> ObjectReaderCache = new ConcurrentDictionary<Type, Reader>();
		static ConstructorInfo KeyValueCtor = typeof(KeyValue).GetConstructor(new Type[] { typeof(string), typeof(object) });
		static BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.Public;

		public static IEnumerable<KeyValue> Read(object obj)
		{
			return GetReader(obj.GetType())(obj);
		}

		public static IEnumerable<KeyValue> Read<T>(T obj)
		{
			return GetReader(typeof(T))(obj);
		}

		static Reader GetReader(Type t)
		{
			if (typeof(IDictionary).IsAssignableFrom(t))
				return DictionaryReader;

			if (typeof(IEnumerable).IsAssignableFrom(t))
				return EnumerableReader;

			return ObjectReaderCache.GetOrAdd(t, CreateObjectReader);
		}

		static IEnumerable<KeyValue> DictionaryReader(object obj)
		{
			var dict = (IDictionary)obj;
			return dict.Keys.Cast<object>().Select(o => new KeyValue(o.ToString(), dict[o]));
		}

		static IEnumerable<KeyValue> EnumerableReader(object obj)
		{
			return ((IEnumerable)obj).Cast<object>().Select((o, i) => new KeyValue(i.ToString(), o));
		}

		static Reader CreateObjectReader(Type t)
		{
			var param = Expression.Parameter(typeof(object), "param");
			var obj = Expression.Variable(t, "obj");

			var members = t.GetProperties(InstanceFlags)
				.Where(o => o.GetIndexParameters().Length == 0 && o.CanRead)
				.Cast<MemberInfo>()
				.Concat(t.GetFields(InstanceFlags))
				.Select(x => Expression.New(KeyValueCtor, Expression.Constant(x.Name), Expression.Convert(Expression.MakeMemberAccess(obj, x), typeof(Object))));

			var block = Expression.Block(
				typeof(KeyValue[]),
				new ParameterExpression[] { obj },
				new Expression[] {
					Expression.Assign(obj, Expression.Convert(param, t)),
					Expression.NewArrayInit(typeof(KeyValue), members)
				}
			);
			return Expression.Lambda<Reader>(block, param).Compile();
		}

	}

}
