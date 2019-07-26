using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataBrowser.Actions;

namespace DataBrowser.Nodes
{
	class ServersNode : Node
	{
		public List<string> Servers = new List<string>();

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
				return Servers.Select(s => new ServerNode { Name = s });
			}
		}

		public override IEnumerable<string> Actions
		{
			get
			{
				yield return "Add Server";
			}
		}

		public override ActionResult Action(string action)
		{
			switch (action)
			{
				case "Add Server":
					Servers.Add("Server");
					return new AddNode { Node = new ServerNode { Name = "Server" } };
			}

			return null;
		}

		public override string ToString()
		{
			return "Servers";
		}
	}
}
