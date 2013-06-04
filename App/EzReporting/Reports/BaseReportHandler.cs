using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Ezbob.Database;
using Ezbob.Logger;

namespace Reports {
	using Mailer;

	public class BaseReportHandler : SafeLog {
		public const string DefaultToEMail = "dailyreports@ezbob.com";
		public const string DefaultFromEMailPassword = "ezbob2012";
		public const string DefaultFromEMail = "ezbob@ezbob.com";

		private static string Lock = "";

		public BaseReportHandler(AConnection oDB, ASafeLog log = null) : base(log) {
			DB = oDB;
		} // constructor

		protected AConnection DB { get; private set; }

		public string BuildPlainedPaymentReport(Report report, string today, string tomorrow) {
			var bodyText = new StringBuilder();
			bodyText.Append(ReportsStyling.BodyHtmlStyle).Append("<h1> ").Append(report.Title).Append(" ").Append(today).Append("</h1>");
			bodyText = PaymentReport(bodyText, today);
			bodyText.Append("</body>");
			return bodyText.ToString();
		} // BuildPlainedPaymentReport

		public void AddReportToList(List<Report> reportList, DataRow row, string defaultToEmail) {
			ReportType type;

			if (!Enum.TryParse<ReportType>(row["Type"].ToString(), out type))
				type = ReportType.RPT_GENERIC;

			reportList.Add(new Report {
				Type = type,
				Title = row["Title"].ToString(),
				StoredProcedure = row["StoredProcedure"].ToString(),
				IsDaily = (bool)row["IsDaily"],
				IsWeekly = (bool)row["IsWeekly"],
				IsMonthly = (bool)row["IsMonthly"],
				Headers = row["Header"].ToString().Split(','),
				Fields = row["Fields"].ToString().Split(','),
				ToEmail = (string.IsNullOrEmpty(row["ToEmail"].ToString()) ? defaultToEmail : row["ToEmail"].ToString()),
				IsMonthToDate = (bool)row["IsMonthToDate"]
			});
		} // AddReportToList

		public void SendReport(string subject, string mailBody, string toAddressStr = DefaultToEMail, string period = "Daily") {
			lock (Lock) {
				Mailer.SendMail(DefaultFromEMail, DefaultFromEMailPassword, "EZBOB " + period + " Client Report", mailBody, toAddressStr);
				Debug("Mail {0} sent to: {1}", subject, toAddressStr);
			} // lock
		} // SendReport

		public string BuildNewClientReport(Report report, string today, string tomorrow) {
			var bodyText = new StringBuilder();
			bodyText.Append(ReportsStyling.BodyHtmlStyle).Append("<h1>").Append(report.Title).Append(" ").Append(today).Append("</h1>");
			AdsReport(bodyText, today);
			bodyText.Append("<p>");
			CustomerReport(bodyText, today);
			bodyText.Append("</body>");

			return bodyText.ToString();
		} // BuildNewClientReport

		private void CustomerReport(StringBuilder bodyText, String today) {
			try {
				DataTable dt = DB.ExecuteReader("RptCustomerReport", new QueryParameter("@DateStart", today));

				int lineCounter = 0;
				foreach (DataRow row in dt.Rows) {
					if (lineCounter == 0) {
						bodyText.Append("<table style=").Append(ReportsStyling.TableHtmlStyle).Append("><thead>").Append(ReportsStyling.TrHeadHtmlStyle);
						bodyText.Append(ReportsStyling.ThHtmlStyle).Append("Email </th>").
								 Append(ReportsStyling.ThHtmlStyle).Append("Status </th>").
								 Append(ReportsStyling.ThHtmlStyle).Append("Wizard Finished </th>").
								 Append(ReportsStyling.ThHtmlStyle).Append("Account # </th>").
								 Append(ReportsStyling.ThHtmlStyle).Append("Credit Offer </th>").
								 Append(ReportsStyling.ThHtmlStyle).Append("Source Ad </th>");
						bodyText.Append("</tr></thead><tbody>");
					}
					bodyText.Append("<tr>");
					bodyText.Append(ReportsStyling.TdHtmlStyle).Append(row["Name"]).Append("</td>");
					bodyText.Append(ReportsStyling.TdHtmlStyle).Append(row["Status"]).Append("</td>");
					bodyText.Append(ReportsStyling.TdHtmlStyle).Append(row["IsSuccessfullyRegistered"]).Append("</td>");
					bodyText.Append(ReportsStyling.TdHtmlStyle).Append(row["AccountNumber"]).Append("</td>");
					bodyText.Append(ReportsStyling.TdAlignRightHtmlStyle).Append(row["CreditSum"]).Append("</td>");
					bodyText.Append(ReportsStyling.TdHtmlStyle).Append(row["ReferenceSource"]).Append("</td>");
					bodyText.Append("</tr>");
					lineCounter++;
				}
				bodyText.Append("</tbody></table>");
			}
			catch (Exception e) {
				Error(e.ToString());
			}
		} // CustomerReport

