namespace WinFormTestApp
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new System.Windows.Forms.DataGridViewCellStyle();
			this.summaryDataTable = new System.Data.DataTable();
			this.txtReq = new System.Windows.Forms.RichTextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.txtException = new System.Windows.Forms.RichTextBox();
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.chkClearRes = new System.Windows.Forms.CheckBox();
			this.treeRes = new System.Windows.Forms.TreeView();
			this.tabPage2 = new System.Windows.Forms.TabPage();
			this.txtRes = new System.Windows.Forms.RichTextBox();
			this.label6 = new System.Windows.Forms.Label();
			this.tabPage3 = new System.Windows.Forms.TabPage();
			this.tableLayoutPanel13 = new System.Windows.Forms.TableLayoutPanel();
			this._rtbTestAllLog = new System.Windows.Forms.RichTextBox();
			this._lvMethods = new System.Windows.Forms.ListView();
			this._chMethod = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this._imgList = new System.Windows.Forms.ImageList(this.components);
			this.tabPage4 = new System.Windows.Forms.TabPage();
			this.summaryGrid = new System.Windows.Forms.DataGridView();
			this.tabPage5 = new System.Windows.Forms.TabPage();
			this.faultOnly = new System.Windows.Forms.RichTextBox();
			this.summarydataSet = new System.Data.DataSet();
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.testToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.chkClearReq = new System.Windows.Forms.CheckBox();
			this.chkClearException = new System.Windows.Forms.CheckBox();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanel6 = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanel8 = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanel7 = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
			this.label1 = new System.Windows.Forms.Label();
			this.ddlMethods = new System.Windows.Forms.ComboBox();
			this.tableLayoutPanel9 = new System.Windows.Forms.TableLayoutPanel();
			this.btnLoadReq = new System.Windows.Forms.Button();
			this.tableLayoutPanel11 = new System.Windows.Forms.TableLayoutPanel();
			this.btnTestCustom = new System.Windows.Forms.Button();
			this.btnSearch = new System.Windows.Forms.Button();
			this.txtSearchTerm = new System.Windows.Forms.TextBox();
			this.chkWholeWord = new System.Windows.Forms.CheckBox();
			this.EndPoint = new System.Windows.Forms.Label();
			this.tableLayoutPanel14 = new System.Windows.Forms.TableLayoutPanel();
			this.imgLoader = new System.Windows.Forms.Label();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this._epEndpoint = new System.Windows.Forms.ErrorProvider(this.components);
			this.btnClear = new System.Windows.Forms.Button();
			this.chkClearAll = new System.Windows.Forms.CheckBox();
			((System.ComponentModel.ISupportInitialize)(this.summaryDataTable)).BeginInit();
			this.tabControl1.SuspendLayout();
			this.tabPage1.SuspendLayout();
			this.tabPage2.SuspendLayout();
			this.tabPage3.SuspendLayout();
			this.tableLayoutPanel13.SuspendLayout();
			this.tabPage4.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.summaryGrid)).BeginInit();
			this.tabPage5.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.summarydataSet)).BeginInit();
			this.menuStrip1.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			this.tableLayoutPanel6.SuspendLayout();
			this.tableLayoutPanel8.SuspendLayout();
			this.tableLayoutPanel7.SuspendLayout();
			this.tableLayoutPanel2.SuspendLayout();
			this.tableLayoutPanel5.SuspendLayout();
			this.tableLayoutPanel9.SuspendLayout();
			this.tableLayoutPanel11.SuspendLayout();
			this.tableLayoutPanel14.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this._epEndpoint)).BeginInit();
			this.SuspendLayout();
			// 
			// txtReq
			// 
			this.txtReq.Location = new System.Drawing.Point(3, 33);
			this.txtReq.Name = "txtReq";
			this.txtReq.Size = new System.Drawing.Size(436, 236);
			this.txtReq.TabIndex = 2;
			this.txtReq.Tag = "Request";
			this.txtReq.Text = "";
			// 
			// label2
			// 
			this.label2.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(3, 5);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(47, 13);
			this.label2.TabIndex = 7;
			this.label2.Text = "Request";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(6, 4);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(55, 13);
			this.label3.TabIndex = 8;
			this.label3.Text = "Response";
			// 
			// label5
			// 
			this.label5.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(3, 5);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(54, 13);
			this.label5.TabIndex = 11;
			this.label5.Text = "Exception";
			// 
			// txtException
			// 
			this.txtException.Location = new System.Drawing.Point(447, 33);
			this.txtException.Name = "txtException";
			this.txtException.Size = new System.Drawing.Size(469, 236);
			this.txtException.TabIndex = 10;
			this.txtException.Text = "";
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.Add(this.tabPage1);
			this.tabControl1.Controls.Add(this.tabPage2);
			this.tabControl1.Controls.Add(this.tabPage3);
			this.tabControl1.Controls.Add(this.tabPage4);
			this.tabControl1.Controls.Add(this.tabPage5);
			this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControl1.Location = new System.Drawing.Point(3, 610);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(926, 237);
			this.tabControl1.TabIndex = 22;
			// 
			// tabPage1
			// 
			this.tabPage1.Controls.Add(this.chkClearRes);
			this.tabPage1.Controls.Add(this.treeRes);
			this.tabPage1.Controls.Add(this.label3);
			this.tabPage1.Location = new System.Drawing.Point(4, 22);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage1.Size = new System.Drawing.Size(918, 211);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = "Tree View";
			this.tabPage1.UseVisualStyleBackColor = true;
			// 
			// chkClearRes
			// 
			this.chkClearRes.AutoSize = true;
			this.chkClearRes.Checked = true;
			this.chkClearRes.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkClearRes.Location = new System.Drawing.Point(328, 2);
			this.chkClearRes.Name = "chkClearRes";
			this.chkClearRes.Size = new System.Drawing.Size(50, 17);
			this.chkClearRes.TabIndex = 40;
			this.chkClearRes.Text = "Clear";
			this.chkClearRes.UseVisualStyleBackColor = true;
			// 
			// treeRes
			// 
			this.treeRes.Location = new System.Drawing.Point(6, 20);
			this.treeRes.Name = "treeRes";
			this.treeRes.Size = new System.Drawing.Size(406, 386);
			this.treeRes.TabIndex = 15;
			// 
			// tabPage2
			// 
			this.tabPage2.Controls.Add(this.txtRes);
			this.tabPage2.Controls.Add(this.label6);
			this.tabPage2.Location = new System.Drawing.Point(4, 22);
			this.tabPage2.Name = "tabPage2";
			this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage2.Size = new System.Drawing.Size(918, 211);
			this.tabPage2.TabIndex = 1;
			this.tabPage2.Text = "Rich Text View";
			this.tabPage2.UseVisualStyleBackColor = true;
			// 
			// txtRes
			// 
			this.txtRes.Location = new System.Drawing.Point(8, 24);
			this.txtRes.Name = "txtRes";
			this.txtRes.Size = new System.Drawing.Size(403, 387);
			this.txtRes.TabIndex = 12;
			this.txtRes.Text = "";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(6, 3);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(55, 13);
			this.label6.TabIndex = 10;
			this.label6.Text = "Response";
			// 
			// tabPage3
			// 
			this.tabPage3.Controls.Add(this.tableLayoutPanel13);
			this.tabPage3.Location = new System.Drawing.Point(4, 22);
			this.tabPage3.Name = "tabPage3";
			this.tabPage3.Size = new System.Drawing.Size(918, 211);
			this.tabPage3.TabIndex = 2;
			this.tabPage3.Text = "TestAll";
			this.tabPage3.UseVisualStyleBackColor = true;
			// 
			// tableLayoutPanel13
			// 
			this.tableLayoutPanel13.ColumnCount = 2;
			this.tableLayoutPanel13.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 35.07625F));
			this.tableLayoutPanel13.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 64.92374F));
			this.tableLayoutPanel13.Controls.Add(this._rtbTestAllLog, 1, 0);
			this.tableLayoutPanel13.Controls.Add(this._lvMethods, 0, 0);
			this.tableLayoutPanel13.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel13.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel13.Name = "tableLayoutPanel13";
			this.tableLayoutPanel13.RowCount = 1;
			this.tableLayoutPanel13.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel13.Size = new System.Drawing.Size(918, 211);
			this.tableLayoutPanel13.TabIndex = 60;
			// 
			// _rtbTestAllLog
			// 
			this._rtbTestAllLog.Location = new System.Drawing.Point(325, 3);
			this._rtbTestAllLog.Name = "_rtbTestAllLog";
			this._rtbTestAllLog.Size = new System.Drawing.Size(590, 205);
			this._rtbTestAllLog.TabIndex = 1;
			this._rtbTestAllLog.Text = "";
			// 
			// _lvMethods
			// 
			this._lvMethods.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this._chMethod});
			this._lvMethods.Dock = System.Windows.Forms.DockStyle.Fill;
			this._lvMethods.Location = new System.Drawing.Point(3, 3);
			this._lvMethods.Name = "_lvMethods";
			this._lvMethods.Size = new System.Drawing.Size(316, 205);
			this._lvMethods.SmallImageList = this._imgList;
			this._lvMethods.TabIndex = 2;
			this._lvMethods.UseCompatibleStateImageBehavior = false;
			this._lvMethods.View = System.Windows.Forms.View.Details;
			// 
			// _chMethod
			// 
			this._chMethod.Tag = "";
			this._chMethod.Text = "Inspected Method";
			this._chMethod.Width = 300;
			// 
			// _imgList
			// 
			this._imgList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("_imgList.ImageStream")));
			this._imgList.TransparentColor = System.Drawing.Color.Transparent;
			this._imgList.Images.SetKeyName(0, "Failed.png");
			this._imgList.Images.SetKeyName(1, "Passed.gif");
			this._imgList.Images.SetKeyName(2, "progress.png");
			// 
			// tabPage4
			// 
			this.tabPage4.Controls.Add(this.summaryGrid);
			this.tabPage4.Location = new System.Drawing.Point(4, 22);
			this.tabPage4.Name = "tabPage4";
			this.tabPage4.Size = new System.Drawing.Size(918, 211);
			this.tabPage4.TabIndex = 3;
			this.tabPage4.Text = "Summary";
			this.tabPage4.UseVisualStyleBackColor = true;
			// 
			// summaryGrid
			// 
			dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle7.BackColor = System.Drawing.SystemColors.Control;
			dataGridViewCellStyle7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			dataGridViewCellStyle7.ForeColor = System.Drawing.SystemColors.WindowText;
			dataGridViewCellStyle7.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle7.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle7.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.summaryGrid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle7;
			this.summaryGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle8.BackColor = System.Drawing.SystemColors.Window;
			dataGridViewCellStyle8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			dataGridViewCellStyle8.ForeColor = System.Drawing.SystemColors.ControlText;
			dataGridViewCellStyle8.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle8.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle8.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
			this.summaryGrid.DefaultCellStyle = dataGridViewCellStyle8;
			this.summaryGrid.Location = new System.Drawing.Point(168, 3);
			this.summaryGrid.Name = "summaryGrid";
			dataGridViewCellStyle9.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle9.BackColor = System.Drawing.SystemColors.Control;
			dataGridViewCellStyle9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			dataGridViewCellStyle9.ForeColor = System.Drawing.SystemColors.WindowText;
			dataGridViewCellStyle9.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle9.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle9.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.summaryGrid.RowHeadersDefaultCellStyle = dataGridViewCellStyle9;
			this.summaryGrid.Size = new System.Drawing.Size(503, 406);
			this.summaryGrid.TabIndex = 0;
			// 
			// tabPage5
			// 
			this.tabPage5.Controls.Add(this.faultOnly);
			this.tabPage5.Location = new System.Drawing.Point(4, 22);
			this.tabPage5.Name = "tabPage5";
			this.tabPage5.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage5.Size = new System.Drawing.Size(918, 211);
			this.tabPage5.TabIndex = 4;
			this.tabPage5.Text = "faultOnly";
			this.tabPage5.UseVisualStyleBackColor = true;
			// 
			// faultOnly
			// 
			this.faultOnly.Location = new System.Drawing.Point(6, 3);
			this.faultOnly.Name = "faultOnly";
			this.faultOnly.Size = new System.Drawing.Size(811, 403);
			this.faultOnly.TabIndex = 0;
			this.faultOnly.Text = "";
			// 
			// summarydataSet
			// 
			this.summarydataSet.DataSetName = "NewDataSet";
			// 
			// menuStrip1
			// 
			this.menuStrip1.BackColor = System.Drawing.SystemColors.Control;
			this.menuStrip1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.toolsToolStripMenuItem,
            this.helpToolStripMenuItem});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
			this.menuStrip1.Size = new System.Drawing.Size(932, 24);
			this.menuStrip1.TabIndex = 34;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// fileToolStripMenuItem
			// 
			this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.saveToolStripMenuItem,
            this.saveAsToolStripMenuItem,
            this.exitToolStripMenuItem});
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
			this.fileToolStripMenuItem.Text = "File";
			// 
			// openToolStripMenuItem
			// 
			this.openToolStripMenuItem.Name = "openToolStripMenuItem";
			this.openToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+O";
			this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
			this.openToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
			this.openToolStripMenuItem.Text = "Open";
			this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
			// 
			// saveToolStripMenuItem
			// 
			this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
			this.saveToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+S";
			this.saveToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
			this.saveToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
			this.saveToolStripMenuItem.Text = "Save";
			this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
			// 
			// saveAsToolStripMenuItem
			// 
			this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
			this.saveAsToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+Alt+S";
			this.saveAsToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Alt) 
            | System.Windows.Forms.Keys.S)));
			this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
			this.saveAsToolStripMenuItem.Text = "Save As...";
			this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
			// 
			// exitToolStripMenuItem
			// 
			this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
			this.exitToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Q)));
			this.exitToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
			this.exitToolStripMenuItem.Text = "Exit";
			this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
			// 
			// toolsToolStripMenuItem
			// 
			this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.testToolStripMenuItem});
			this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
			this.toolsToolStripMenuItem.Size = new System.Drawing.Size(48, 20);
			this.toolsToolStripMenuItem.Text = "Tools";
			// 
			// testToolStripMenuItem
			// 
			this.testToolStripMenuItem.Name = "testToolStripMenuItem";
			this.testToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.T)));
			this.testToolStripMenuItem.Size = new System.Drawing.Size(137, 22);
			this.testToolStripMenuItem.Text = "Test";
			this.testToolStripMenuItem.Click += new System.EventHandler(this.testToolStripMenuItem_Click);
			// 
			// helpToolStripMenuItem
			// 
			this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
			this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
			this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
			this.helpToolStripMenuItem.Text = "Help";
			// 
			// aboutToolStripMenuItem
			// 
			this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
			this.aboutToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.H)));
			this.aboutToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
			this.aboutToolStripMenuItem.Text = "About";
			this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
			// 
			// chkClearReq
			// 
			this.chkClearReq.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.chkClearReq.AutoSize = true;
			this.chkClearReq.Checked = true;
			this.chkClearReq.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkClearReq.Location = new System.Drawing.Point(204, 3);
			this.chkClearReq.Name = "chkClearReq";
			this.chkClearReq.Size = new System.Drawing.Size(50, 17);
			this.chkClearReq.TabIndex = 38;
			this.chkClearReq.Text = "Clear";
			this.chkClearReq.UseVisualStyleBackColor = true;
			// 
			// chkClearException
			// 
			this.chkClearException.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.chkClearException.AutoSize = true;
			this.chkClearException.Checked = true;
			this.chkClearException.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkClearException.Location = new System.Drawing.Point(228, 3);
			this.chkClearException.Name = "chkClearException";
			this.chkClearException.Size = new System.Drawing.Size(50, 17);
			this.chkClearException.TabIndex = 39;
			this.chkClearException.Text = "Clear";
			this.chkClearException.UseVisualStyleBackColor = true;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel6, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.tabControl1, 0, 2);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 24);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 3;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20.70588F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50.70588F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 28.47059F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(932, 850);
			this.tableLayoutPanel1.TabIndex = 49;
			// 
			// tableLayoutPanel6
			// 
			this.tableLayoutPanel6.ColumnCount = 2;
			this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 47.94817F));
			this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 52.05183F));
			this.tableLayoutPanel6.Controls.Add(this.tableLayoutPanel8, 1, 0);
			this.tableLayoutPanel6.Controls.Add(this.tableLayoutPanel7, 0, 0);
			this.tableLayoutPanel6.Controls.Add(this.txtException, 1, 1);
			this.tableLayoutPanel6.Controls.Add(this.txtReq, 0, 1);
			this.tableLayoutPanel6.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel6.Location = new System.Drawing.Point(3, 179);
			this.tableLayoutPanel6.Name = "tableLayoutPanel6";
			this.tableLayoutPanel6.RowCount = 2;
			this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel6.Size = new System.Drawing.Size(926, 425);
			this.tableLayoutPanel6.TabIndex = 50;
			this.tableLayoutPanel6.Paint += new System.Windows.Forms.PaintEventHandler(this.tableLayoutPanel6_Paint);
			// 
			// tableLayoutPanel8
			// 
			this.tableLayoutPanel8.ColumnCount = 2;
			this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 47.36842F));
			this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 52.63158F));
			this.tableLayoutPanel8.Controls.Add(this.label5, 0, 0);
			this.tableLayoutPanel8.Controls.Add(this.chkClearException, 1, 0);
			this.tableLayoutPanel8.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel8.Location = new System.Drawing.Point(447, 3);
			this.tableLayoutPanel8.Name = "tableLayoutPanel8";
			this.tableLayoutPanel8.RowCount = 1;
			this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel8.Size = new System.Drawing.Size(476, 24);
			this.tableLayoutPanel8.TabIndex = 63;
			// 
			// tableLayoutPanel7
			// 
			this.tableLayoutPanel7.ColumnCount = 2;
			this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 46.01367F));
			this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 53.98633F));
			this.tableLayoutPanel7.Controls.Add(this.label2, 0, 0);
			this.tableLayoutPanel7.Controls.Add(this.chkClearReq, 1, 0);
			this.tableLayoutPanel7.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel7.Location = new System.Drawing.Point(3, 3);
			this.tableLayoutPanel7.Name = "tableLayoutPanel7";
			this.tableLayoutPanel7.RowCount = 1;
			this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel7.Size = new System.Drawing.Size(438, 24);
			this.tableLayoutPanel7.TabIndex = 62;
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.ColumnCount = 2;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel5, 0, 2);
			this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel14, 1, 1);
			this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 3;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel2.Size = new System.Drawing.Size(926, 170);
			this.tableLayoutPanel2.TabIndex = 61;
			// 
			// tableLayoutPanel5
			// 
			this.tableLayoutPanel5.ColumnCount = 4;
			this.tableLayoutPanel2.SetColumnSpan(this.tableLayoutPanel5, 2);
			this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel5.Controls.Add(this.label1, 0, 0);
			this.tableLayoutPanel5.Controls.Add(this.ddlMethods, 0, 1);
			this.tableLayoutPanel5.Controls.Add(this.tableLayoutPanel9, 1, 1);
			this.tableLayoutPanel5.Controls.Add(this.tableLayoutPanel11, 1, 2);
			this.tableLayoutPanel5.Controls.Add(this.btnSearch, 2, 1);
			this.tableLayoutPanel5.Controls.Add(this.txtSearchTerm, 2, 2);
			this.tableLayoutPanel5.Controls.Add(this.chkWholeWord, 3, 2);
			this.tableLayoutPanel5.Controls.Add(this.EndPoint, 1, 0);
			this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel5.Location = new System.Drawing.Point(3, 38);
			this.tableLayoutPanel5.Name = "tableLayoutPanel5";
			this.tableLayoutPanel5.RowCount = 2;
			this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel5.Size = new System.Drawing.Size(920, 129);
			this.tableLayoutPanel5.TabIndex = 0;
			// 
			// label1
			// 
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(3, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(107, 16);
			this.label1.TabIndex = 6;
			this.label1.Text = "Select Method";
			// 
			// ddlMethods
			// 
			this.ddlMethods.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.ddlMethods.FormattingEnabled = true;
			this.ddlMethods.Location = new System.Drawing.Point(3, 31);
			this.ddlMethods.Name = "ddlMethods";
			this.ddlMethods.Size = new System.Drawing.Size(168, 21);
			this.ddlMethods.TabIndex = 0;
			this.ddlMethods.SelectedIndexChanged += new System.EventHandler(this.ddlMethods_SelectedIndexChanged);
			// 
			// tableLayoutPanel9
			// 
			this.tableLayoutPanel9.ColumnCount = 3;
			this.tableLayoutPanel9.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel9.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel9.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 138F));
			this.tableLayoutPanel9.Controls.Add(this.btnLoadReq, 0, 0);
			this.tableLayoutPanel9.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel9.Location = new System.Drawing.Point(177, 27);
			this.tableLayoutPanel9.Name = "tableLayoutPanel9";
			this.tableLayoutPanel9.RowCount = 1;
			this.tableLayoutPanel9.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel9.Size = new System.Drawing.Size(394, 29);
			this.tableLayoutPanel9.TabIndex = 0;
			// 
			// btnLoadReq
			// 
			this.btnLoadReq.Location = new System.Drawing.Point(3, 3);
			this.btnLoadReq.Name = "btnLoadReq";
			this.btnLoadReq.Size = new System.Drawing.Size(122, 23);
			this.btnLoadReq.TabIndex = 1;
			this.btnLoadReq.Text = "load deafult request";
			this.btnLoadReq.UseVisualStyleBackColor = true;
			this.btnLoadReq.Click += new System.EventHandler(this.btnLoadReq_Click);
			// 
			// tableLayoutPanel11
			// 
			this.tableLayoutPanel11.ColumnCount = 4;
			this.tableLayoutPanel11.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel11.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel11.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel11.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 141F));
			this.tableLayoutPanel11.Controls.Add(this.btnTestCustom, 0, 0);
			this.tableLayoutPanel11.Controls.Add(this.btnClear, 2, 0);
			this.tableLayoutPanel11.Controls.Add(this.chkClearAll, 3, 0);
			this.tableLayoutPanel11.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel11.Location = new System.Drawing.Point(177, 62);
			this.tableLayoutPanel11.Name = "tableLayoutPanel11";
			this.tableLayoutPanel11.RowCount = 1;
			this.tableLayoutPanel11.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel11.Size = new System.Drawing.Size(394, 64);
			this.tableLayoutPanel11.TabIndex = 51;
			// 
			// btnTestCustom
			// 
			this.btnTestCustom.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.btnTestCustom.Location = new System.Drawing.Point(3, 20);
			this.btnTestCustom.Name = "btnTestCustom";
			this.btnTestCustom.Size = new System.Drawing.Size(122, 23);
			this.btnTestCustom.TabIndex = 3;
			this.btnTestCustom.Text = "test";
			this.btnTestCustom.UseVisualStyleBackColor = true;
			this.btnTestCustom.Click += new System.EventHandler(this.btnTestCustom_Click);
			// 
			// btnSearch
			// 
			this.btnSearch.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.btnSearch.Location = new System.Drawing.Point(577, 30);
			this.btnSearch.Name = "btnSearch";
			this.btnSearch.Size = new System.Drawing.Size(132, 23);
			this.btnSearch.TabIndex = 41;
			this.btnSearch.Text = "Search in Response";
			this.btnSearch.UseVisualStyleBackColor = true;
			this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
			// 
			// txtSearchTerm
			// 
			this.txtSearchTerm.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.txtSearchTerm.Location = new System.Drawing.Point(577, 84);
			this.txtSearchTerm.Name = "txtSearchTerm";
			this.txtSearchTerm.Size = new System.Drawing.Size(132, 20);
			this.txtSearchTerm.TabIndex = 42;
			// 
			// chkWholeWord
			// 
			this.chkWholeWord.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.chkWholeWord.AutoSize = true;
			this.chkWholeWord.Location = new System.Drawing.Point(715, 85);
			this.chkWholeWord.Name = "chkWholeWord";
			this.chkWholeWord.Size = new System.Drawing.Size(116, 17);
			this.chkWholeWord.TabIndex = 46;
			this.chkWholeWord.Text = "Match whole word ";
			this.chkWholeWord.UseVisualStyleBackColor = true;
			// 
			// EndPoint
			// 
			this.EndPoint.AutoSize = true;
			this.EndPoint.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.EndPoint.Location = new System.Drawing.Point(177, 0);
			this.EndPoint.Name = "EndPoint";
			this.EndPoint.Size = new System.Drawing.Size(118, 24);
			this.EndPoint.TabIndex = 54;
			this.EndPoint.Text = "EndPoint IP: ";
			// 
			// tableLayoutPanel14
			// 
			this.tableLayoutPanel14.ColumnCount = 3;
			this.tableLayoutPanel14.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel14.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel14.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 37F));
			this.tableLayoutPanel14.Controls.Add(this.imgLoader, 1, 0);
			this.tableLayoutPanel14.Controls.Add(this.pictureBox1, 2, 0);
			this.tableLayoutPanel14.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel14.Location = new System.Drawing.Point(466, 3);
			this.tableLayoutPanel14.Name = "tableLayoutPanel14";
			this.tableLayoutPanel14.RowCount = 1;
			this.tableLayoutPanel14.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel14.Size = new System.Drawing.Size(457, 29);
			this.tableLayoutPanel14.TabIndex = 53;
			// 
			// imgLoader
			// 
			this.imgLoader.AutoSize = true;
			this.imgLoader.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.imgLoader.Location = new System.Drawing.Point(213, 0);
			this.imgLoader.Name = "imgLoader";
			this.imgLoader.Size = new System.Drawing.Size(163, 26);
			this.imgLoader.TabIndex = 21;
			this.imgLoader.Text = "Proccessing...";
			this.imgLoader.Visible = false;
			// 
			// pictureBox1
			// 
			this.pictureBox1.Image = global::WinFormTestApp.Properties.Resources.Progress;
			this.pictureBox1.Location = new System.Drawing.Point(423, 3);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(31, 23);
			this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.pictureBox1.TabIndex = 22;
			this.pictureBox1.TabStop = false;
			// 
			// _epEndpoint
			// 
			this._epEndpoint.ContainerControl = this;
			// 
			// btnClear
			// 
			this.btnClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.btnClear.Location = new System.Drawing.Point(131, 20);
			this.btnClear.Name = "btnClear";
			this.btnClear.Size = new System.Drawing.Size(119, 23);
			this.btnClear.TabIndex = 65;
			this.btnClear.Text = "Clear";
			this.btnClear.UseVisualStyleBackColor = true;
			// 
			// chkClearAll
			// 
			this.chkClearAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.chkClearAll.AutoSize = true;
			this.chkClearAll.Location = new System.Drawing.Point(256, 23);
			this.chkClearAll.Name = "chkClearAll";
			this.chkClearAll.Size = new System.Drawing.Size(135, 17);
			this.chkClearAll.TabIndex = 66;
			this.chkClearAll.Text = "Clear All";
			this.chkClearAll.UseVisualStyleBackColor = true;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(932, 874);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Controls.Add(this.menuStrip1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MainMenuStrip = this.menuStrip1;
			this.Name = "Form1";
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "StasTool";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
			this.Load += new System.EventHandler(this.Form1_Load);
			((System.ComponentModel.ISupportInitialize)(this.summaryDataTable)).EndInit();
			this.tabControl1.ResumeLayout(false);
			this.tabPage1.ResumeLayout(false);
			this.tabPage1.PerformLayout();
			this.tabPage2.ResumeLayout(false);
			this.tabPage2.PerformLayout();
			this.tabPage3.ResumeLayout(false);
			this.tableLayoutPanel13.ResumeLayout(false);
			this.tabPage4.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.summaryGrid)).EndInit();
			this.tabPage5.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.summarydataSet)).EndInit();
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel6.ResumeLayout(false);
			this.tableLayoutPanel8.ResumeLayout(false);
			this.tableLayoutPanel8.PerformLayout();
			this.tableLayoutPanel7.ResumeLayout(false);
			this.tableLayoutPanel7.PerformLayout();
			this.tableLayoutPanel2.ResumeLayout(false);
			this.tableLayoutPanel5.ResumeLayout(false);
			this.tableLayoutPanel5.PerformLayout();
			this.tableLayoutPanel9.ResumeLayout(false);
			this.tableLayoutPanel11.ResumeLayout(false);
			this.tableLayoutPanel11.PerformLayout();
			this.tableLayoutPanel14.ResumeLayout(false);
			this.tableLayoutPanel14.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this._epEndpoint)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        private System.Windows.Forms.RichTextBox txtReq;
        private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.RichTextBox txtException;
        private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TreeView treeRes;
		private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.RichTextBox txtRes;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.DataGridView summaryGrid;
        private System.Data.DataSet summarydataSet;
        private System.Windows.Forms.TabPage tabPage5;
		private System.Windows.Forms.RichTextBox faultOnly;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem testToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.CheckBox chkClearRes;
        private System.Windows.Forms.CheckBox chkClearReq;
		private System.Windows.Forms.CheckBox chkClearException;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel6;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel8;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel7;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox chkWholeWord;
        private System.Windows.Forms.ComboBox ddlMethods;
        private System.Windows.Forms.Button btnSearch;
		private System.Windows.Forms.TextBox txtSearchTerm;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel9;
		private System.Windows.Forms.Button btnLoadReq;
		private System.Windows.Forms.Button btnTestCustom;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel11;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel13;
        private System.Windows.Forms.RichTextBox _rtbTestAllLog;
        private System.Windows.Forms.ListView _lvMethods;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel14;
        private System.Windows.Forms.Label imgLoader;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.ImageList _imgList;
        private System.Windows.Forms.ColumnHeader _chMethod;
		private System.Windows.Forms.ErrorProvider _epEndpoint;
		private System.Windows.Forms.Label EndPoint;
		private System.Windows.Forms.Button btnClear;
		private System.Windows.Forms.CheckBox chkClearAll;
    }
}

