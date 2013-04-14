using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slapper.Tests.DB
{
	public abstract class TestBase
	{
		protected IDbConnection OpenConnection()
		{
			var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["Test"].ConnectionString);
			conn.Open();
			return conn;
		}
	}
}
