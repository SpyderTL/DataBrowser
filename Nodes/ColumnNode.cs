using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using DataBrowser.Actions;

namespace DataBrowser.Nodes
{
	class ColumnNode : Node
	{
		public string Name { get; set; }
		public string Schema { get; set; }
		public string Type { get; set; }
		public int? Length { get; set; }
		public bool Nullable { get; set; }
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

				switch(Type)
				{
					case "char":
					case "varchar":
					case "nvarchar":
						using (var connection = new SqlConnection(builder.ToString()))
						{
							connection.Open();

							using (var command = new SqlCommand("select distinct substring([" + Name + "], 1, 1) from [" + Schema + "].[" + Table + "] order by substring([" + Name + "], 1, 1)", connection))
							{
								using (var reader = command.ExecuteReader())
								{
									foreach (var record in reader.Cast<IDataRecord>())
									{
										if(record.IsDBNull(0))
											yield return new DistinctValueNode { Value = null, Column = Name, Schema = Schema, Table = Table, Database = Database, Server = Server };
										else
											yield return new StartsWithNode { Value = record.GetString(0), Column = Name, Schema = Schema, Table = Table, Database = Database, Server = Server };
									}
								}
							}
						}
						yield break;

					default:
						using (var connection = new SqlConnection(builder.ToString()))
						{
							connection.Open();

							using (var command = new SqlCommand("select distinct [" + Name + "] from [" + Schema + "].[" + Table + "] order by [" + Name + "]", connection))
							{
								using (var reader = command.ExecuteReader())
								{
									foreach (var record in reader.Cast<IDataRecord>())
									{
										yield return new DistinctValueNode { Value = record[0], Column = Name, Schema = Schema, Table = Table, Database = Database, Server = Server };
									}
								}
							}
						}
						yield break;
				}
			}
		}

		public override IEnumerable<string> Actions
		{
			get
			{
				yield return "SELECT";
				yield return "SELECT DISTINCT";
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
				case "SELECT":
					return new ExecuteQuery { CommandText = "select " + Name + " from [" + Schema + "].[" + Table + "]", ConnectionString = builder.ToString() };

				case "SELECT DISTINCT":
					return new ExecuteQuery { CommandText = "select distinct(" + Name + ") from [" + Schema + "].[" + Table + "]", ConnectionString = builder.ToString() };
			}

			return null;
		}

		public override string ToString()
		{
			if(Length == null)
				return Name + " " + Type + (Nullable ? " null" : " not null");

			if (Length == -1)
				return Name + " " + Type + "(max)" + (Nullable ? " null" : " not null");

			return Name + " " + Type + "(" + Length.ToString() + ")" + (Nullable ? " null" : " not null");
		}
	}
}
