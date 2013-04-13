﻿using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Slapper.Reflection;

namespace Slapper.Tests.Utils
{
	[TestClass]
	public class ParameterReaderTests
	{
		[TestMethod]
		public void ReadSpecificInterface()
		{
			var prms = ParameterReader.Read<IFoo>(new Foo { Bar = "BAR!", Baz = 10 });
			Assert.AreEqual(1, prms.Count());
			Assert.IsTrue(prms.Any(x => x.Key == "Baz" && (int)x.Value == 10));
		}

		[TestMethod]
		public void ReadUntypedObject()
		{
			var prms = ParameterReader.Read((object)new Foo { Bar = "BAR!", Baz = 10 });
			Assert.AreEqual(2, prms.Count());
			Assert.IsTrue(prms.Any(x => x.Key == "Bar" && (string)x.Value == "BAR!"));
			Assert.IsTrue(prms.Any(x => x.Key == "Baz" && (int)x.Value == 10));
		}

		public class Foo : IFoo
		{
			public string Bar;
			public int Baz { get; set; }
			private string IgnoreMe = "IGNORE";
		}

		public interface IFoo
		{
			int Baz { get; set; }
		}
	}
}
