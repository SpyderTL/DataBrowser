using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace DataBrowser.Nodes
{
	class DatabaseNode : Node
	{
		public string Name { get; set; }
		public string Server { get; set; }

		public override bool HasNodes
		{
			get
			{
				return true;
			}
		}

		public override IEnumerable<Node> Nodes
		{
			get
			{
				var builder = new SqlConnectionStringBuilder();

				builder.DataSource = Server;
				builder.InitialCatalog = Name;
				builder.IntegratedSecurity = true;

				using (var connection = new SqlConnection(builder.ToString()))
				{
					connection.Open();

					using (var command = new SqlCommand("select SCHEMA_NAME from INFORMATION_SCHEMA.SCHEMATA where SCHEMA_OWNER = 'dbo'", connection))
					using (var reader = command.ExecuteReader())
						return reader.Cast<IDataRecord>().Select(r => new SchemaNode { Name = r.GetString(0), Database = Name, Server = Server }).ToArray();
				}
			}
		}

		public override string ToString()
		{
			return Name;
		}
	}
}