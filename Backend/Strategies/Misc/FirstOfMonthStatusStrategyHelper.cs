namespace Ezbob.Backend.Strategies.Misc {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Text;
	using ConfigManager;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using StructureMap;
	using log4net;
	using MailApi;

	public class FirstOfMonthStatusStrategyHelper {
		public FirstOfMonthStatusStrategyHelper() {
			_customers = ObjectFactory.GetInstance<CustomerRepository>();
		}

		public void SendFirstOfMonthStatusMail() {
			bool firstOfMonthStatusMailEnabled = CurrentValues.Instance.FirstOfMonthStatusMailEnabled;
			string firstOfMonthStatusMailCopyTo = CurrentValues.Instance.FirstOfMonthStatusMailCopyTo;
			bool firstOfMonthEnableCustomerMail = CurrentValues.Instance.FirstOfMonthEnableCustomerMail;

			var customersToWorkOn = new List<Customer>();

			if (firstOfMonthStatusMailEnabled) {
				customersToWorkOn.AddRange(_customers.GetAll().Where(c => !c.IsTest && c.CollectionStatus.CurrentStatus.IsEnabled && (!c.MonthlyStatusEnabled.HasValue || c.MonthlyStatusEnabled.Value)));
			} else {
				customersToWorkOn.AddRange(_customers.GetAll().Where(c => !c.IsTest && c.CollectionStatus.CurrentStatus.IsEnabled && c.MonthlyStatusEnabled.HasValue && c.MonthlyStatusEnabled.Value));
			}

			log.InfoFormat("Starting to send first of month status mails");

			foreach (Customer customer in customersToWorkOn) {
				List<Loan> outstandingLoans = GetOutstandingLoans(customer.Id);

				if (outstandingLoans.Count > 0) {
					List<Loan> closedLoans = GetLastMonthClosedLoans(customer.Id);
					log.InfoFormat("Customer {0} has {1} outstanding loans. Will send status mail to him", customer.Id, outstandingLoans.Count);
					SendStatusMailToCustomer(customer, outstandingLoans, closedLoans, firstOfMonthStatusMailCopyTo, firstOfMonthEnableCustomerMail);
				}
			}
		}

		public static List<Loan> GetOutstandingLoans(int customerId) {
			return ObjectFactory.GetInstance<LoanRepository>()
				.ByCustomer(customerId)
				.Where(l => l.Status != LoanStatus.PaidOff)
				.ToList();
		} // GetOutstandingLoans

		private static List<Loan> GetLastMonthClosedLoans(int customerId) {
			DateTime now = DateTime.UtcNow;
			DateTime startOfMonth = new DateTime(now.Year, now.Month, 1);
			DateTime endOfLastMonth = startOfMonth.Subtract(TimeSpan.FromMilliseconds(1));
			DateTime startOfLastMonth = new DateTime(endOfLastMonth.Year, endOfLastMonth.Month, 1);

			return ObjectFactory.GetInstance<LoanRepository>().ByCustomer(customerId)
				.Where(l => l.Status == LoanStatus.PaidOff && l.DateClosed.HasValue && l.DateClosed >= startOfLastMonth && l.DateClosed <= endOfLastMonth)
				.ToList();
		} // GetLastMonthClosedLoans

		private static string CreateHtmlTable(string body, params string[] headers) {
			var tableHeader = new StringBuilder();
			tableHeader.Append("<table style=\"*border-collapse: collapse;background-color: transparent;border: 1px solid #ddd;border-collapse: separate;border-left: 0;border-radius: 4px;border-spacing: 0;font-family: tahoma;margin-bottom: 14px;max-width: 100%;-moz-border-radius: 4px;-webkit-border-radius: 4px;\">").Append("<thead>").Append("<tr style=\"background-color: #9ab1d1;color: #fff;\">");
			foreach (string header in headers) {
				tableHeader.Append("<th style=\"border-left: 1px solid #ddd;border-top: 0;border-top-left-radius: 4px;font-weight: bold;line-height: 14px;-moz-border-radius-topleft: 4px;padding: 8px;padding-right: 15px;text-align: left;vertical-align: top;-webkit-border-top-left-radius: 4px;\">").Append(header).Append("</th>");
			}
			tableHeader.Append("</tr>").Append("</thead>").Append("<tbody>").Append(body).Append("</tbody>").Append("</table>");
			return tableHeader.ToString();
		}

		private void AddTd(StringBuilder body, bool isNumber, bool isLate, bool isDone, string content) {
			string color = isLate ? "color:red;" : (isDone ? "color:green;" : string.Empty);
			string align = isNumber ? "text-align: right;" : "text-align: left;";
			string style = string.Format("<td style=\"border-left: 1px solid #ddd;border-top: 1px solid #ddd;line-height: 14px;padding: 8px;vertical-align: top;{0}{1}\">", color, align);
			body.Append(style).Append(content).Append("</td>");
		}

		private string CreateHtmlTableFromClass(IEnumerable<LoanStatusRow> listOfInstances) {
			var headers = new List<string> { "Type", "Date", "Status", "Principal", "Interest", "Fees", "Total", "Description" };

			var body = new StringBuilder();
			foreach (LoanStatusRow instance in listOfInstances) {
				body.Append("<tr>");
				bool isLate = instance.Status == "Late";
				bool isDone = instance.Status == "Done" || instance.Status == "Paid" || instance.Status == "PaidOnTime" || instance.Status == "PaidEarly";
				AddTd(body, false, isLate, isDone, instance.Type);
				AddTd(body, false, isLate, isDone, instance.PostDate.ToString("dd-MMM-yyyy"));
				AddTd(body, false, isLate, isDone, instance.Status);
				AddTd(body, true, isLate, isDone, instance.Principal);
				AddTd(body, true, isLate, isDone, instance.Interest);
				AddTd(body, true, isLate, isDone, instance.Fees);
				AddTd(body, true, isLate, isDone, instance.Total);
				AddTd(body, false, isLate, isDone, instance.Description);

				body.Append("</tr>");
			}

			return CreateHtmlTable(body.ToString(), headers.ToArray());
		}

		private LoanStatusRow CreateLoanStatusRowFromTransaction(LoanTransaction loanTransaction) {
			var currentRow = new LoanStatusRow {
				PostDate = loanTransaction.PostDate,
				Description = loanTransaction.Description,
				Fees = FormatNumberWithDash(loanTransaction.Fees)
			};

			var pacnetTransaction = loanTransaction as PacnetTransaction;
			if (pacnetTransaction == null) {
				var paypointTransaction = loanTransaction as PaypointTransaction;
				if (paypointTransaction == null) {
					return null;
				}

				currentRow.Type = "Payment";
				currentRow.Interest = FormatNumberWithDash(paypointTransaction.Interest);
				currentRow.Principal = FormatNumberWithDash(paypointTransaction.LoanRepayment);
				currentRow.Total = FormatNumberWithDash(paypointTransaction.LoanRepayment + paypointTransaction.Interest + loanTransaction.Fees);
				currentRow.Status = paypointTransaction.Status.ToString();
			} else {
				currentRow.Type = "Loan";
				currentRow.Interest = "-";
				currentRow.Principal = "-";
				currentRow.Total = "-";
				currentRow.Status = pacnetTransaction.Status.ToString();
			}

			return currentRow;
		}

		private string FormatNumberWithDash(decimal num) {
			return num == 0 ? "-" : num.ToString("N2", CultureInfo.InvariantCulture);
		}

		private string GenerateClosedLoansSection(List<Loan> closedLoans) {
			bool isFirst = true;
			var closedLoansSection = new StringBuilder();
			if (closedLoans.Count > 0) {
				closedLoansSection.Append("Loans that were closed last month:<br>");
				foreach (Loan closedLoan in closedLoans) {
					if (!isFirst) {
						closedLoansSection.Append("<br>");
					}
					isFirst = false;
					var rows = new List<LoanStatusRow>();
					foreach (LoanTransaction loanTransaction in closedLoan.Transactions.Where(lt => lt.Status == LoanTransactionStatus.Done)) {
						LoanStatusRow currentRow = CreateLoanStatusRowFromTransaction(loanTransaction);
						if (currentRow != null) {
							rows.Add(currentRow);
						}
					}
					string tableForLoan = CreateHtmlTableFromClass(rows.OrderBy(p => p.PostDate));
					closedLoansSection.Append(tableForLoan);
				}
			}
			return closedLoansSection.ToString();
		}

		private string GenerateHeaderSection(Customer customer, List<Loan> outstandingLoans) {
			bool isLate = outstandingLoans.Any(l => l.Status == LoanStatus.Late);
			if (isLate) {
				return "Here is the status of your account and we see that you are late for a payment. Please deal with this today or contact us at <a href=\"mailto:customercare@ezbob.com\" target=\"_self\">customercare@ezbob.com</a><";
			}

			decimal outstandingPrincipal = outstandingLoans.Sum(l => l.Principal);
			decimal approvedSum = customer.LastCashRequest.ApprovedSum();
			if (approvedSum > 2 * outstandingPrincipal) {
				return "Here is the status of your account and we see that you can get more funding with us today. If you \"click for cash\" you can see what we can offer you.";
			}

			return "Here is the status of your account with us as of today...";
		}

		private string GenerateOutstandingLoansSection(List<Loan> outstandingLoans) {
			bool isFirst = true;
			var outstandingLoansSection = new StringBuilder();
			if (outstandingLoans.Count > 0) {
				outstandingLoansSection.Append("Loans that are outstanding:<br>");
				foreach (Loan outstandingLoan in outstandingLoans) {
					if (!isFirst) {
						outstandingLoansSection.Append("<br>");
					}
					isFirst = false;
					var rows = new List<LoanStatusRow>();
					foreach (
						LoanTransaction loanTransaction in
							outstandingLoan.Transactions.Where(lt => lt.Status == LoanTransactionStatus.Done)) {
						LoanStatusRow currentRow = CreateLoanStatusRowFromTransaction(loanTransaction);
						if (currentRow != null) {
							rows.Add(currentRow);
						}
					}

					foreach (
						var loanSchedule in
							outstandingLoan.Schedule.Where(
								ls =>
								ls.Status != LoanScheduleStatus.Paid && ls.Status != LoanScheduleStatus.PaidEarly &&
								ls.Status != LoanScheduleStatus.PaidOnTime)) {
						rows.Add(new LoanStatusRow {
							Type = "Schedule",
							PostDate = loanSchedule.Date,
							Description = string.Empty,
							Fees = FormatNumberWithDash(loanSchedule.Fees),
							Interest = FormatNumberWithDash(loanSchedule.Interest),
							Principal = FormatNumberWithDash(loanSchedule.LoanRepayment),
							Status = loanSchedule.Status.ToString(),
							Total = FormatNumberWithDash(loanSchedule.LoanRepayment + loanSchedule.Interest + loanSchedule.Fees)
						});
					}

					string tableForLoan = CreateHtmlTableFromClass(rows.OrderBy(p => p.PostDate));
					outstandingLoansSection.Append(tableForLoan);
				}
			}
			return outstandingLoansSection.ToString();
		}

		private string GenerateSummarySection(IEnumerable<Loan> outstandingLoans) {
			var summarySection = new StringBuilder();
			var loanSummaryRows = new StringBuilder();
			var summaryHeaders = new List<string> { "Loan", "Date", "Balance", "Outstanding Principal", "Next Repayment", "Next Payment Date" };

			decimal totalBalance = 0;
			decimal totalNextRepayment = 0;
			decimal totalPrincipal = 0;
			DateTime? totalNextSchedulePaymentDate = null;
			foreach (Loan l in outstandingLoans) {
				var nextSchedulePaymentDate =
					l.Schedule.Where(
						ls =>
						ls.Status != LoanScheduleStatus.Paid && ls.Status != LoanScheduleStatus.PaidEarly &&
						ls.Status != LoanScheduleStatus.PaidOnTime).OrderBy(ls => ls.Date).FirstOrDefault();
				loanSummaryRows.Append("<tr>");

				AddTd(loanSummaryRows, true, false, false, l.RefNumber);
				AddTd(loanSummaryRows, false, false, false, l.Date.ToString("dd-MMM-yyyy"));
				AddTd(loanSummaryRows, true, false, false, FormatNumberWithDash(l.Balance));
				AddTd(loanSummaryRows, true, false, false, FormatNumberWithDash(l.Principal));
				AddTd(loanSummaryRows, true, false, false, FormatNumberWithDash(l.NextRepayment));
				AddTd(loanSummaryRows, false, false, false, nextSchedulePaymentDate == null ? "-" : nextSchedulePaymentDate.Date.ToString("dd-MMM-yyyy"));

				loanSummaryRows.Append("</tr>");
				totalBalance += l.Balance;
				totalPrincipal += l.Principal;
				if ((nextSchedulePaymentDate != null) &&
					(!totalNextSchedulePaymentDate.HasValue || totalNextSchedulePaymentDate.Value > nextSchedulePaymentDate.Date)) {
					totalNextSchedulePaymentDate = nextSchedulePaymentDate.Date;
					totalNextRepayment = l.NextRepayment;
				}
			}

			var loanSummaryTotalRow = new StringBuilder();
			loanSummaryTotalRow.Append("<tr>");

			AddTd(loanSummaryRows, false, false, false, "Total");
			AddTd(loanSummaryRows, false, false, false, "-");
			AddTd(loanSummaryRows, true, false, false, FormatNumberWithDash(totalBalance));
			AddTd(loanSummaryRows, true, false, false, FormatNumberWithDash(totalPrincipal));
			AddTd(loanSummaryRows, true, false, false, FormatNumberWithDash(totalNextRepayment));
			AddTd(loanSummaryRows, false, false, false, totalNextSchedulePaymentDate != null
										   ? totalNextSchedulePaymentDate.Value.ToString("dd-MMM-yyyy")
										   : "-");

			loanSummaryTotalRow.Append("</tr>");
			summarySection.Append(loanSummaryTotalRow);
			summarySection.Append(loanSummaryRows);

			return "Loans summary:<br>" + CreateHtmlTable(summarySection.ToString(), summaryHeaders.ToArray());
		}

		private void SendStatusMail(string toAddress, string firstName, string headerSection, string loanSummarySection, string closedLoansSection, string outstandingLoansSection) {
			string firstOfMonthStatusMailMandrillTemplateName = CurrentValues.Instance.FirstOfMonthStatusMailMandrillTemplateName;
			var mail = new Mail();

			var vars = new Dictionary<string, string>
				{
					{"FirstName", firstName},
					{"HeaderSection", headerSection},
					{"LoanSummarySection", loanSummarySection},
					{"ClosedLoansSection", closedLoansSection},
					{"OutstandingLoansSection", outstandingLoansSection} 
				};

			var result = mail.Send(vars, toAddress, firstOfMonthStatusMailMandrillTemplateName);
			if (result == "OK") {
				log.InfoFormat("Sent mail - {0}", firstOfMonthStatusMailMandrillTemplateName);
			} else {
				log.ErrorFormat("Failed sending alert mail - {0}. Result:{1}", result, firstOfMonthStatusMailMandrillTemplateName);
			}
		}

		private void SendStatusMailToCustomer(Customer customer, List<Loan> outstandingLoans, List<Loan> closedLoans,
											  string copyToAddress, bool shouldSendToCustomer) {
			log.InfoFormat("Preparing first of month mail for customer:{0}", customer.Id);

			string headerSection = GenerateHeaderSection(customer, outstandingLoans);
			string summarySection = GenerateSummarySection(outstandingLoans);
			string closedLoansSection = GenerateClosedLoansSection(closedLoans);
			string outstandingLoansSection = GenerateOutstandingLoansSection(outstandingLoans);

			if (shouldSendToCustomer) {
				SendStatusMail(customer.Name, customer.PersonalInfo.FirstName, headerSection, summarySection, closedLoansSection,
							   outstandingLoansSection);
			}
			if (!string.IsNullOrEmpty(copyToAddress)) {
				SendStatusMail(copyToAddress, customer.PersonalInfo.FirstName, headerSection, summarySection, closedLoansSection,
							   outstandingLoansSection);
			}
		}

		private static readonly ILog log = LogManager.GetLogger(typeof(FirstOfMonthStatusStrategyHelper));
		private readonly CustomerRepository _customers;
	}

	public class LoanStatusRow {
		public string Description { get; set; }

		public string Fees { get; set; }

		public string Interest { get; set; }

		public DateTime PostDate { get; set; }

		public string Principal { get; set; }

		public string Status { get; set; }

		public string Total { get; set; }

		public string Type { get; set; }
	}
}