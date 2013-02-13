using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slapper.Attributes
{
	public class EntityField : SlapperAttribute
	{
		public string Name;

		public EntityField(string name)
		{
			Name = name;
		}
	}
}