		private void AdsReport(StringBuilder bodyText, String today) {
			try {
				DataTable dt = DB.ExecuteReader("RptAdsReport", new QueryParameter("@time", today));

				int lineCounter = 0;
				foreach (DataRow row in dt.Rows) {
					if (lineCounter == 0) {
						bodyText.Append("<table style=").Append(ReportsStyling.TableHtmlStyle).Append("><thead>").Append(ReportsStyling.TrHeadHtmlStyle);
						bodyText.Append(ReportsStyling.ThHtmlStyle).Append("Ad Name </th>").
								 Append(ReportsStyling.ThHtmlStyle).Append(" # </th>").
								 Append(ReportsStyling.ThHtmlStyle).Append("Total Credit Approved </th>");
						bodyText.Append("</tr></thead><tbody>");
					}
					bodyText.Append("<tr>");
					bodyText.Append(ReportsStyling.TdHtmlStyle).Append(row["ReferenceSource"]).Append("</td>");
					bodyText.Append(ReportsStyling.TdHtmlStyle).Append(row["TotalUsers"]).Append("</td>");
					bodyText.Append(ReportsStyling.TdAlignRightHtmlStyle).Append(row["TotalCredit"]).Append("</td>");
					bodyText.Append("</tr>");
					lineCounter++;
				}
				bodyText.Append("</tbody></table>");
			}
			catch (Exception e) {
				Error(e.ToString());
			}
		} // AdsReport

		private StringBuilder PaymentReport(StringBuilder bodyText, String today) {
			try {
				DataTable dt = DB.ExecuteReader("RptPaymentReport",
					new QueryParameter("@DateStart", today),
					new QueryParameter("@DateEnd", DateTime.Today.AddDays(3).ToString("yyyy-MM-dd"))
				);

				int lineCounter = 0;
				foreach (DataRow row in dt.Rows) {
					if (lineCounter == 0) {
						bodyText.Append("<table id='tableReportData' style=").Append(ReportsStyling.TableHtmlStyle).Append("><thead>").Append(ReportsStyling.TrHeadHtmlStyle);
						bodyText.Append(ReportsStyling.ThHtmlStyle).Append(" Id </th>").
								 Append(ReportsStyling.ThHtmlStyle).Append(" Name </th>").
								 Append(ReportsStyling.ThHtmlStyle).Append(" Email </th>").
								 Append(ReportsStyling.ThHtmlStyle).Append(" Date </th>").
								 Append(ReportsStyling.ThHtmlStyle).Append(" Amount </th>");
						bodyText.Append("</tr><thead><tbody>");
					}
					bodyText.Append("<tr>");
					bodyText.Append(ReportsStyling.TdHtmlStyle).Append(row["Id"]).Append("</td>");
					bodyText.Append(ReportsStyling.TdHtmlStyle).Append(row["Firstname"]).Append(" ").Append(row["SurName"]).Append("</td>");
					bodyText.Append(ReportsStyling.TdHtmlStyle).Append(row["Name"]).Append("</td>");
					bodyText.Append(ReportsStyling.TdHtmlStyle).Append(row["DATE"]).Append("</td>");
					bodyText.Append(ReportsStyling.TdAlignRightHtmlStyle).Append(row["AmountDue"]).Append("</td>");
					bodyText.Append("</tr>");
					lineCounter++;
				}
				bodyText.Append("</tbody></table>");
			}
			catch (Exception e) {
				Error(e.ToString());
			}

			return bodyText;
		} // PaymentReport

		public string BuildDailyStatsReportBody(Report report, string today, string tomorrow) {
			var bodyText = new StringBuilder();
			bodyText.Append(ReportsStyling.BodyHtmlStyle).Append("<h1> ").Append(report.Title).Append(" ").Append(today).Append("</h1>");
			bodyText = DailyStatsReport(bodyText, today, tomorrow);
			bodyText.Append("</body>");
			return bodyText.ToString();
		} // BuildDailyStatsReportBody

		private void TableTD(StringBuilder bodyText, string val) {
			bodyText.Append(ReportsStyling.TdHtmlStyle).Append(val).Append("</td>");
		} // TableID

		private bool IsApplicationHeader(string db, bool isNew) {
			return db == "Applications" && isNew;
		} // IsApplicationHeader

		private bool IsApplicationLine(string db) {
			return db == "Applications";
		} // IsApplicationLine

		private bool IsLoanHeader(string db, bool isNew) {
			return db == "Loans" && isNew;
		} // IsLoanHeader

		private bool IsLoanLine(string db) {
			return db == "Loans";
		} // IsLoanLine

