﻿using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Slapper.Attributes;
using Slapper.Tests.Model;

namespace Slapper.Tests.DB
{
	[TestClass]
	public class QueryTests : TestBase
	{
		[TestMethod]
		public void QueryTuple()
		{
			using (var conn = OpenConnection())
			{
				var list = conn.Query<Tuple<Employee,Company>>(@"
select *
from Employee e
inner join Company c on(c.ID=e.CompanyID)").ToList();

				Assert.AreNotEqual(0, list.Count);
				Assert.IsFalse(list.All(x => x.Item1.ID == x.Item2.ID));
				Assert.IsFalse(list.All(x => x.Item1.Name == x.Item2.Name));
			}
		}

		[TestMethod]
		public void QueryObjects()
		{
			using (var conn = OpenConnection())
			{
				var list = conn.Query<Employee>("select * from Employee").ToList();
				Assert.AreNotEqual(0, list.Count);
				Assert.IsFalse(list.Any(x => x.ID == 0));
				Assert.IsFalse(list.Any(x => String.IsNullOrEmpty(x.Name)));
				Assert.IsFalse(list.Any(x => x.CompanyID == 0));
			}
		}

		[TestMethod]
		public void QueryInherited()
		{
			using (var conn = OpenConnection())
			{
				var e = conn.Query<EmployeeEx>(@"
select e.*, c.Name as CompanyName
from Employee e
inner join Company c on(c.ID=e.CompanyID)
").First();
				Assert.IsNotNull(e.Name);
				Assert.IsNotNull(e.CompanyName);
			}
		}

		private class EmployeeEx : Employee
		{
			public string CompanyName;
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

		public class BadEmployee
		{
			public DateTime ID;
			public string Name;
		}

	}
}
