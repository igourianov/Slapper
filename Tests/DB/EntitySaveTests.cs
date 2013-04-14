using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Slapper.Tests.Model;

namespace Slapper.Tests.DB
{
	[TestClass]
	public class EntitySaveTests : TestBase
	{
		[TestMethod]
		public void BasicEntityUpdate()
		{
			using (var conn = OpenConnection())
			{
				var employee = conn.Query<Employee>("select top 1 * from Employee").First();
				employee.Name = "FOO BAR";
				conn.Update(employee);
				employee = conn.Query<Employee>("select top 1 * from Employee where ID=@ID", employee).First();
				Assert.AreEqual("FOO BAR", employee.Name);
			}
		}

		[TestMethod]
		public void InheritedEntityUpdate()
		{
			using (var conn = OpenConnection())
			{
				var employee = conn.Query<EmployeeEx>(@"
select top 1 *
from Employee e
inner join Company c on (c.ID=e.CompanyID)
").First();
				employee.Name = "FOO BAR";
				conn.Update(employee);
				employee = conn.Query<EmployeeEx>("select top 1 * from Employee where ID=@ID", employee).First();
				Assert.AreEqual("FOO BAR", employee.Name);
			}
		}

		public class EmployeeEx : Employee
		{
			public string CompanyName;
		}
	}
}
