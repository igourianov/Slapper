using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Slapper.Tests
{
	[SetUpFixture]
	public class SetupDatabase
	{
		[SetUp]
		public void Setup()
		{
			using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["Test"].ConnectionString))
			{
				conn.Open();

				conn.ExecuteNonQuery(@"
if exists(select top 1 null from sys.objects where type='U' and name='Person')
	drop table Person;

create table Person
(
	ID int primary key identity(1,1),
	Name varchar(100) null,
	DOB date null,
);
");
			}
		}

	}
}
