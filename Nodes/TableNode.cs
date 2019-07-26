using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using DataBrowser.Actions;

namespace DataBrowser.Nodes
{
	class TableNode : Node
	{
		public string Schema { get; set; }
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

					using (var command = new SqlCommand(@"
select rc.CONSTRAINT_SCHEMA, rc.CONSTRAINT_NAME, kcu.ORDINAL_POSITION, kcu.COLUMN_NAME, kcu2.TABLE_SCHEMA, kcu2.TABLE_NAME, kcu2.COLUMN_NAME
from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS as rc 
join INFORMATION_SCHEMA.KEY_COLUMN_USAGE as kcu 
    on kcu.CONSTRAINT_CATALOG = rc.CONSTRAINT_CATALOG  
    and kcu.CONSTRAINT_SCHEMA = rc.CONSTRAINT_SCHEMA 
    and kcu.CONSTRAINT_NAME = rc.CONSTRAINT_NAME 
join INFORMATION_SCHEMA.KEY_COLUMN_USAGE as kcu2 
    on kcu2.CONSTRAINT_CATALOG = rc.UNIQUE_CONSTRAINT_CATALOG  
    and kcu2.CONSTRAINT_SCHEMA = rc.UNIQUE_CONSTRAINT_SCHEMA 
    and kcu2.CONSTRAINT_NAME = rc.UNIQUE_CONSTRAINT_NAME 
    and kcu2.ORDINAL_POSITION = kcu.ORDINAL_POSITION
where kcu.TABLE_SCHEMA = @schema and kcu.TABLE_NAME = @table
union select rc.CONSTRAINT_SCHEMA, rc.CONSTRAINT_NAME, kcu2.ORDINAL_POSITION, kcu2.COLUMN_NAME, kcu.TABLE_SCHEMA, kcu.TABLE_NAME, kcu.COLUMN_NAME
from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS as rc 
join INFORMATION_SCHEMA.KEY_COLUMN_USAGE as kcu 
    on kcu.CONSTRAINT_CATALOG = rc.CONSTRAINT_CATALOG  
    and kcu.CONSTRAINT_SCHEMA = rc.CONSTRAINT_SCHEMA 
    and kcu.CONSTRAINT_NAME = rc.CONSTRAINT_NAME 
join INFORMATION_SCHEMA.KEY_COLUMN_USAGE as kcu2 
    on kcu2.CONSTRAINT_CATALOG = rc.UNIQUE_CONSTRAINT_CATALOG  
    and kcu2.CONSTRAINT_SCHEMA = rc.UNIQUE_CONSTRAINT_SCHEMA 
    and kcu2.CONSTRAINT_NAME = rc.UNIQUE_CONSTRAINT_NAME 
    and kcu2.ORDINAL_POSITION = kcu.ORDINAL_POSITION
where kcu2.TABLE_SCHEMA = @schema and kcu2.TABLE_NAME = @table
", connection))
					{
						command.Parameters.Add("@table", SqlDbType.VarChar, 255).Value = Name;
						command.Parameters.Add("@schema", SqlDbType.VarChar, 255).Value = Schema;
						command.CommandTimeout = 0;

						using (var reader = command.ExecuteReader())
						{
							foreach (var constraint in reader
								.Cast<IDataRecord>()
								.Select(r => new { ConstraintSchema = r.GetString(0), ConstraintName = r.GetString(1), OrdinalPosition = r.GetInt32(2), Column = r.GetString(3), Schema2 = r.GetString(4), Table2 = r.GetString(5), Column2 = r.GetString(6) })
								.GroupBy(c => new { c.ConstraintSchema, c.ConstraintName })
								.OrderBy(g => g.First().Schema2)
								.ThenBy(g => g.First().Table2))
								yield return new JoinedTableNode
								{
									ConstraintSchema = constraint.Key.ConstraintSchema,
									ConstraintName = constraint.Key.ConstraintName,
									Name = constraint.First().Table2,
									Schema = constraint.First().Schema2,
									Table = Name,
									TableSchema = Schema,
									Columns = constraint.Select(p => new Tuple<string, string>(p.Column, p.Column2)).ToArray(),
									Database = Database,
									Server = Server
								};
						}
					}

					using (var command = new SqlCommand("select COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, IS_NULLABLE from INFORMATION_SCHEMA.COLUMNS where TABLE_SCHEMA = @schema and TABLE_NAME = @table", connection))
					{
						command.Parameters.Add("@table", SqlDbType.VarChar, 255).Value = Name;
						command.Parameters.Add("@schema", SqlDbType.VarChar, 255).Value = Schema;

						using (var reader = command.ExecuteReader())
						{
							foreach (var record in reader.Cast<IDataRecord>())
								yield return new ColumnNode { Name = record.GetString(0), Schema = Schema, Type = record.GetString(1), Length = record.IsDBNull(2) ? (int?)null : (int?)record.GetInt32(2), Nullable = record.GetString(3) == "YES", Table = Name, Database = Database, Server = Server };
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
				yield return "Generate XML";
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
					return new ExecuteQuery { CommandText = "select * from [" + Schema + "].[" + Name + "]", ConnectionString = builder.ToString() };

				case "Generate XML":
					return new GenerateXml { CommandText = "select * from [" + Schema + "].[" + Name + "]", ConnectionString = builder.ToString(), TableName = Schema + "." + Name };
			}

			return null;
		}

		public override string ToString()
		{
			//return Schema + "." + Name;
			//return Name + " (" + Schema + ")";
			return Name;
		}
	}
}