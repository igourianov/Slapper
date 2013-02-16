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

		public Field(string name)
		{
			Name = name;
		}
	}

	public class Key : Field
	{
		public bool IsIdentity;

		public Key(string name, bool isIdentity)
			: base(name)
		{
			IsIdentity = isIdentity;
		}
	}
}
