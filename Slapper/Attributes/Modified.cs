using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slapper.Attributes
{
	public class Modified : SlapperAttribute
	{
		public string Name;

		public Modified(string name)
		{
			Name = name;
		}
	}
}
