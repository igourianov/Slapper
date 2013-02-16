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

		public Entity(string table)
		{
			Table = table;
		}
	}
}
