using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slapper.Attributes
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
	public class ModifierAttribute : SlapperAttribute
	{
		public string Name;

		public ModifierAttribute(string name)
		{
			Name = name;
		}
	}
}
