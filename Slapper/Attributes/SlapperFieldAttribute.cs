using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slapper.Attributes
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
	public class SlapperFieldAttribute : SlapperAttribute
	{
		public string Name;
		public FieldFlags Flags;

		public SlapperFieldAttribute(string name, FieldFlags flags)
		{
			Name = name;
			Flags = flags;
		}

		public SlapperFieldAttribute(string name)
			: this(name, FieldFlags.None)
		{
		}

		public SlapperFieldAttribute()
			: this(null)
		{
		}
	}

	[Flags]
	public enum FieldFlags
	{
		None = 0,
		Key = 1,
		ReadOnly = 2,
		Identity = 4,
	}
}
