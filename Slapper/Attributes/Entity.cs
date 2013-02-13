using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slapper.Attributes
{
	public class Entity : SlapperAttribute
	{
		public string Table;
		public bool ExplicitLayout;

		public Entity(string table, bool explicitLayout)
		{
			Table = table;
			ExplicitLayout = explicitLayout;
		}

		public Entity(string table)
			: this(table, false)
		{
		}
	}
}
