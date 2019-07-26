using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataBrowser.Actions;

namespace DataBrowser.Nodes
{
	abstract class Node
	{
		public virtual bool HasNodes
		{
			get
			{
				return false;
			}
		}

		public virtual IEnumerable<Node> Nodes
		{
			get
			{
				yield break;
			}
		}

		public virtual IEnumerable<string> Actions
		{
			get
			{
				yield break;
			}
		}

		public virtual ActionResult Action(string action)
		{
			return null;
		}
	}
}
