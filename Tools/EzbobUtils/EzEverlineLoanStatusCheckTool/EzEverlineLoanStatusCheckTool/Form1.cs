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

        private void btnDetails_Click(object sender, EventArgs e)
        {
            lblLoading.Visible = true;
            btnCheck.Enabled = false;
            Refresh();
            try
            {
                RestClient client = new RestClient("https://restapi.everline.com/1.0/ezbob/businessorganisationdetails");

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
 				var result = JsonConvert.DeserializeObject<EverlineBusinessOrganisationDetails>(response.Content);

                decimal? outstanding = 0;
                string loanRef = "";
                int term = 0;
                DateTime? date = null;
                if (result.LoanApplications.Any(x => !x.ClosedOn.HasValue && x.BalanceDetails != null && x.BalanceDetails.TotalOutstandingBalance > 0))
                {
                    var loan =
                        result.LoanApplications.First(
                            x =>
                                !x.ClosedOn.HasValue && x.BalanceDetails != null &&
                                x.BalanceDetails.TotalOutstandingBalance > 0);

                    outstanding = loan.BalanceDetails.TotalOutstandingBalance;
                    loanRef = loan.LoanId.ToString();
                    term = loan.Term;
                    date = loan.FundedOn;

                }

                lblResult.Text = string.Format("outstanding {0}, loanRef {1}, term {2}, date {3}", outstanding, loanRef, term, date);
                lblError.Text = JsonConvert.SerializeObject(result.LoanApplications, Formatting.Indented);
            }
            catch (Exception ex)
            {
                lblResult.Text = EverlineLoanStatus.Error.ToString();
                lblError.Text = ex.Message;
            }
            finally
            {
                lblLoading.Visible = false;
                btnCheck.Enabled = true;
                Refresh();
            }
        }
	}
}
