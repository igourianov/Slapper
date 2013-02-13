using System;
using System.Linq;
using NUnit.Framework;
using Slapper.Reflection;

namespace Slapper.Tests
{
	[TestFixture]
	public class ParameterReaderTests
	{
		[Test]
		public void ReadObject()
		{
			var prms = ParameterReader.Read(new Foo { Bar = "BAR!", Baz = 10 });
			Assert.AreEqual(2, prms.Count());
			Assert.IsTrue(prms.Any(x => x.Key == "Bar" && (string)x.Value == "BAR!"));
			Assert.IsTrue(prms.Any(x => x.Key == "Baz" && (int)x.Value == 10));
		}

		public class Foo
		{
			public string Bar;
			public int Baz { get; set; }
			private string IgnoreMe = "IGNORE";
		}
	}
}
