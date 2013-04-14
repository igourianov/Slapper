using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Slapper.Attributes;
using System.Reflection;

namespace Slapper.Reflection
{
	public static class EntityMapper
	{
		static ConstructorInfo ColumnCtor = typeof(Column).GetConstructor(new Type[] { typeof(string), typeof(object), typeof(bool), typeof(FieldFlags) });
		static BindingFlags InstanceFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

		public class Column
		{
			public string Name;
			public object Value;
			public bool Modified;
			public FieldFlags Flags;

			public Column(string name, object value, bool modified, FieldFlags flags)
			{
				Name = name;
				Value = value;
				Modified = modified;
				Flags = flags;
			}
		}

		public class Map<T>
		{
			public string Table;
			public Func<T, IEnumerable<EntityMapper.Column>> FieldReader;
			public Action<T, int?> IdentitySetter;
		}

		public static Map<T> CreateMap<T>()
		{
			var attr = typeof(T).GetCustomAttribute<SlapperEntityAttribute>();
			return new Map<T> {
				Table = attr != null && attr.Table != null ? attr.Table : typeof(T).Name,
				FieldReader = CreateFieldReader<T>(),
				IdentitySetter = CreateIdentitySetter<T>(),
			};
		}

		public static Action<T, int?> CreateIdentitySetter<T>()
		{
			var member = GetBindableMembers(typeof(T)).FirstOrDefault(x => {
				var attr = x.GetCustomAttribute<SlapperFieldAttribute>();
				return attr != null && (attr.Flags & FieldFlags.Identity) > 0;
			});

			if (member == null)
				return null;

			var obj = Expression.Parameter(typeof(T), "obj");
			var value = Expression.Parameter(typeof(int?), "value");

			return Expression.Lambda<Action<T, int?>>(Expression.Assign(Expression.MakeMemberAccess(obj, member), value), obj, value).Compile();
		}

		public static Func<T, IEnumerable<Column>> CreateFieldReader<T>()
		{
			MemberInfo modifier;
			var obj = Expression.Parameter(typeof(T), "obj");
			var values = new List<Expression>();
			var members = GetBindableMembers(typeof(T)).ToArray();
			var modifiers = members.Select(x => new { Member = x, Attr = x.GetCustomAttribute<SlapperFieldModifierAttribute>() })
				.Where(x => x.Attr != null)
				.ToDictionary(x => x.Attr.Name, x => x.Member);

			foreach (var member in members)
			{
				var attr = member.GetCustomAttribute<SlapperFieldAttribute>();
				if (attr != null)
				{
					var name = attr.Name ?? member.Name;
					modifiers.TryGetValue(name, out modifier);
					values.Add(Expression.New(ColumnCtor,
						Expression.Constant(name),
						Expression.Convert(Expression.MakeMemberAccess(obj, member), typeof(object)),
						modifier == null ? (Expression)Expression.Constant((attr.Flags & FieldFlags.ReadOnly) == 0) : Expression.MakeMemberAccess(obj, modifier),
						Expression.Constant(attr.Flags)
					));
				}
			}

			return Expression.Lambda<Func<T, IEnumerable<Column>>>(Expression.NewArrayInit(typeof(Column), values), obj).Compile();
		}

		static IEnumerable<MemberInfo> GetBindableMembers(Type t)
		{
			var fields = t.GetFields(InstanceFlags)
				.Where(x => !x.IsInitOnly);

			var props = t.GetProperties(InstanceFlags)
				.Where(x => x.GetIndexParameters().Length == 0 && x.CanRead);

			return fields.Cast<MemberInfo>().Concat(props)
				.Where(x => x.GetCustomAttribute<SlapperIgnoreAttribute>() == null);
		}
	}

	
}
