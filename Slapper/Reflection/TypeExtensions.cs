 using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Slapper.Reflection
{
	public static class TypeExtensions
	{
		public static IEnumerable<T> GetCustomAttributes<T>(this MemberInfo mi, bool inherit = true)
			where T : Attribute
		{
			return mi.GetCustomAttributes(typeof(T), inherit).Cast<T>();
		}

		public static bool IsNullable(this Type type)
		{
			return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
		}

		public static bool IsNullableOf(this Type type, Type t)
		{
			return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) && type.GetGenericArguments()[0] == t;
		}

		public static bool IsScalar(this Type type)
		{
			return type.IsValueType || type == typeof(String) || type == typeof(byte[]);
		}
	}
}
