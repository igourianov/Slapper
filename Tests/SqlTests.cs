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
		public void BasicTest()
		{
			using (var conn = OpenConnection())
			{
				var ret = conn.ExecuteNonQuery("insert into Person (Name, DOB) values (@Name, @DOB)", new { Name = "Ilia", DOB = new DateTime(1981,03,18) });
				Assert.AreEqual(1, ret);

				using (var reader = conn.ExecuteReader("select top 1 * from Person order by 1 desc"))
				{
					Assert.IsTrue(reader.Read());
					Assert.Greater((int)reader["ID"], 0);
					Assert.AreEqual("Ilia", reader["Name"]);
				}

				var dob = conn.ExecuteScalar<DateTime>("select top 1 DOB from Person order by 1 desc");
				Assert.AreEqual(new DateTime(1981, 03, 18), dob);

				ret = conn.ExecuteNonQuery("delete from Person");
				Assert.AreEqual(1, ret);
			}
		}
	}
}
