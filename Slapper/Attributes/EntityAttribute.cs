using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slapper.Attributes
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class EntityAttribute : SlapperAttribute
	{
		public string Table;

		public EntityAttribute(string table)
		{
			Table = table;
		}
	}
}
