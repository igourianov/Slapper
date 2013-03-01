using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slapper.Attributes
{
	public class Field : SlapperAttribute
	{
		public string Name;
		public FieldFlags Flags;

		public Field(string name, FieldFlags flags)
		{
			Name = name;
			Flags = flags;
		}

		public Field(string name)
			 : this(name, FieldFlags.None)
		{
		}

		public Field()
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
