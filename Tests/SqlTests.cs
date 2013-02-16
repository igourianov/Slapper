using System.Configuration;
using System.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Slapper.Tests
{
	[TestClass]
	public class SqlTests
	{
		private SqlConnection OpenConnection()
		{
			var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["Test"].ConnectionString);
			conn.Open();
			return conn;
		}

		[TestMethod]
		public void BasicRead()
		{
			using (var conn = OpenConnection())
			{
				using (var reader = conn.ExecuteReader("select top 1 * from Employee where Name like @Name", new { Name = "Zapp%" }))
				{
					Assert.IsTrue(reader.Read());
					Assert.AreNotEqual((int)reader["ID"], 0);
					Assert.AreEqual("Zapp Brannigan", reader["Name"]);
				}

				var company = conn.ExecuteScalar<int>("select top 1 CompanyID from Employee where Name=@Name", new { Name = "Hermes Conrad" });
				Assert.AreEqual(1, company);
			}
		}
	}
}