		private bool IsPaymentHeader(string db, bool isNew) {
			return db == "Payments" && isNew;
		} // IsPaymentHeader

		private bool IsPaymentLine(string db) {
			return db == "Payments";
		} // IsPaymentLine

		private bool IsRegisterHeader(string db, bool isNew) {
			return db == "Registers" && isNew;
		} // IsRegisterHeader

		private bool IsRegisterLine(string db) {
			return db == "Registers";
		} // IsRegisterLine

		private StringBuilder DailyStatsReport(StringBuilder bodyText, string today, string tomorrow) {
			try {
				DataTable dt = DB.ExecuteReader("RptDailyStats",
					new QueryParameter("@DateStart", today),
					new QueryParameter("@DateEnd", tomorrow)
				);

				bool firstLoansLine = true;
				bool firstApplicationLine = true;
				bool firstPaymentLine = true;
				bool firstRegisterLine = true;
				foreach (DataRow row in dt.Rows) {
					string firstVal = row[0].ToString();
					if (IsApplicationHeader(firstVal, firstApplicationLine)) {
						bodyText.Append("<p><table id='tableReportData' style=").Append(ReportsStyling.TableHtmlStyle).Append("><thead>").Append(ReportsStyling.TrHeadHtmlStyle);
						bodyText.Append(ReportsStyling.ThHtmlStyle).Append(" Application </th>").
								 Append(ReportsStyling.ThHtmlStyle).Append(" #</th>").
								 Append(ReportsStyling.ThHtmlStyle).Append(" Descision </th>").
								 Append(ReportsStyling.ThHtmlStyle).Append(" Value</th>");
						bodyText.Append("</tr></thead><tbody>");
						firstApplicationLine = false;
					}

					if (IsApplicationLine(firstVal)) {
						bodyText.Append("<tr>");
						TableTD(bodyText, row[1].ToString());
						TableTD(bodyText, row[2].ToString());
						TableTD(bodyText, row[3].ToString());
						TableTD(bodyText, row[4].ToString());
						bodyText.Append("</tr>");

					}

					if (IsLoanHeader(firstVal, firstLoansLine)) {
						bodyText.Append("</tbody></table></p>");
						bodyText.Append("<p><table id='tableReportData' style=").Append(ReportsStyling.TableHtmlStyle).Append("><thead>").Append(ReportsStyling.TrHeadHtmlStyle);
						bodyText.Append(ReportsStyling.ThHtmlStyle).Append(" Loans </th>").
								 Append(ReportsStyling.ThHtmlStyle).Append(" # </th>").
								 Append(ReportsStyling.ThHtmlStyle).Append(" Value </th>");
						bodyText.Append("</tr></thead><tbody>");
						firstLoansLine = false;
					}

					if (IsLoanLine(firstVal)) {
						bodyText.Append("<tr>");
						TableTD(bodyText, row[1].ToString());
						TableTD(bodyText, row[2].ToString());
						TableTD(bodyText, row[4].ToString());
						bodyText.Append("</tr>");

					}

					if (IsPaymentHeader(firstVal, firstPaymentLine)) {
						bodyText.Append("</tbody></table></p>");
						bodyText.Append("<p><table id='tableReportData' style=").Append(ReportsStyling.TableHtmlStyle).Append("><thead>").Append(ReportsStyling.TrHeadHtmlStyle);
						bodyText.Append(ReportsStyling.ThHtmlStyle).Append("Payments </th>").
								 Append(ReportsStyling.ThHtmlStyle).Append(" Value </th>");
						bodyText.Append("</tr></thead><tbody>");
						firstLoansLine = false;
					}

					if (IsPaymentLine(firstVal)) {
						bodyText.Append("<tr>");
						TableTD(bodyText, row[2].ToString());
						TableTD(bodyText, row[4].ToString());
						bodyText.Append("</tr>");

					}

					if (IsRegisterHeader(firstVal, firstRegisterLine)) {
						bodyText.Append("</tbody></table></p>");
						bodyText.Append("<p><table id='tableReportData' style=").Append(ReportsStyling.TableHtmlStyle).Append("><thead>").Append(ReportsStyling.TrHeadHtmlStyle);
						bodyText.Append(ReportsStyling.ThHtmlStyle).Append(" Registers </th>");
						bodyText.Append("</tr></thead><tbody>");
						firstLoansLine = false;
					}

					if (IsRegisterLine(firstVal)) {
						bodyText.Append("<tr>");
						TableTD(bodyText, row[2].ToString());
						bodyText.Append("</tr>");
					}
				}
				bodyText.Append("</tbody></table></p>");

			}
			catch (Exception e) {
				Error(e.ToString());
			}

			return bodyText;
		} // DailyStatsReport

