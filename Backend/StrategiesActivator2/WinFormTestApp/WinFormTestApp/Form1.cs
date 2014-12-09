using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Reflection;
using System.Xml;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace WinFormTestApp
{
	using StasEzService;

	public partial class Form1 : Form
	{
		private string SelectedMethod { get; set; }
		private string CurrentFile { get; set; }

		private const string MATCHES_FOUND_TEXT = "Matches were found";
		private const string DEFAULT_ENDPOINT_IP_TEXT = "EndPoint IP: ";
		private static bool isLoaded = false;
		// private string connectionString=

		DataTable summaryDataTable;//= new DataTable();
		private string SelectedEndpoint;

		public Form1()
		{
			InitializeComponent();
			this.Location = new Point(30, 10);

			summaryDataTable.Columns.Add("Transaction Name", typeof(string));
			summaryDataTable.Columns.Add("Result", typeof(string));
			summaryDataTable.Columns.Add("Error Message", typeof(string));
			CurrentFile = null;

			//summaryGrid.DataSource = summaryDataTable;
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			var service = new EzServiceClient();
			Type type = typeof(EzServiceClient);
			System.Reflection.MethodInfo[] methods = type.GetMethods();
			var methodsStr = new List<string>(30);
			methodsStr.AddRange(methods.Select(method => method.Name));
			methodsStr.Sort();
			ddlMethods.DataSource = methodsStr;

			//  ddlMethods.Text = "Name";
			//  ddlMethods.ValueMember = "Name";
			ddlMethods.PerformLayout();

			AutoCompleteStringCollection AutoCompleteMethods = new AutoCompleteStringCollection();
			for (int i = 0; i < methods.Length; i++)
			{
				AutoCompleteMethods.Add(methods[i].Name);
			}

			ddlMethods.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
			ddlMethods.AutoCompleteSource = AutoCompleteSource.ListItems;
			ddlMethods.AutoCompleteCustomSource = AutoCompleteMethods;

			SelectedMethod = ddlMethods.SelectedValue.ToString();

			//lblFbnclient.Text = TestMethod.Fbnclient.Endpoint.ListenUri.ToString();

			imgLoader.Visible = true;
			pictureBox1.Visible = false;
			imgLoader.Text = "";
			imgLoader.Refresh();
			var endpoints = LoadWindowHelper.LoadEndpointsFromConfig();
			EndPoint.Text = DEFAULT_ENDPOINT_IP_TEXT + endpoints[0].EndPointPath.Host;
		}

		private string getConnectionString(string server)
		{

			switch (server)
			{
				default:
					return @"Server=localhost;Database=ezbob;User Id=stas;Password=ezbobuser;";

			}
		}

		private void ddlMethods_SelectedIndexChanged(object sender, EventArgs e)
		{
			SelectedMethod = ddlMethods.SelectedValue.ToString();
			CurrentFile = null;
		}

		private void AddControl(Control ctl)
		{

			tableLayoutPanel6.RowCount += 1;
			tableLayoutPanel6.RowStyles.Add(new RowStyle(SizeType.AutoSize));
			ctl.Dock = DockStyle.Top;
			tableLayoutPanel6.Controls.Add(ctl, 0, tableLayoutPanel6.RowCount - 1);
		}
		private void btnLoadReq_Click(object sender, EventArgs e)
		{

			Progress("Processing", true);
			CurrentFile = null;

			try
			{
				if (tableLayoutPanel6.RowCount > 2)
				{
					int rows = tableLayoutPanel6.RowCount;
					for (int i = 3; i <= rows; i++)
					{
						tableLayoutPanel6.Controls.RemoveByKey("param" + (i - 3));
						tableLayoutPanel6.RowCount -= 1;
					}
				}
				txtReq.Text = "";
				Type type = typeof(EzServiceClient);
				MethodInfo method
					 = type.GetMethod(SelectedMethod);

				object[] args = new object[] { };
				int pPos = 0;
				string pType = null;
				string pName = null;
				foreach (ParameterInfo pParameter in method.GetParameters())
				{
					//Position of parameter in method
					pPos = pParameter.Position;

					//Name of parameter type
					pType = pParameter.ParameterType.Name;

					//Name of parameter
					pName = pParameter.Name;

					txtReq.Text += string.Format("pos:{0}{3}type:{1}{3}name:{2}{3}{3}", pPos, pType, pName, Environment.NewLine);

					AddControl(new TextBox() { Name = "param" + pPos, Text = pName });
				}

				HighlightColors.HighlightRTF(txtReq);

			}
			catch (TargetInvocationException ex)
			{
				Exception inner = ex.InnerException;
				if (inner.GetType() == typeof(NotImplementedException))
				{
					txtException.Text = (SelectedMethod + "  Not Implemented\n");
				}
				else if (inner.GetType() == typeof(NullReferenceException))
				{
					txtException.Text = (SelectedMethod + " test method is missing\n ");
				}
				else if (inner.GetType() == typeof(TimeoutException))
				{
					txtException.Text = (SelectedMethod + " timeout exception \n");
				}
				else
				{
					txtException.Text = SelectedMethod + "\n" + ex + "\n" + ex.StackTrace + "\n";
				}
			}
			catch (Exception ex)
			{
				txtException.Text = ex.Message;
			}
			finally
			{
				Progress("Finished");
			}
		}

		private void btnTestCustom_Click(object sender, EventArgs e)
		{
			if (this.ValidateChildren())
			{
				UpdateProgress("Processing...", true);
				string hdr = string.Empty;
				string res = string.Empty;
				Task T = Task.Factory.StartNew(() =>
					{
						try
						{
							TestMethod.CreateProxy(SelectedEndpoint);
							string req = txtReq.Text;
							// HighlightColors.HighlightRTF(txtReq);
							Type t = Type.GetType("WinFormTestApp.TestMethod");
							MethodInfo method = t.GetMethod("test" + SelectedMethod, BindingFlags.Static | BindingFlags.Public);
							object[] args = new object[] { req };
							res = (string)(method.Invoke(null, args));
							hdr = (string)args[1];
							WrapUpTestCustom(res, hdr);
						}
						catch (TargetInvocationException ex)
						{
							Exception inner = ex.InnerException;
							if (inner.GetType() == typeof(NotImplementedException))
							{
								UpdateExceptionLog(SelectedMethod + "  Not Implemented\n");
							}
							else if (inner.GetType() == typeof(NullReferenceException))
							{
								UpdateExceptionLog(SelectedMethod + " test method is missing\n ");
							}
							else if (inner.GetType() == typeof(TimeoutException))
							{
								UpdateExceptionLog(SelectedMethod + " timeout exception \n");
							}
							else
							{
								UpdateExceptionLog(SelectedMethod + "\n" + ex + "\n" + ex.StackTrace + "\n");
							}
						}
						catch (Exception ex)
						{
							UpdateExceptionLog(ex.Message + "\n" + ex.InnerException + "\n" + ex.StackTrace);
						}
						finally
						{
							Progress("Finished");
							summaryDataTable.Rows.Add(SelectedMethod, "OK");
							SummaryGridUpdate(summaryDataTable);
						}
					});
			}
		}

		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (summaryDataTable != null)
			{
				xmlHelper.saveSummary(summaryDataTable, "summary" + DateTime.Now.ToFileTime().ToString() + ".txt");
			}
			if (_rtbTestAllLog.Text != string.Empty)
			{
				HighlightColors.HighlightRTF(_rtbTestAllLog);
				_rtbTestAllLog.SaveFile("testAll" + DateTime.Now.ToFileTime().ToString() + ".rtf");
			}

		}

		private void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Form1_FormClosing(sender, null);
		}

		private void openToolStripMenuItem_Click(object sender, EventArgs e)
		{
			OpenFileDialog dlg = new OpenFileDialog()
			{
				DefaultExt = "xml",
				InitialDirectory = Path.Combine(Application.StartupPath, "reqs"),

			};

			dlg.Title = "Open Request";
			dlg.Filter = "xml files (*.xml)|*.xml";

			if (dlg.ShowDialog() == DialogResult.OK)
			{
				using (StreamReader sr = new StreamReader(dlg.FileName, true))
				{
					XDocument xmlDoc = XDocument.Load(sr);

					CurrentFile = dlg.FileName;
					StringWriter sw = new StringWriter();
					XmlTextWriter xw = new XmlTextWriter(sw);
					xw.Formatting = Formatting.Indented;
					xmlDoc.WriteTo(xw);
					txtReq.Text = sw.ToString();
					HighlightColors.HighlightRTF(txtReq);

				}
			}

			dlg.Dispose();
		}

		private void btnSave_Click(object sender, EventArgs e)
		{
			saveToolStripMenuItem_Click(sender, e);
		}

		private void btnLoadFile_Click(object sender, EventArgs e)
		{
			openToolStripMenuItem_Click(sender, e);
		}

		private void saveToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (CurrentFile != null)
			{
				using (StreamWriter outfile = new StreamWriter(CurrentFile))
				{
					outfile.Write(txtReq.Text);
				}
			}
			else
			{
				saveAsToolStripMenuItem_Click(sender, e);
			}
		}

		private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// Displays a SaveFileDialog so the user can save the Image
			// assigned to Button2.
			string method = ddlMethods.SelectedValue.ToString();
			SaveFileDialog dlg = new SaveFileDialog()
			{
				Filter = "xml files (*.xml)|*.xml",
				Title = "Save Request to xml file",
				InitialDirectory = Path.Combine(Application.StartupPath, "reqs"),
				FileName = method + DateTime.Today.Year + DateTime.Today.Month.ToString("00") + DateTime.Today.Day.ToString("00"),
			};

			dlg.ShowDialog();

			// If the file name is not an empty string open it for saving.
			if (dlg.FileName != "")
			{
				// Saves the Image via a FileStream created by the OpenFile method.
				CurrentFile = dlg.FileName;
				using (StreamWriter outfile = new StreamWriter(dlg.OpenFile()))
				{
					outfile.Write(txtReq.Text);
				}
			}
		}

		private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			MessageBox.Show("ezbob team developed this tool for internal integration testing of host transactions", "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		private void testToolStripMenuItem_Click(object sender, EventArgs e)
		{
			btnTestCustom_Click(sender, e);
		}

		private void btnClear_Click(object sender, EventArgs e)
		{
			if (chkClearException.Checked)
			{
				txtException.Clear();
			}

			if (chkClearReq.Checked)
			{
				txtReq.Clear();
			}

			if (chkClearRes.Checked)
			{
				treeRes.Nodes.Clear();

			}
		}

		private void btnSearch_Click(object sender, EventArgs e)
		{
			int numOfResults = 0;
			numOfResults = FindNodeInTree(treeRes.Nodes, txtSearchTerm.Text, chkWholeWord.Checked);
			//lblMatches.Text = numOfResults.ToString() + " " + MATCHES_FOUND_TEXT;

		}

		private int FindNodeInTree(TreeNodeCollection nodes, string strSearchValue, bool wholeWord)
		{
			//   bool nodeFound = false;
			int numOfResults = 0;
			for (int iCount = 0; iCount < nodes.Count; iCount++)
			{
				if (wholeWord == false ? nodes[iCount].Text.ToLower().Contains(strSearchValue.ToLower()) : nodes[iCount].Text.ToLower().Equals(strSearchValue.ToLower()))
				{
					treeRes.SelectedNode = nodes[iCount];
					treeRes.SelectedNode.BackColor = Color.Yellow;
					treeRes.Select();
					numOfResults++;
					//nodeFound  = true;
				}
				else
				{
					nodes[iCount].BackColor = Color.White;
					// m_bNonodeFound = false;
				}

				//Recursively search the text in the child nodes
				numOfResults += FindNodeInTree(nodes[iCount].Nodes, strSearchValue, wholeWord);

			}
			return numOfResults;
		}

		public void UpdateList(string name, Status status)
		{
			if (_lvMethods.InvokeRequired)
				_lvMethods.BeginInvoke((Action)delegate()
				{
					if (status == Status.Processing)
						_lvMethods.Items.Add(name, 2);
					else if (status == Status.Success)
					{
						var item = SelectItem(name);
						_lvMethods.Items.Remove(item);
						item.ImageIndex = 1;
						_lvMethods.Items.Add(item);
					}
					else
					{
						var item = SelectItem(name);
						_lvMethods.Items.Remove(item);
						item.ImageIndex = 0;
						_lvMethods.Items.Add(item);
						//_lvMethods.Items.Remove(item);
						//_lvMethods.Items.Add(name, 1);
					}
				});
			else
				_lvMethods.Items.Add(name);
		}

		private ListViewItem SelectItem(string name)
		{
			var item = new ListViewItem();
			foreach (var i in _lvMethods.Items)
			{
				if ((i as ListViewItem).Text.Equals(name))
				{
					item = (i as ListViewItem);
				}
			}
			return item;
		}

		public void UpdateFault(string text)
		{
			if (faultOnly.InvokeRequired)
				faultOnly.BeginInvoke((Action)delegate() { faultOnly.AppendText(text); });
			else
				faultOnly.AppendText(text);
		}

		public void UpdateLog(string text)
		{
			if (_rtbTestAllLog.InvokeRequired)
				_rtbTestAllLog.BeginInvoke((Action)delegate() { _rtbTestAllLog.AppendText(text); });
			else
				_rtbTestAllLog.AppendText(text);
		}

		public void UpdateExceptionLog(string text)
		{
			if (txtException.InvokeRequired)
				txtException.BeginInvoke((Action)delegate() { txtException.AppendText(text); });
			else
				txtException.AppendText(text);
		}

		public void Progress(string progress, bool imgVisible = false)
		{
			if (imgLoader.InvokeRequired)
				imgLoader.BeginInvoke((Action)delegate()
				{
					UpdateProgress(progress, imgVisible);
				});
			else
				UpdateProgress(progress, imgVisible);
		}
		private void UpdateProgress(string progress, bool imgVisible)
		{
			imgLoader.Text = progress;
			pictureBox1.Visible = imgVisible;
			imgLoader.Refresh();
			pictureBox1.Refresh();
		}
		private void ChangeFont(Font f)
		{
			if (_rtbTestAllLog.InvokeRequired)
			{
				_rtbTestAllLog.BeginInvoke((Action)delegate()
				{
					_rtbTestAllLog.SelectionStart = _rtbTestAllLog.TextLength;
					_rtbTestAllLog.SelectionLength = 0;
					_rtbTestAllLog.SelectionFont = f;
				});
			}
			else
			{
				_rtbTestAllLog.SelectionStart = _rtbTestAllLog.TextLength;
				_rtbTestAllLog.SelectionLength = 0;
				_rtbTestAllLog.SelectionFont = f;
			}
		}

		private void ChangeFontFault(Font f)
		{
			if (faultOnly.InvokeRequired)
			{
				faultOnly.BeginInvoke((Action)delegate()
				{
					faultOnly.SelectionStart = _rtbTestAllLog.TextLength;
					faultOnly.SelectionLength = 0;
					faultOnly.SelectionFont = f;
				});
			}
			else
			{
				faultOnly.SelectionStart = _rtbTestAllLog.TextLength;
				faultOnly.SelectionLength = 0;
				faultOnly.SelectionFont = f;
			}
		}

		public void SummaryGridUpdate(DataTable dt)
		{
			if (summaryGrid.InvokeRequired)
			{
				summaryGrid.BeginInvoke((Action)delegate()
				{
					summaryGrid.DataSource = summaryDataTable;
					summaryGrid.Update();
				});
			}
			else
			{
				summaryGrid.DataSource = summaryDataTable;
				summaryGrid.Update();
			}
		}
		public void WrapUpTestAll()
		{
			if (_rtbTestAllLog.InvokeRequired)
			{
				_rtbTestAllLog.BeginInvoke((Action)delegate
				{
					HighlightColors.HighlightRTF(_rtbTestAllLog);
					_rtbTestAllLog.SaveFile("testAll" + DateTime.Now.ToFileTime().ToString() + ".rtf");
				});
			}
			else
			{
				HighlightColors.HighlightRTF(_rtbTestAllLog);
				_rtbTestAllLog.SaveFile("testAll" + DateTime.Now.ToFileTime().ToString() + ".rtf");
			}
		}

		public void WrapUpTestCustom(string res, string hdr)
		{
			if (treeRes.InvokeRequired)
			{
				treeRes.BeginInvoke((Action)delegate
				{
					treeRes.Nodes.Clear();
					xmlHelper.fillTree(res, treeRes);
					txtRes.Text = res;
					HighlightColors.HighlightRTF(txtRes);
				});
			}
			else
			{
				treeRes.Nodes.Clear();
				xmlHelper.fillTree(res, treeRes);
				txtRes.Text = res;
				HighlightColors.HighlightRTF(txtRes);
			}
		}

		private void _cbEndpoints_Validating(object sender, CancelEventArgs e)
		{
			string error = null;
			if (SelectedEndpoint == null)
			{
				error = "Please select endpoint";
				e.Cancel = true;
			}
			_epEndpoint.SetError((Control)sender, error);
		}

		private void ddlEndpoints_SelectedIndexChanged(object sender, EventArgs e)
		{
			SelectedEndpoint = ((ComboBox)sender).SelectedValue.ToString();
			var endpoints = LoadWindowHelper.LoadEndpointsFromConfig();
			foreach (var endpoint in endpoints)
			{
				if (SelectedEndpoint.Equals(endpoint.EndPointName))
				{
					EndPoint.Text = DEFAULT_ENDPOINT_IP_TEXT + endpoint.EndPointPath.Host;
				}
			}
		}

		private void chkClearAll_CheckedChanged(object sender, EventArgs e)
		{
			chkClearReq.Checked = chkClearAll.Checked;
			chkClearRes.Checked = chkClearAll.Checked;
			chkClearException.Checked = chkClearAll.Checked;
			chkClearReq.Checked = chkClearAll.Checked;

		}

		private void tableLayoutPanel6_Paint(object sender, PaintEventArgs e)
		{

		}

	}

	public enum Status
	{
		Processing,
		Success,
		Failed
	}
}

