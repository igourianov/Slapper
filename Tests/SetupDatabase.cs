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
if exists(select top 1 null from sys.objects where type='U' and name='Employee')
	drop table Employee;

create table Employee
(
	ID int primary key identity(1,1),
	Name varchar(200) not null,
	CompanyID int not null,
);

if exists(select top 1 null from sys.objects where type='U' and name='Company')
	drop table Company;

create table Company
(
	ID int primary key identity(1,1),
	Name varchar(200) not null,
);
");
				conn.ExecuteNonQuery(@"
insert into Company (Name) values ('Planet Express');
insert into Company (Name) values ('DOOP');
insert into Company (Name) values ('Mom Corp');

insert into Employee (Name, CompanyID) values ('Philip Fry', 1);
insert into Employee (Name, CompanyID) values ('Bender', 1);
insert into Employee (Name, CompanyID) values ('Hermes Conrad', 1);
insert into Employee (Name, CompanyID) values ('Zapp Brannigan', 2);
insert into Employee (Name, CompanyID) values ('Kif Kroker', 2);
insert into Employee (Name, CompanyID) values ('Mom', 3);
");
			}
		}

	}
}
