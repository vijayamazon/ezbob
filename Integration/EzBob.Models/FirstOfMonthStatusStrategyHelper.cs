﻿namespace EzBob.Models
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Text;
	using EZBob.DatabaseLib.Model;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using StructureMap;
	using log4net;
	using MailApi;

	public class FirstOfMonthStatusStrategyHelper
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(FirstOfMonthStatusStrategyHelper));
        private readonly CustomerRepository _customers;
		private readonly ConfigurationVariablesRepository configurationVariablesRepository;
		private readonly StrategyHelper strategyHelper = new StrategyHelper();

		public FirstOfMonthStatusStrategyHelper()
        {
            _customers = ObjectFactory.GetInstance<CustomerRepository>();
			configurationVariablesRepository = ObjectFactory.GetInstance<ConfigurationVariablesRepository>();
        }
		
		public void SendFirstOfMonthStatusMail()
		{
			bool firstOfMonthStatusMailEnabled = configurationVariablesRepository.GetByNameAsBool("FirstOfMonthStatusMailEnabled");
			string firstOfMonthStatusMailCopyTo = configurationVariablesRepository.GetByName("FirstOfMonthStatusMailCopyTo").Value;
			bool firstOfMonthEnableCustomerMail = configurationVariablesRepository.GetByNameAsBool("FirstOfMonthEnableCustomerMail");

			var customersToWorkOn = new List<Customer>();
			
			if (firstOfMonthStatusMailEnabled)
			{
				customersToWorkOn.AddRange(_customers.GetAll().Where(c => !c.IsTest && c.CollectionStatus.CurrentStatus.IsEnabled && (!c.MonthlyStatusEnabled.HasValue || c.MonthlyStatusEnabled.Value)));
			}
			else
			{
				customersToWorkOn.AddRange(_customers.GetAll().Where(c => !c.IsTest && c.CollectionStatus.CurrentStatus.IsEnabled && c.MonthlyStatusEnabled.HasValue && c.MonthlyStatusEnabled.Value));
			}

			log.InfoFormat("Starting to send first of month status mails");
			foreach (Customer customer in customersToWorkOn)
			{
				List<Loan> outstandingLoans = strategyHelper.GetOutstandingLoans(customer.Id);
				if (outstandingLoans.Count > 0)
				{
					List<Loan> closedLoans = strategyHelper.GetLastMonthClosedLoans(customer.Id);
					log.InfoFormat("Customer {0} has {1} outstanding loans. Will send status mail to him", customer.Id, outstandingLoans.Count);
					SendStatusMailToCustomer(customer, outstandingLoans, closedLoans, firstOfMonthStatusMailCopyTo, firstOfMonthEnableCustomerMail);
				}
			}
		}

		private string FormatNumberWithDash(decimal num)
		{
			return num == 0 ? "-" : num.ToString("N2", CultureInfo.InvariantCulture);
		}

		private LoanStatusRow CreateLoanStatusRowFromTransaction(LoanTransaction loanTransaction)
		{
			var currentRow = new LoanStatusRow
			{
				PostDate = loanTransaction.PostDate,
				Description = loanTransaction.Description,
				Fees = FormatNumberWithDash(loanTransaction.Fees)
			};

			var pacnetTransaction = loanTransaction as PacnetTransaction;
			if (pacnetTransaction == null)
			{
				var paypointTransaction = loanTransaction as PaypointTransaction;
				if (paypointTransaction == null)
				{
					return null;
				}

				currentRow.Type = "Payment";
				currentRow.Interest = FormatNumberWithDash(paypointTransaction.Interest);
				currentRow.Principal = FormatNumberWithDash(paypointTransaction.LoanRepayment);
				currentRow.Total = FormatNumberWithDash(paypointTransaction.LoanRepayment + paypointTransaction.Interest + loanTransaction.Fees);
				currentRow.Status = paypointTransaction.Status.ToString();
			}
			else
			{
				currentRow.Type = "Loan";
				currentRow.Interest = "-";
				currentRow.Principal = "-";
				currentRow.Total = "-";
				currentRow.Status = pacnetTransaction.Status.ToString();
			}

			return currentRow;
		}

		private string GenerateSummarySection(IEnumerable<Loan> outstandingLoans)
		{
			var summarySection = new StringBuilder();
			var loanSummaryRows = new StringBuilder();
			var summaryHeaders = new List<string> { "Loan", "Date", "Balance", "Outstanding Principal", "Next Repayment", "Next Payment Date" };
			
			decimal totalBalance = 0;
			decimal totalNextRepayment = 0;
			decimal totalPrincipal = 0;
			DateTime? totalNextSchedulePaymentDate = null;
			foreach (Loan l in outstandingLoans)
			{
				var nextSchedulePaymentDate =
					l.Schedule.Where(
						ls =>
						ls.Status != LoanScheduleStatus.Paid && ls.Status != LoanScheduleStatus.PaidEarly &&
						ls.Status != LoanScheduleStatus.PaidOnTime).OrderBy(ls => ls.Date).FirstOrDefault();
				loanSummaryRows.Append("<tr>");

				AddTd(loanSummaryRows, true, false, false, l.RefNumber);
				AddTd(loanSummaryRows, false, false, false, l.Date.ToString("dd-MMM-yyyy"));
				AddTd(loanSummaryRows, true, false, false, l.Balance.ToString(CultureInfo.InvariantCulture));
				AddTd(loanSummaryRows, true, false, false, l.Principal.ToString(CultureInfo.InvariantCulture));
				AddTd(loanSummaryRows, true, false, false, l.NextRepayment.ToString(CultureInfo.InvariantCulture));
				AddTd(loanSummaryRows, false, false, false, nextSchedulePaymentDate == null ? "-" : nextSchedulePaymentDate.Date.ToString("dd-MMM-yyyy"));
				
				loanSummaryRows.Append("</tr>");
				totalBalance += l.Balance;
				totalPrincipal += l.Principal;
				if ((nextSchedulePaymentDate != null) &&
					(!totalNextSchedulePaymentDate.HasValue || totalNextSchedulePaymentDate.Value > nextSchedulePaymentDate.Date))
				{
					totalNextSchedulePaymentDate = nextSchedulePaymentDate.Date;
					totalNextRepayment = l.NextRepayment;
				}
			}

			var loanSummaryTotalRow = new StringBuilder();
			loanSummaryTotalRow.Append("<tr>");

			AddTd(loanSummaryRows, false, false, false, "Total");
			AddTd(loanSummaryRows, false, false, false, "-");
			AddTd(loanSummaryRows, true, false, false, totalBalance.ToString(CultureInfo.InvariantCulture));
			AddTd(loanSummaryRows, true, false, false, totalPrincipal.ToString(CultureInfo.InvariantCulture));
			AddTd(loanSummaryRows, true, false, false, totalNextRepayment.ToString(CultureInfo.InvariantCulture));
			AddTd(loanSummaryRows, false, false, false, totalNextSchedulePaymentDate != null
										   ? totalNextSchedulePaymentDate.Value.ToString("dd-MMM-yyyy")
										   : "-");

			loanSummaryTotalRow.Append("</tr>");
			summarySection.Append(loanSummaryTotalRow);
			summarySection.Append(loanSummaryRows);

			return "Loans summary:<br>" + CreateHtmlTable(summarySection.ToString(), summaryHeaders.ToArray()) + "<br>";
		}

		private void SendStatusMailToCustomer(Customer customer, List<Loan> outstandingLoans, List<Loan> closedLoans,
		                                      string copyToAddress, bool shouldSendToCustomer)
		{
			log.InfoFormat("Preparing first of month mail for customer:{0}", customer.Id);

			string headerSection = GenerateHeaderSection(customer, outstandingLoans);
			string summarySection = GenerateSummarySection(outstandingLoans);
			string closedLoansSection = GenerateClosedLoansSection(closedLoans);
			string outstandingLoansSection = GenerateOutstandingLoansSection(outstandingLoans);

			if (shouldSendToCustomer)
			{
				SendStatusMail(customer.Name, customer.PersonalInfo.FirstName, headerSection, summarySection, closedLoansSection,
				               outstandingLoansSection);
			}
			if (!string.IsNullOrEmpty(copyToAddress))
			{
				SendStatusMail(copyToAddress, customer.PersonalInfo.FirstName, headerSection, summarySection, closedLoansSection,
				               outstandingLoansSection);
			}
		}

		private string GenerateHeaderSection(Customer customer, List<Loan> outstandingLoans)
		{
			string s = "general header + contact us details";
			bool isLate = outstandingLoans.Any(x => x.Status == LoanStatus.Late);
			
			decimal outstandingPrincipal = outstandingLoans.Sum(x => x.Principal);
			decimal approvedSum = customer.LastCashRequest.ApprovedSum();
			if (approvedSum > 2*outstandingPrincipal)
			{
				//can get more money
			}

			if (isLate)
			{
				//is late
			}

			return s;
		}

		private string GenerateOutstandingLoansSection(List<Loan> outstandingLoans)
		{
			var outstandingLoansSection = new StringBuilder();
			if (outstandingLoans.Count > 0)
			{
				outstandingLoansSection.Append("Loans that are outstanding:<br>");
				foreach (Loan outstandingLoan in outstandingLoans)
				{
					var rows = new List<LoanStatusRow>();
					foreach (
						LoanTransaction loanTransaction in
							outstandingLoan.Transactions.Where(lt => lt.Status == LoanTransactionStatus.Done))
					{
						LoanStatusRow currentRow = CreateLoanStatusRowFromTransaction(loanTransaction);
						if (currentRow != null)
						{
							rows.Add(currentRow);
						}
					}

					foreach (
						var loanSchedule in
							outstandingLoan.Schedule.Where(
								ls =>
								ls.Status != LoanScheduleStatus.Paid && ls.Status != LoanScheduleStatus.PaidEarly &&
								ls.Status != LoanScheduleStatus.PaidOnTime))
					{
						rows.Add(new LoanStatusRow
							{
								Type = "Schedule",
								PostDate = loanSchedule.Date,
								Description = string.Empty,
								Fees = loanSchedule.Fees.ToString("N2", CultureInfo.InvariantCulture),
								Interest = loanSchedule.Interest.ToString("N2", CultureInfo.InvariantCulture),
								Principal = loanSchedule.LoanRepayment.ToString("N2", CultureInfo.InvariantCulture),
								Status = loanSchedule.Status.ToString(),
								Total =
									(loanSchedule.LoanRepayment + loanSchedule.Interest + loanSchedule.Fees).ToString("N2",
									                                                                                  CultureInfo.InvariantCulture)
							});
					}

					string tableForLoan = CreateHtmlTableFromClass(rows.OrderBy(p => p.PostDate));
					outstandingLoansSection.Append(tableForLoan).Append("<br>");
				}
			}
			return outstandingLoansSection.ToString();
		}

		private string GenerateClosedLoansSection(List<Loan> closedLoans)
		{
			var closedLoansSection = new StringBuilder();
			if (closedLoans.Count > 0)
			{
				closedLoansSection.Append("Loans that were closed last month:<br>");
				foreach (Loan closedLoan in closedLoans)
				{
					var rows = new List<LoanStatusRow>();
					foreach (
						LoanTransaction loanTransaction in closedLoan.Transactions.Where(lt => lt.Status == LoanTransactionStatus.Done))
					{
						LoanStatusRow currentRow = CreateLoanStatusRowFromTransaction(loanTransaction);
						if (currentRow != null)
						{
							rows.Add(currentRow);
						}
					}
					string tableForLoan = CreateHtmlTableFromClass(rows.OrderBy(p => p.PostDate));
					closedLoansSection.Append(tableForLoan).Append("<br>");
				}
				closedLoansSection.Append("<br><br>");
			}
			return closedLoansSection.ToString();
		}

		private void AddTd(StringBuilder body, bool isNumber, bool isLate, bool isDone, string content)
		{
			string color = isLate ? "color:red;" : (isDone ? "color:green;" : string.Empty);
			string align = isNumber ? "text-align: right;" : "text-align: left;";
			string style = string.Format("<td style=\"border-left: 1px solid #ddd;border-top: 1px solid #ddd;line-height: 14px;padding: 8px;vertical-align: top;{0}{1}\">", color, align);
			body.Append(style).Append(content).Append("</td>");
		}

		private string CreateHtmlTableFromClass(IEnumerable<LoanStatusRow> listOfInstances)
		{
			var headers = new List<string> { "Type", "Date", "Status", "Principal", "Interest", "Fees", "Total", "Description" };
			
			var body = new StringBuilder();
			foreach (LoanStatusRow instance in listOfInstances)
			{
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

		private static string CreateHtmlTable(string body, params string[] headers)
		{
			var tableHeader = new StringBuilder();
			tableHeader.Append("<table style=\"*border-collapse: collapse;background-color: transparent;border: 1px solid #ddd;border-collapse: separate;border-left: 0;border-radius: 4px;border-spacing: 0;font-family: tahoma;margin-bottom: 14px;max-width: 100%;-moz-border-radius: 4px;-webkit-border-radius: 4px;\">").Append("<thead>").Append("<tr style=\"background-color: #9ab1d1;color: #fff;\">");
			foreach (string header in headers)
			{
				tableHeader.Append("<th style=\"border-left: 1px solid #ddd;border-top: 0;border-top-left-radius: 4px;font-weight: bold;line-height: 14px;-moz-border-radius-topleft: 4px;padding: 8px;padding-right: 15px;text-align: left;vertical-align: top;-webkit-border-top-left-radius: 4px;\">").Append(header).Append("</th>");
			}
			tableHeader.Append("</tr>").Append("</thead>").Append("<tbody>").Append(body).Append("</tbody>").Append("</table>");
			return tableHeader.ToString();
		}

		private void SendStatusMail(string toAddress, string firstName, string headerSection, string loanSummarySection, string closedLoansSection, string outstandingLoansSection)
		{
			string firstOfMonthStatusMailMandrillTemplateName = configurationVariablesRepository.GetByName("FirstOfMonthStatusMailMandrillTemplateName").Value;
			var mail = ObjectFactory.GetInstance<IMail>();

			var vars = new Dictionary<string, string>
				{
					{"FirstName", firstName},
					{"HeaderSection", headerSection},
					{"LoanSummarySection", loanSummarySection},
					{"ClosedLoansSection", closedLoansSection},
					{"OutstandingLoansSection", outstandingLoansSection} 
				};

			var result = mail.Send(vars, toAddress, firstOfMonthStatusMailMandrillTemplateName);
			if (result == "OK")
			{
				log.InfoFormat("Sent mail - {0}", firstOfMonthStatusMailMandrillTemplateName);
			}
			else
			{
				log.ErrorFormat("Failed sending alert mail - {0}. Result:{1}", result, firstOfMonthStatusMailMandrillTemplateName);
			}
		}

		public void NotifyAutoApproveSilentMode(int customerId, int autoApproveAmount, string autoApproveSilentTemplateName, string autoApproveSilentToAddress)
		{
			try
			{
				log.InfoFormat("Sending silent auto approval mail for: customerId={0} autoApproveAmount={1} autoApproveSilentTemplateName={2} autoApproveSilentToAddress={3}", customerId, autoApproveAmount, autoApproveSilentTemplateName, autoApproveSilentToAddress);
				var mail = ObjectFactory.GetInstance<IMail>();
				var vars = new Dictionary<string, string>
				{
					{"customerId", customerId.ToString(CultureInfo.InvariantCulture)},
					{"autoApproveAmount", autoApproveAmount.ToString(CultureInfo.InvariantCulture)}
				};

				var result = mail.Send(vars, autoApproveSilentToAddress, autoApproveSilentTemplateName);
				if (result == "OK")
				{
					log.InfoFormat("Sent mail - silent auto approval");
				}
				else
				{
					log.ErrorFormat("Failed sending alert mail - silent auto approval. Result:{0}", result);
				}
			}
			catch (Exception e)
			{
				log.ErrorFormat("Failed sending alert mail - silent auto approval. Exception:{0}", e);
			}
		}
	}

	public class LoanStatusRow
	{
		public string Type { get; set; }
		public DateTime PostDate { get; set; }
		public string Principal { get; set; }
		public string Interest { get; set; }
		public string Fees { get; set; }
		public string Total { get; set; }
		public string Status { get; set; }
		public string Description { get; set; }
	}
}