using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace DataBrowser.Nodes
{
	class ServerNode : Node
	{
		public string Name { get; set; }

		public override string ToString()
		{
			return Name;
		}

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

				builder.DataSource = Name;
				builder.InitialCatalog = "master";
				builder.IntegratedSecurity = true;

				using (var connection = new SqlConnection(builder.ToString()))
				{
					connection.Open();

					using (var command = new SqlCommand("sp_databases", connection))
					using (var reader = command.ExecuteReader())
						return reader.Cast<IDataRecord>().Select(r => new DatabaseNode { Name = r.GetString(0), Server = Name }).ToArray();
				}
			}
		}
	}
}