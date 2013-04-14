using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slapper.Attributes
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class SlapperEntityAttribute : SlapperAttribute
	{
		public string Table;

		public SlapperEntityAttribute(string table)
		{
			Table = table;
		}
	}
}