		public string BuildInWizardReport(Report report, string today, string tomorrow) {
			var bodyText = new StringBuilder();
			bodyText.Append(ReportsStyling.BodyHtmlStyle).Append("<h1> ").Append(report.Title).Append(" ").Append(today).Append("</h1>");
			bodyText.Append("<p>");
			bodyText.Append("Clients that enetered Shops but did not complete:");
			bodyText.Append("<p>");
			DailyInWizardReport(bodyText, today, tomorrow);
			bodyText.Append("<p>");
			bodyText.Append("Clients that just enetered their email:");
			bodyText.Append("<p>");
			DailyStep1Report(bodyText, today, tomorrow);
			bodyText.Append("</body>");
			return bodyText.ToString();
		} // BuildInWizardReport

		private void DailyInWizardReport(StringBuilder bodyText, string fromDate, string toDate) {
			string[] headers;
			string[] fields;
			GetHeaderAndFields(ReportType.RPT_IN_WIZARD, out headers, out fields);
			TableReport(bodyText, "RptNewClients", fromDate, toDate, headers, fields, true);
		} // DailyInWizardReport

		private void DailyStep1Report(StringBuilder bodyText, string fromDate, string toDate) {
			string[] headers;
			string[] fields;
			GetHeaderAndFields(ReportType.RPT_IN_WIZARD, out headers, out fields);
			TableReport(bodyText, "RptNewClientsStep1", fromDate, toDate, headers, fields, true);
		} // DailyStep1Report

		private void GetHeaderAndFields(ReportType type, out string[] header, out string[] fields) {
			header = null;
			fields = null;

			DataTable dt = DB.ExecuteReader("RptScheduler_GetHeaderAndFields", new QueryParameter("@Type", type.ToString()));

			foreach (DataRow row in dt.Rows) {
				header = row[0].ToString().Split(',');
				fields = row[1].ToString().Split(',');
			}
		} // GetHeadersAndFields

		public void TableReport(StringBuilder bodyText, string spName, string startDate, string endDate, string[] headers, string[] fields, bool isSharones = false) {
			try {
				DataTable dt = DB.ExecuteReader(spName,
					new QueryParameter("@DateStart", startDate),
					new QueryParameter("@DateEnd", endDate)
				);

				int lineCounter = 0;
				foreach (DataRow row in dt.Rows) {
					if (lineCounter == 0) {
						if (isSharones) {
							bodyText.Append("<table style=").Append(ReportsStyling.TableHtmlStyle).Append("><thead>").Append(ReportsStyling.TrHeadHtmlStyle);
						}
						else {
							bodyText.Append("<table id='tableReportData' style=").Append(ReportsStyling.TableHtmlStyle).Append("><thead>").Append(ReportsStyling.TrHeadHtmlStyle);
						}

						for (int columnIndex = 0; columnIndex < headers.Length; columnIndex++) {
							bodyText.Append(ReportsStyling.ThHtmlStyle).Append(headers[columnIndex]).Append("</th>");
						}

						bodyText.Append("</tr></thead><tbody>");
					}
					else if (lineCounter % 2 == 0) {
						bodyText.Append(ReportsStyling.TrBodyEven);
					}
					else {
						bodyText.Append(ReportsStyling.TrBodyOdd);
					}
					for (int columnIndex = 0; columnIndex < fields.Length; columnIndex++) {
						if (headers[columnIndex].Contains("Value") ||
							headers[columnIndex].Contains("Amount Due") ||
							headers[columnIndex].Contains("InterestRate") ||
							headers[columnIndex].Contains("Interest") ||
							headers[columnIndex].Contains("LoanRepayment") ||
							headers[columnIndex].Contains("Payment #") ||
							headers[columnIndex].Contains("Max Approved") ||
							headers[columnIndex].Contains("Total Loans") ||
							headers[columnIndex].Contains("Annual Turnover") ||
							headers[columnIndex].Contains("Credit Score") ||
							headers[columnIndex].Contains("Repaid") ||
							headers[columnIndex].Contains("Principal") ||
							headers[columnIndex].Contains("Loan") ||
							headers[columnIndex].Contains("Approved") ||
							headers[columnIndex].Contains("Open Credit") ||
							headers[columnIndex].Contains("Offer To Client") ||
							headers[columnIndex].Contains("Fees")) {
							bodyText.Append(ReportsStyling.TdAlignRightHtmlStyle).Append(row[fields[columnIndex]]).Append("</td>");
						}
						else {
							bodyText.Append(ReportsStyling.TdHtmlStyle).Append(row[fields[columnIndex]]).Append("</td>");
						}
					}
					bodyText.Append("</tr>");
					lineCounter++;
				}
				bodyText.Append("</tbody></table>");
			}
			catch (Exception e) {
				Error(e.ToString());
			}
		} // TableReport
	} // class BaseReportHandler
} // namespace Reports
