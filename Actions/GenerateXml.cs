﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBrowser.Actions
{
	class GenerateXml : ActionResult
	{
		public string CommandText { get; set; }
		public string ConnectionString { get; set; }
		public string TableName { get; set; }
	}
}
