using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using DataBrowser.Actions;

namespace DataBrowser.Nodes
{
	class SchemaNode : Node
	{
		public string Name { get; set; }
		public string Server { get; set; }
		public string Database { get; set; }

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
				builder.InitialCatalog = Database;
				builder.IntegratedSecurity = true;

				using (var connection = new SqlConnection(builder.ToString()))
				{
					connection.Open();

					using (var command = new SqlCommand("select TABLE_NAME from INFORMATION_SCHEMA.TABLES where TABLE_SCHEMA = @schema order by TABLE_NAME", connection))
					{
						command.Parameters.Add("@schema", SqlDbType.VarChar, 255).Value = Name;

						using (var reader = command.ExecuteReader())
						{
							foreach (var record in reader.Cast<IDataRecord>())
								yield return new TableNode { Name = record.GetString(0), Schema = Name, Database = Database, Server = Server };
						}
					}
				}
			}
		}

		public override string ToString()
		{
			return Name;
		}
	}
}