using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Slapper.Attributes;

namespace Slapper.Tests
{
	[TestClass]
	public class QueryTests
	{
		private SqlConnection OpenConnection()
		{
			var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["Test"].ConnectionString);
			conn.Open();
			return conn;
		}

		[TestMethod]
		public void QueryObjects()
		{
			using (var conn = OpenConnection())
			{
				var list = conn.Query<GoodEmployee>("select * from Employee").ToList();
				Assert.AreNotEqual(0, list.Count);
				Assert.IsFalse(list.Any(x => x.EmployeeID == 0));
				Assert.IsFalse(list.Any(x => String.IsNullOrEmpty(x.Name)));
				Assert.IsFalse(list.Any(x => x.CompanyID == null));
			}
		}

		[TestMethod]
		public void QueryValues()
		{
			using (var conn = OpenConnection())
			{
				var list = conn.Query<int>("select ID from Employee").ToList();
				Assert.AreNotEqual(0, list.Count);
				Assert.IsFalse(list.Any(x => x == 0));
			}
		}

		[TestMethod]
		public void CheckConnectionFree()
		{
			using (var conn = OpenConnection())
			{
				var x = conn.Query<string>("select Name from Employee").First();
				var y = conn.Query<string>("select Name from Employee").First();
			}
		}

		[TestMethod, ExpectedException(typeof(Slapper.Reflection.DataReaderMapper.MemberMappingException))]
		public void CheckBindException()
		{
			using (var conn = OpenConnection())
			{
				var list = conn.Query<BadEmployee>("select * from Employee").ToList();
			}
		}

		[TestMethod]
		public void QueryObjectTuple()
		{
			using (var conn = OpenConnection())
			{
				var list = conn.Query<Employee, Company>(@"
select *
from Employee e
inner join Company c on(c.ID=e.CompanyID)
").ToList();
				
				Assert.AreNotEqual(0, list.Count);
				Assert.IsFalse(list.All(x => x.Item1.ID == x.Item2.ID));
				Assert.IsFalse(list.All(x => x.Item1.Name == x.Item2.Name));
			}
		}

		public class GoodEmployee
		{
			[EntityField("ID")]
			public int EmployeeID;
			public string Name { get; set; }
			public int? CompanyID;
		}

		public class BadEmployee
		{
			public DateTime ID;
			public string Name;
		}

		public class Employee
		{
			public int ID;
			public string Name;
			public int CompanyID;
		}

		public class Company
		{
			public int ID;
			public string Name;
		}
	}
}
