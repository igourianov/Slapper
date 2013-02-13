using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Slapper;
using System.Data.SqlClient;
using System.Configuration;

namespace Slapper.Tests
{
	[TestFixture]
	public class SqlTests
	{
		private SqlConnection OpenConnection()
		{
			var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["Test"].ConnectionString);
			conn.Open();
			return conn;
		}

		[Test]
		public void BasicRead()
		{
			using (var conn = OpenConnection())
			{
				using (var reader = conn.ExecuteReader("select top 1 * from Employee where Name like @Name", new { Name = "Zapp%" }))
				{
					Assert.IsTrue(reader.Read());
					Assert.Greater((int)reader["ID"], 0);
					Assert.AreEqual("Zapp Brannigan", reader["Name"]);
				}

				var company = conn.ExecuteScalar<int>("select top 1 CompanyID from Employee where Name=@Name", new { Name = "Hermes Conrad" });
				Assert.AreEqual(1, company);
			}
		}
	}
}
