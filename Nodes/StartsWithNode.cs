using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using DataBrowser.Actions;

namespace DataBrowser.Nodes
{
	class StartsWithNode : Node
	{
		public string Value { get; set; }
		public string Column { get; set; }
		public string Schema { get; set; }
		public string Table { get; set; }
		public string Database { get; set; }
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
				builder.InitialCatalog = Database;
				builder.IntegratedSecurity = true;

				using (var connection = new SqlConnection(builder.ToString()))
				{
					connection.Open();

					using (var command = new SqlCommand("select distinct [" + Column + "] from [" + Schema + "].[" + Table + "] where substring([" + Column + "], 1, 1) = @value order by [" + Column + "]", connection))
					{
						command.Parameters.Add("@value", SqlDbType.VarChar, 1).Value = Value;

						using (var reader = command.ExecuteReader())
						{
							foreach (var record in reader.Cast<IDataRecord>())
							{
								yield return new DistinctValueNode { Value = record[0], Column = Column, Schema = Schema, Table = Table, Database = Database, Server = Server };
							}
						}
					}
				}
			}
		}

		public override IEnumerable<string> Actions
		{
			get
			{
				yield return "SELECT *";
			}
		}

		public override Actions.ActionResult Action(string action)
		{
			var builder = new SqlConnectionStringBuilder();

			builder.DataSource = Server;
			builder.InitialCatalog = Database;
			builder.IntegratedSecurity = true;

			switch (action)
			{
				case "SELECT *":
					if (Value == null)
						return new ExecuteQuery { CommandText = "select * from [" + Schema + "].[" + Table + "] where [" + Column + "] is null", ConnectionString = builder.ToString() };
					else if (Value == string.Empty)
						return new ExecuteQuery { CommandText = "select * from [" + Schema + "].[" + Table + "] where [" + Column + "] = ''", ConnectionString = builder.ToString() };
					else
						return new ExecuteQuery { CommandText = "select * from [" + Schema + "].[" + Table + "] where [" + Column + "] like '" + Value + "%'", ConnectionString = builder.ToString() };
			}

			return null;
		}

		public override string ToString()
		{
			return Value == null ? "(null)" : 
				Value == string.Empty ? "(blank)" :
				Value == " " ? "(space)" :
				Value.ToString();
		}
	}
}
