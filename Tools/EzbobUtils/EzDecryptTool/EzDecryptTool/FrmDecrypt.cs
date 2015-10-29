namespace DecryptTool {
	using System;
	using System.Windows.Forms;
	using Ezbob.Utils.Security;

	public partial class FrmDecrypt : Form {
		public FrmDecrypt() {
			InitializeComponent();
		} // constructor

		private void btnEncrypt_Click(object sender, EventArgs e) {
			string output = new Encrypted(this.txtInput.Text);
			this.txtOutput.Text = output;
			this.txtOutput.Update();
		} // encrypt clicked

		private void btnDecrypt_Click(object sender, EventArgs e) {
			try {
				this.txtOutput.Text = Encrypted.Decrypt(this.txtInput.Text);
			} catch (Exception ex) {
				this.txtOutput.Text = ex.Message;
			} // try

			this.txtOutput.Update();
		} // decrypt clicked

		private void FrmDecrypt_Load(object sender, EventArgs e) {
			this.txtInput.Focus();
		} // form loaded
	} // class FrmDecrypt
} // namespace
