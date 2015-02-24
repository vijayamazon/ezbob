using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EzEverlineLoanStatusCheckTool {
	using Newtonsoft.Json;
	using RestSharp;

	public partial class Form1 : Form {
		public Form1() {
			InitializeComponent();
		}

		private void btnCheck_Click(object sender, EventArgs e) {
			lblLoading.Visible = true;
			btnCheck.Enabled = false;
			Refresh();
			try {
				RestClient client = new RestClient("https://restapi.everline.com/1.0/ezbob/customerloanstatus");
				var request = new RestRequest(Method.GET) {
					Parameters = {
						new Parameter {
							Name = "email",
							Type = ParameterType.QueryString,
							Value = txtEmail.Text
						}
					},
				};
				request.AddHeader("X-Authentication", "c83c4951-d205-4452-bce0-f0bc24d3e8b2,00000000-0000-0000-0000-000000000000,5a4690c72b1723a54530700250d90e5b35aa283ffb884525a031c3dc0a164c5c");
				var response = client.Execute(request);
				if (string.IsNullOrEmpty(response.Content)) {
					lblResult.Text = EverlineLoanStatus.Error.ToString();
					lblError.Text = response.ErrorMessage;
					return;
				}
				var result = JsonConvert.DeserializeObject<EverlineLoginLoanCheckerResult>(response.Content);
				lblResult.Text = result.status.ToString();
				lblError.Text = result.Message;
			} catch (Exception ex) {
				lblResult.Text = EverlineLoanStatus.Error.ToString();
				lblError.Text = ex.Message;
			} finally {
				lblLoading.Visible = false;
				btnCheck.Enabled = true;
				Refresh();
			}
		}
	}
}
