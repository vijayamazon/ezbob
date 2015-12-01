namespace Reports {
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Data;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils;
	using Ezbob.Utils.Html;
	using Ezbob.Utils.Html.Attributes;
	using Ezbob.Utils.Html.Tags;
	using OfficeOpenXml;

	public partial class BaseReportHandler : SafeLog {
		public ATag BuildNewUnmatchedPaymentsReport(
			Report report,
			DateTime today,
			DateTime tomorrow,
			List<string> oColumnTypes = null
		) {
			CreateNewUnmatchedPaymentsData(report, today, tomorrow);

			SaveNewUnmatchedPayments();

			return new Body().Add<Class>("Body")
				.Append(new H1().Append(new Text(report.GetTitle(today, oToDate: tomorrow))))
				.Append(new P().Append(TableReport(
					this.newUnmatchedPaymentsData.Value.Key,
					this.newUnmatchedPaymentsData.Value.Value,
					oColumnTypes: oColumnTypes
				)));
		} // BuildStrategyRunningTimeReport

		public ExcelPackage BuildNewUnmatchedPaymentsXls(Report report, DateTime today, DateTime tomorrow) {
			CreateNewUnmatchedPaymentsData(report, today, tomorrow);

			return AddSheetToExcel(this.newUnmatchedPaymentsData.Value.Value, report.GetTitle(today, oToDate: tomorrow));
		} // BuildStrategyRunningTimeXls

		private void CreateNewUnmatchedPaymentsData(Report report, DateTime today, DateTime tomorrow) {
			if (this.newUnmatchedPaymentsData != null)
				return;

			var rpt = new ReportQuery(report) {
				DateStart = today,
				DateEnd = tomorrow,
			};

			DataTable tbl = NewUnmatchedPayment.ToTable();

			DB.ForEachResult<NewUnmatchedPayment>(
				row => {
					row.ToRow(tbl);

					if (this.timestampsToSave == null)
						this.timestampsToSave = new List<PaymentTimestamp>();

					this.timestampsToSave.Add(new PaymentTimestamp {
						PaymentID = row.PaymentID,
						LastKnownTimestamp = row.PaymentTimestamp,
					});
				},
				"FindNewUnmatchedPayments",
				CommandSpecies.StoredProcedure
			);

			this.newUnmatchedPaymentsData = new KeyValuePair<ReportQuery, DataTable>(rpt, tbl);
		} // CreateNewUnmatchedPaymentsData

		private void SaveNewUnmatchedPayments() {
			if ((this.timestampsToSave == null) || (this.timestampsToSave.Count < 1))
				return;

			DB.ExecuteNonQuery(
				"SaveNewUnmatchedPaymentTimestamps",
				CommandSpecies.StoredProcedure,
				DB.CreateTableParameter("TimestampList", (IEnumerable<PaymentTimestamp>)this.timestampsToSave)
			);
		} // SaveNewUnmatchedPayments

		private KeyValuePair<ReportQuery, DataTable>? newUnmatchedPaymentsData;

		private List<PaymentTimestamp> timestampsToSave;

		private class PaymentTimestamp {
			public int PaymentID { get; set; }
			public byte[] LastKnownTimestamp { get; set; }
		} // class PaymentTimestamp

		private class NewUnmatchedPayment : AResultRow {
			public int CustomerID { get; set; }
			public string CustomerEmail { get; set; }
			public string CustomerName { get; set; }
			public int PaymentID { get; set; }
			public byte[] PaymentTimestamp { get; set; }
			public byte[] LastKnownTimestamp { get; set; }
			public DateTime PaymentTime { get; set; }
			public decimal Delta { get; set; }
			public decimal PaidAmount { get; set; }
			public decimal PaidPrincipal { get; set; }
			public decimal PaidInterest { get; set; }
			public decimal PaidFees { get; set; }
			public decimal PaidRollover { get; set; }
			public int LoanID { get; set; }
			public string LoanRefNum { get; set; }
			public DateTime LoanIssueTime { get; set; }
			public decimal LoanAmount { get; set; }
			public decimal LoanInterestRate { get; set; }
			public int LoanTerm { get; set; }

			public static DataTable ToTable() {
				var tbl = new DataTable();

				PropertyTraverser.Traverse(typeof(NewUnmatchedPayment), (ignoredInstance, pi) => {
					if (pi.PropertyType == typeof(byte[]))
						return;

					tbl.Columns.Add(pi.Name, pi.PropertyType);
				});

				return tbl;
			} // ToTable

			public void ToRow(DataTable tbl) {
				var lst = new List<object>();

				this.Traverse((instance, pi) => {
					if (pi.PropertyType == typeof(byte[]))
						return;

					lst.Add(pi.GetValue(instance));
				});

				tbl.Rows.Add(lst.ToArray());
			} // ToTable
		} // class NewUnmatchedPayment
	} // class BaseReportHandler
} // namespace
