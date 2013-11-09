using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Slapper;

namespace Slappergen
{
	class Program
	{
		static void Main(string[] args)
		{
			var ns = "Slapper.Tests.Model";
			using (var writer = new StreamWriter(@"..\..\..\Tests\Model.cs", false))
			using (var conn = new SqlConnection(@"server=antec\sqlexpress;trusted_connection=true;database=slapper_tests"))
			{
				conn.Open();

				writer.WriteLine(@"// Slappergen Model
#pragma warning disable 414,649
using System;
using Slapper.Attributes;

namespace {0}
{{", ns);
				GenerateTables(conn, writer);

				writer.WriteLine("}");
			}
		}

		static void GenerateTables(IDbConnection conn, TextWriter writer)
		{
			foreach (var table in conn.Query<string>(@"select name from sysobjects where xtype='U'").ToList())
			{
				var className = GetMemberName(table);
				writer.WriteLine(@"	[SlapperEntity(""{0}"")]
	public partial class {1}
	{{", table, className);

				foreach (var col in GetSchema(conn, table))
				{
					var flags = new string[] {
						col.IsKey ? "Key" : null,
						col.IsReadOnly ? "ReadOnly" : null,
						col.IsIdentity ? "Identity" : null,
					}
						.Where(x => x != null)
						.Select(x => "FieldFlags." + x)
						.ToArray();

					writer.Write(@"		#region {1}
		[SlapperField(""{2}""{3})]
		protected {0} _{1}_Value;
		[SlapperFieldModifier(""{2}"")]
		protected bool _{1}_Modified;
		[SlapperIgnore]
		public virtual {0} {1}
		{{
			get
			{{
				return _{1}_Value;
			}}
			set
			{{
				if (!_{1}_Modified)
					_{1}_Modified = _{1}_Value != value;
				_{1}_Value = value;
			}}
		}}
		#endregion
", GetTypeName(col.Type, col.IsNullable), GetMemberName(col.Name), col.Name, (flags.Length > 0 ? ", " + String.Join(" | ", flags) : null));
				}

				writer.WriteLine("	}");
			}
		}

		static string GetTypeName(Type t, bool nullable)
		{
			string name;
			switch (t.Name)
			{
				case "Int32":
					name = "int";
					break;
				case "Int64":
					name = "long";
					break;
				case "Single":
					name = "float";
					break;
				case "Boolean":
					name = "bool";
					break;
				default:
					name = t.Name;
					break;
			}
			return name + (t.IsValueType ? "?" : "");
		}

		static string GetMemberName(string name)
		{
			name = Regex.Replace(name, @"[^0-9a-zA-Z]+", "_", RegexOptions.Compiled).Trim('_');
			if (Char.IsDigit(name[0]))
				name = "_" + name;
			name = Regex.Replace(name, @"(^|_)([a-z])", (m) => String.Format("{0}{1}", m.Groups[1].Value, m.Groups[2].Value.ToUpper()), RegexOptions.Compiled);
			return name;
		}

		static IEnumerable<ColumnInfo> GetSchema(IDbConnection conn, string table)
		{
			using (var reader = conn.ExecuteReader("select top 0 * from [" + table + "]", null,
				CommandBehavior.SingleResult | CommandBehavior.SchemaOnly | CommandBehavior.KeyInfo))
			{
				var schema = reader.GetSchemaTable();
				int columnOrdinal = schema.Columns.IndexOf("ColumnOrdinal");
				int columnName = schema.Columns.IndexOf("ColumnName");
				int isKey = schema.Columns.IndexOf("IsKey");
				int isIdentity = schema.Columns.IndexOf("IsIdentity");
				int isReadOnly = schema.Columns.IndexOf("IsReadOnly");
				int isNullable = schema.Columns.IndexOf("AllowDBNull");

				var types = Enumerable.Range(0, reader.FieldCount).Select(x => reader.GetFieldType(x)).ToList();

				return schema.Rows.Cast<DataRow>()
					.Select(x => new ColumnInfo {
						Name = x.Field<string>(columnName),
						Type = types[x.Field<int>(columnOrdinal)],
						IsKey = isKey == -1 ? false : x.Field<bool>(isKey),
						IsIdentity = isIdentity == -1 ? false : x.Field<bool>(isIdentity),
						IsReadOnly = isReadOnly == -1 ? false : x.Field<bool>(isReadOnly),
						IsNullable = isNullable == -1 ? true : x.Field<bool>(isNullable),
					});
			}
		}

		public class ColumnInfo
		{
			public string Name;
			public Type Type;
			public bool IsKey;
			public bool IsReadOnly;
			public bool IsIdentity;
			public bool IsNullable;
		}
	}
}
