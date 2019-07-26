using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using DataBrowser.Actions;
using DataBrowser.Nodes;

namespace DataBrowser
{
	public partial class DataBrowserWindow : Form
	{
		private string ConnectionString;
		private string TableName;

		public DataBrowserWindow()
		{
			InitializeComponent();

			treeView.Nodes.Add(TreeNode(new ServersNode { Servers = new List<string> { Environment.MachineName } }));
		}

		private void contextMenuStrip_Opening(object sender, CancelEventArgs e)
		{
			contextMenuStrip.Items.Clear();

			contextMenuStrip.Items.AddRange(((Node)treeView.SelectedNode.Tag).Actions.Select(a => new ToolStripMenuItem { Text = a }).ToArray());
		}

		private void contextMenuStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
		{
			var result = ((Node)treeView.SelectedNode.Tag).Action(e.ClickedItem.Text);

			if (result is AddNode)
				treeView.SelectedNode.Nodes.Add(new TreeNode { Text = ((AddNode)result).Node.ToString(), Tag = ((AddNode)result).Node });
			else if (result is ExecuteQuery)
			{
				textBox.Text = ((ExecuteQuery)result).CommandText;
				ConnectionString = ((ExecuteQuery)result).ConnectionString;

				ExecuteQuery();
			}
			else if (result is GenerateXml)
			{
				textBox.Text = ((GenerateXml)result).CommandText;
				ConnectionString = ((GenerateXml)result).ConnectionString;
				TableName = ((GenerateXml)result).TableName;

				GenerateXml();
			}
		}

		private TreeNode TreeNode(Node node)
		{
			var treeNode = new TreeNode(node.ToString());

			treeNode.Tag = node;

			if (node.HasNodes)
				treeNode.Nodes.Add(new TreeNode("Loading..."));

			return treeNode;
		}

		private void treeView_AfterExpand(object sender, TreeViewEventArgs e)
		{
			Application.DoEvents();

			treeView.BeginUpdate();

			e.Node.Nodes.Clear();

			e.Node.Nodes.AddRange(((Node)e.Node.Tag).Nodes.Select(n => TreeNode(n)).ToArray());

			treeView.EndUpdate();
		}

		private void treeView_AfterCollapse(object sender, TreeViewEventArgs e)
		{
			e.Node.Nodes.Clear();

			e.Node.Nodes.Add("Loading...");
		}

		private void dataGridView_DataError(object sender, DataGridViewDataErrorEventArgs e)
		{
			e.Cancel = true;
		}

		private void executeSQLToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (tabControl.SelectedTab == dataTabPage)
				ExecuteQuery();
			else
				GenerateXml();
		}

		private void ExecuteQuery()
		{
			try
			{
				var adapter = new SqlDataAdapter(textBox.Text, ConnectionString);

				var table = new DataTable();

				adapter.Fill(table);

				dataGridView.DataSource = table;

				tabControl.SelectedTab = dataTabPage;
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message);

				dataGridView.DataSource = null;
			}
		}

		private void GenerateXml()
		{
			try
			{
				var adapter = new SqlDataAdapter(textBox.Text, ConnectionString);

				var dataSet = new DataSet("dataset");

				var table = new DataTable(TableName);

				dataSet.Tables.Add(table);

				adapter.Fill(table);

				foreach (DataColumn column in table.Columns)
					column.ColumnMapping = MappingType.Attribute;

				using (var writer = new StringWriter())
				using (var xmlWriter = new XmlTextWriter(writer))
				{
					xmlWriter.Indentation = 1;
					xmlWriter.IndentChar = '\t';
					xmlWriter.Formatting = Formatting.Indented;

					table.WriteXml(xmlWriter);

					xmlTextBox.Text = writer.ToString();
				}

				tabControl.SelectedTab = xmlTabPage;
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message);

				dataGridView.DataSource = null;
			}
		}

		private void undoToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (ActiveControl == splitContainer1)
			{
				if (splitContainer1.ActiveControl == splitContainer2)
				{
					if (splitContainer2.ActiveControl == textBox)
						textBox.Undo();
				}
			}
		}

		private void redoToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (ActiveControl == splitContainer1)
			{
				if (splitContainer1.ActiveControl == splitContainer2)
				{
					if (splitContainer2.ActiveControl == textBox)
						textBox.Undo();
				}
			}
		}

		private void cutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (ActiveControl == splitContainer1)
			{
				if (splitContainer1.ActiveControl == splitContainer2)
				{
					if (splitContainer2.ActiveControl == textBox)
						textBox.Cut();
				}
			}
		}

		private void copyToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (ActiveControl == splitContainer1)
			{
				if (splitContainer1.ActiveControl == splitContainer2)
				{
					if (splitContainer2.ActiveControl == textBox)
						textBox.Copy();

					else if (splitContainer2.ActiveControl == dataGridView)
						Clipboard.SetText(string.Join(Environment.NewLine, dataGridView.SelectedCells.Cast<DataGridViewCell>().GroupBy(c => c.RowIndex).Select(r => string.Join("\t", r.Select(c => c.FormattedValue.ToString())))), TextDataFormat.Text);
				}
			}
		}

		private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (ActiveControl == splitContainer1)
			{
				if (splitContainer1.ActiveControl == splitContainer2)
				{
					if (splitContainer2.ActiveControl == textBox)
						textBox.Paste();
				}
			}
		}

		private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (ActiveControl == splitContainer1)
			{
				if (splitContainer1.ActiveControl == splitContainer2)
				{
					if (splitContainer2.ActiveControl == textBox)
						textBox.SelectAll();
				}
			}
		}
	}
}