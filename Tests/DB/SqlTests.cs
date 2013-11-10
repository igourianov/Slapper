using System;
using System.Configuration;
using System.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Slapper.Tests.DB
{
	[TestClass]
	public class SqlTests
	{
		[TestMethod, TestCategory("SqlExtensions")]
		public void BasicRead()
		{
			using (var conn = Database.OpenConnection())
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

		[TestMethod, TestCategory("SqlExtensions")]
		public void TransactionSupport()
		{
			using (var conn = Database.OpenConnection())
			using (var txn = conn.BeginTransaction())
			{
				txn.ExecuteScalar<int>("select count(*) from Company");
				txn.Rollback();
			}
		}

		[TestMethod, TestCategory("SqlExtensions")]
		public void ReplaceParameters()
		{
			using (var conn = Database.OpenConnection())
			using (var cmd = conn.CreateCommand("select ID from Employee where Name=@name"))
			{
				var id1 = cmd.ExecuteScalar<int>(new { name = "Bender" });
				var id2 = cmd.ExecuteScalar<int>(new { name = "Mom" });
				Assert.AreNotEqual(id1, id2);
			}
		}
	}
}
