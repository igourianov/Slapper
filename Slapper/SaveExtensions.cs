using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slapper.Reflection;

namespace Slapper
{
	public static class SaveExtensions
	{
		static ConcurrentDictionary<Type, object> EntityMapCache = new ConcurrentDictionary<Type, object>();

		

		public static int Update<T>(this IDbConnection conn, T obj)
		{
			return 0;
		}
	}
}
