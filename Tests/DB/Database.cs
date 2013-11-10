using System.Configuration;
using System.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;

namespace Slapper.Tests.DB
{
	[TestClass]
	public class Database
	{
		public static IDbConnection OpenConnection()
		{
			var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["Test"].ConnectionString);
			conn.Open();
			return conn;
		}

		[AssemblyInitialize]
		public static void Setup(TestContext context)
		{
			using (var conn = OpenConnection())
			{
				conn.ExecuteNonQuery(@"
if exists(select top 1 null from sys.objects where type='U' and name='Employee')
	drop table Employee;

create table Employee
(
	ID int primary key identity(1,1),
	Name varchar(200) not null,
	CompanyID int not null,
	ShiftStart time(0) null,
);

if exists(select top 1 null from sys.objects where type='U' and name='Company')
	drop table Company;

create table Company
(
	ID int primary key identity(1,1),
	Name varchar(200) not null,
	[IndexName] as ((replace(str([ID],(6)),' ','0')+'-')+[Name]) persisted,
);
");
				conn.ExecuteNonQuery(@"
insert into Company (Name) values ('Planet Express');
insert into Company (Name) values ('DOOP');
insert into Company (Name) values ('Mom Corp');

insert into Employee (Name, CompanyID, ShiftStart) values ('Philip Fry', 1, '9:00');
insert into Employee (Name, CompanyID, ShiftStart) values ('Bender', 1, '9:30');
insert into Employee (Name, CompanyID, ShiftStart) values ('Hermes Conrad', 1, '8:00');
insert into Employee (Name, CompanyID, ShiftStart) values ('Zapp Brannigan', 2, '12:00');
insert into Employee (Name, CompanyID, ShiftStart) values ('Kif Kroker', 2, '6:00');
insert into Employee (Name, CompanyID, ShiftStart) values ('Mom', 3, '10:00');
");
			}
		}

	}
}
