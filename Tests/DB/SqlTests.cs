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
			using (var reader = conn.ExecuteReader("select top 1 * from Employee where Name like @Name", new { Name = "Zapp%" }))
			{
				Assert.IsTrue(reader.Read());
				Assert.AreEqual(4, (int)reader["ID"]);
				Assert.AreEqual("Zapp Brannigan", (string)reader["Name"]);
			}
		}

		[TestMethod, TestCategory("SqlExtensions")]
		public void TransactionSupport()
		{
			using (var conn = Database.OpenConnection())
			{
				using (var txn = conn.BeginTransaction())
				{
					txn.ExecuteNonQuery("update Employee set Name='foo'");
					txn.Rollback();
				}
				Assert.AreEqual(0, conn.ExecuteScalar<int>("select count (*) from Employee where Name='foo'"));
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
