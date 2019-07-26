using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using DataBrowser.Actions;

namespace DataBrowser.Nodes
{
	class MultipleJoinedTableNode : Node
	{
		public string ConstraintSchema { get; set; }
		public string ConstraintName { get; set; }
		public string Schema { get; set; }
		public string Name { get; set; }
		public string TableSchema { get; set; }
		public string Table { get; set; }
		public Tuple<string, string>[] Columns { get; set; }
		public string[] TableSchemas { get; set; }
		public string[] Tables { get; set; }
		public Tuple<string, string>[][] TableColumns { get; set; }
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

					using (var command = new SqlCommand("select COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH from INFORMATION_SCHEMA.COLUMNS where TABLE_SCHEMA = @schema and TABLE_NAME = @table", connection))
					{
						command.Parameters.Add("@table", SqlDbType.VarChar, 255).Value = Table;
						command.Parameters.Add("@schema", SqlDbType.VarChar, 255).Value = TableSchema;

						using (var reader = command.ExecuteReader())
						{
							foreach (var record in reader.Cast<IDataRecord>())
								yield return new JoinedColumnNode { Name = record.GetString(0), Schema = TableSchema, Type = record.GetString(1), Length = record.IsDBNull(2) ? (int?)null : (int?)record.GetInt32(2), Table = Table, Database = Database, Server = Server };
						}
					}

					for (int table = 0; table < Tables.Length; table++)
					{
						using (var command = new SqlCommand("select COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH from INFORMATION_SCHEMA.COLUMNS where TABLE_SCHEMA = @schema and TABLE_NAME = @table", connection))
						{
							command.Parameters.Add("@table", SqlDbType.VarChar, 255).Value = Tables[table];
							command.Parameters.Add("@schema", SqlDbType.VarChar, 255).Value = TableSchemas[table];

							using (var reader = command.ExecuteReader())
							{
								foreach (var record in reader.Cast<IDataRecord>())
									yield return new JoinedColumnNode { Name = record.GetString(0), Schema = TableSchemas[table], Type = record.GetString(1), Length = record.IsDBNull(2) ? (int?)null : (int?)record.GetInt32(2), Table = Tables[table], Database = Database, Server = Server };
							}
						}
					}

					using (var command = new SqlCommand("select COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH from INFORMATION_SCHEMA.COLUMNS where TABLE_SCHEMA = @schema and TABLE_NAME = @table", connection))
					{
						command.Parameters.Add("@table", SqlDbType.VarChar, 255).Value = Name;
						command.Parameters.Add("@schema", SqlDbType.VarChar, 255).Value = Schema;

						using (var reader = command.ExecuteReader())
						{
							foreach (var record in reader.Cast<IDataRecord>())
								yield return new JoinedColumnNode { Name = record.GetString(0), Schema = Schema, Type = record.GetString(1), Length = record.IsDBNull(2) ? (int?)null : (int?)record.GetInt32(2), Table = Name, Database = Database, Server = Server };
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
					return new ExecuteQuery
					{
						CommandText = "select * from [" + TableSchema + "].[" + Table + "]" +
							string.Join(" ", Enumerable.Range(0, Tables.Length).Select(t => " join [" + TableSchemas[t] + "].[" + Tables[t] + "] on " +
								string.Join(" and ", Enumerable.Range(0, TableColumns[t].Length).Select(c => "[" + TableSchemas[t] + "].[" + Tables[t] + "].[" + TableColumns[t][c].Item1 + "] = [" + TableSchemas[t] + "].[" + TableSchemas[t] + "].[" + TableColumns[t][c].Item2 + "]")))) +
							" join [" + Schema + "].[" + Name + "] on " +
								(Tables.Any() ?
									string.Join(" and ", Columns.Select(c => "[" + TableSchemas.Last() + "].[" + Tables.Last() + "].["+ c.Item1 + "] = [" + Schema + "].[" + Name + "].[" + c.Item2 + "]")) :
									string.Join(" and ", Columns.Select(c => "[" + TableSchema + "].[" + Table + "].["+ c.Item1 + "] = [" + Schema + "].[" + Name + "].[" + c.Item2 + "]")))
									, ConnectionString = builder.ToString() };
			}

			return null;
		}

		public override string ToString()
		{
			//return Schema + "." + Name;
			//return Name + " (" + Schema + ")";
			//return Name;
			return Name + " (" + string.Join(", ", Columns.Select(c => c.Item1 + "=" + c.Item2)) + ")";
			//return Name + " (" + Table + "." + string.Join(", ", Columns.Select(c => c.Item1)) + ")";
		}
	}
}
