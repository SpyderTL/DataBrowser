using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using DataBrowser.Actions;

namespace DataBrowser.Nodes
{
	class DistinctValueNode : Node
	{
		public object Value { get; set; }
		public string Column { get; set; }
		public string Schema { get; set; }
		public string Table { get; set; }
		public string Database { get; set; }
		public string Server { get; set; }

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
					if(Value == null)
						return new ExecuteQuery { CommandText = "select * from [" + Schema + "].[" + Table + "] where [" + Column + "] is null", ConnectionString = builder.ToString() };
					else
						return new ExecuteQuery { CommandText = "select * from [" + Schema + "].[" + Table + "] where [" + Column + "] = '" + Value + "'", ConnectionString = builder.ToString() };
			}

			return null;
		}

		public override string ToString()
		{
			return Value == null ? "(null)" :
				Value.ToString() == string.Empty ? "(blank)" :
				Value.ToString() == " " ? "(space)" :
				Value.ToString();
		}
	}
}
