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

				writer.WriteLine(@"// Slappergen
using System;
using System.Collections.Generic;
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
				var className = MakeCSharpName(table);
				writer.WriteLine(@"	[Entity(""{0}"", true)]
	public partial class {1}
	{{", table, className);

				foreach (var col in GetSchema(conn, table))
				{
					var memberName = MakeCSharpName(col.Name);

					if (col.IsKey)
						writer.WriteLine("		[Key(\"{0}\", {1})]", col.Name, col.IsIdentity.ToString().ToLower());
					else
						writer.WriteLine("		[Field(\"{0}\")]", col.Name);

					writer.WriteLine("		private {0} _{1}_Value;", col.Type.Name, memberName);

					if (!col.IsComputed)
						writer.WriteLine(@"		[Modified(""{0}"")]
		private bool _{1}_Modified;", col.Name, memberName);

					writer.WriteLine(@"		[Ignore]
		public virtual {0} {1}
		{{
			get {{ return _{1}_Value; }}", col.Type.Name, memberName);

					if (!col.IsComputed)
					{
						writer.WriteLine("			set {");
						if (col.Type.IsValueType || col.Type == typeof(string))
						{
							writer.WriteLine(@"				if (_{0}_Value != value)
					_{0}_Modified = true;
				_{0}_Value = value;", memberName);
						}
						else
						{
							writer.WriteLine(@"				_{0}_Modified = true;
				_{0}_Value = value;", memberName);
						}

						writer.WriteLine("			}");
					}
					writer.WriteLine("		}");
				}

				writer.WriteLine("	}");
			}
		}

		static string MakeCSharpName(string name)
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
				for (int i = 0; i < reader.FieldCount; i++)
					yield return new ColumnInfo { Index = i, Name = reader.GetName(i), Type = reader.GetFieldType(i) };
			}
		}

		public class ColumnInfo
		{
			public int Index;
			public string Name;
			public Type Type;
			public bool IsKey;
			public bool IsComputed;
			public bool IsIdentity;
		}
	}
}
