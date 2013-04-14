using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slapper.Attributes
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
	public class SlapperFieldModifierAttribute : SlapperAttribute
	{
		public string Name;

		public SlapperFieldModifierAttribute(string name)
		{
			Name = name;
		}
	}
}
