using System;
using System.Windows.Forms;

namespace DecryptTool {
	using Ezbob.Utils.Security;

	public partial class FrmDecrypt : Form {
		public FrmDecrypt() {
			InitializeComponent();
		}

		private void btnEncrypt_Click(object sender, EventArgs e) {
			string output = new Encrypted(txtInput.Text);
			txtOutput.Text = output;
			txtOutput.Update();
		}

		private void btnDecrypt_Click(object sender, EventArgs e) {
			try {
				string output = Encrypted.Decrypt(txtInput.Text);
				txtOutput.Text = output;
			} catch (Exception ex) {
				txtOutput.Text = ex.Message;
			}
			txtOutput.Update();
		}
	}
}
