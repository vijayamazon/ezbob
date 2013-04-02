namespace GuiNodeEzBobLib
{
	partial class EzBobUpdateCustomerDataNodePropertiesForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose( bool disposing )
		{
			if ( disposing && ( components != null ) )
			{
				components.Dispose();
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EzBobUpdateCustomerDataNodePropertiesForm));
			this.VarMappingTabPage = new System.Windows.Forms.TabPage();
			this.StrategyAndNodeVariableMapping = new StrategyBuilder.UI.Common.StrategyAndNodeVariableMapping();
			this.tabControl1.SuspendLayout();
			this.mainPanel.SuspendLayout();
			this.VarMappingTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabControl1
			// 
			resources.ApplyResources(this.tabControl1, "tabControl1");
			this.tabControl1.Controls.Add(this.VarMappingTabPage);
			this.tabControl1.Controls.SetChildIndex(this.VarMappingTabPage, 0);
			this.tabControl1.Controls.SetChildIndex(this.m_AnnotationTabPage, 0);
			// 
			// m_TabImageList
			// 
			this.m_TabImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("m_TabImageList.ImageStream")));
			this.m_TabImageList.Images.SetKeyName(0, "notebook.png");
			// 
			// m_AnnotationTabPage
			// 
			resources.ApplyResources(this.m_AnnotationTabPage, "m_AnnotationTabPage");
			// 
			// ResetButton
			// 
			resources.ApplyResources(this.ResetButton, "ResetButton");
			// 
			// ApplyButton
			// 
			resources.ApplyResources(this.ApplyButton, "ApplyButton");
			// 
			// CancelButton
			// 
			resources.ApplyResources(this.CancelButton, "CancelButton");
			// 
			// OKButton
			// 
			resources.ApplyResources(this.OKButton, "OKButton");
			// 
			// mainPanel
			// 
			resources.ApplyResources(this.mainPanel, "mainPanel");
			// 
			// VarMappingTabPage
			// 
			resources.ApplyResources(this.VarMappingTabPage, "VarMappingTabPage");
			this.VarMappingTabPage.Controls.Add(this.StrategyAndNodeVariableMapping);
			this.VarMappingTabPage.Name = "VarMappingTabPage";
			this.VarMappingTabPage.UseVisualStyleBackColor = true;
			// 
			// StrategyAndNodeVariableMapping
			// 
			resources.ApplyResources(this.StrategyAndNodeVariableMapping, "StrategyAndNodeVariableMapping");
			this.StrategyAndNodeVariableMapping.AllowCreateAndLink = true;
			this.StrategyAndNodeVariableMapping.DisableVariableFilters = false;
			this.StrategyAndNodeVariableMapping.EnableVariableMultipleLinks = false;
			this.StrategyAndNodeVariableMapping.ExternalAndFilter = null;
			this.StrategyAndNodeVariableMapping.Name = "StrategyAndNodeVariableMapping";
			this.StrategyAndNodeVariableMapping.UseScalarAndColumnsVariablesMapping = false;
			// 
			// EzBobUpdateCustomerDataNodePropertiesForm
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Name = "EzBobUpdateCustomerDataNodePropertiesForm";
			this.tabControl1.ResumeLayout(false);
			this.mainPanel.ResumeLayout(false);
			this.VarMappingTabPage.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TabPage VarMappingTabPage;
		private StrategyBuilder.UI.Common.StrategyAndNodeVariableMapping StrategyAndNodeVariableMapping;
	}
}