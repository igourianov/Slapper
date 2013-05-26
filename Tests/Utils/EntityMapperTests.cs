using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Slapper.Reflection;
using Slapper.Tests.Model;

namespace Slapper.Tests.Utils
{
	[TestClass]
	public class EntityMapperTests
	{
		[TestMethod]
		public void ReadFields()
		{
			var reader = EntityMapper.CreateFieldReader<Employee>();
			var fields = reader(new Employee { Name = "Foo Bar" });

			var name = fields.FirstOrDefault(x => x.Name == "Name");
			Assert.IsNotNull(name);
			Assert.AreEqual("Foo Bar", (string)name.Value);
			Assert.IsTrue(name.Modified);

			var id = fields.FirstOrDefault(x => x.Name == "ID");
			Assert.IsNotNull(id);
			Assert.IsFalse(id.Modified);
		}

		[TestMethod]
		public void ReadOnlyField()
		{
			var reader = EntityMapper.CreateFieldReader<Company>();
			var fields = reader(new Company { Name = "Foo" });

			var index = fields.FirstOrDefault(x => x.Name == "IndexName");
			Assert.IsNotNull(index);
			Assert.IsFalse(index.Modified);
		}

		[TestMethod]
		public void SetIdentity()
		{
			var setter = EntityMapper.CreateIdentitySetter<Employee>();
			var obj = new Employee();
			setter(obj, 10);
			Assert.AreEqual(10, obj.ID);
		}
	}
}
