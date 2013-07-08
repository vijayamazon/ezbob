using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using Ezbob.Database;
using Ezbob.Logger;
using Aspose.Cells;
using Html;
using Html.Attributes;
using Html.Tags;

namespace Reports {
	using Mailer;

	public class BaseReportHandler : SafeLog {
		#region public

		#region method InitAspose

		public static void InitAspose() {
			var license = new License();

			using (var s = Assembly.GetExecutingAssembly().GetManifestResourceStream("Reports.Aspose.Total.lic")) {
				s.Position = 0;
				license.SetLicense(s);
			} // using
		} // InitAspose

		#endregion method InitAspose

		#region constructor

		public BaseReportHandler(AConnection oDB, ASafeLog log = null) : base(log) {
			DB = oDB;
		} // constructor

		#endregion constructor

		#region method AddReportToList

		public void AddReportToList(List<Report> reportList, DataRow row, string defaultToEmail) {
			reportList.Add(new Report(row, defaultToEmail));
		} // AddReportToList

		#endregion method AddReportToList

		#region report generators

		public ATag TableReport(string spName, DateTime startDate, DateTime endDate, ColumnInfo[] columns, bool isSharones = false, string RptTitle = "") {
			var tbl = new Table().Add<Class>("Report");

			try {
				DataTable dt = DB.ExecuteReader(spName,
					new QueryParameter("@DateStart", DB.DateToString(startDate)),
					new QueryParameter("@DateEnd", DB.DateToString(endDate))
				);

				if (!isSharones)
					tbl.Add<ID>("tableReportData");

				var tr = new Tr().Add<Class>("HR");

				for (int columnIndex = 0; columnIndex < columns.Length; columnIndex++)
					if (columns[columnIndex].IsVisible)
						tr.Append(new Th().Add<Class>("H").Append(new Text(columns[columnIndex].Caption)));

				tbl.Append(new Thead().Append(tr));

				var oTbody = new Tbody();
				tbl.Append(oTbody);

				int lineCounter = 0;

				foreach (DataRow row in dt.Rows) {
					var oTr = new Tr().Add<Class>(lineCounter % 2 == 0 ? "Even" : "Odd");
					oTbody.Append(oTr);

					List<string> oClassesToApply = new List<string>();

					for (int columnIndex = 0; columnIndex < columns.Length; columnIndex++) {
						ColumnInfo col = columns[columnIndex];
						var oValue = row[col.FieldName];

						if (col.IsVisible) {
							var oTd = new Td();
							oTr.Append(oTd);

							if (IsNumber(oValue))
								oTd.Add<Class>("R").Append(new Text(NumStr(oValue, col.Format(IsInt(oValue) ? 0 : 2))));
							else
								oTd.Add<Class>("L").Append(new Text(oValue.ToString()));
						}
						else {
							if (col.ValueType == ValueType.CssClass)
								oClassesToApply.Add(oValue.ToString());
						} // if
					} // for each column

					if (oClassesToApply.Count > 0)
						oTr.ApplyToChildren<Class>(string.Join(" ", oClassesToApply.ToArray()));

					lineCounter++;
				} // for each data row

				var wb = AddSheetToExcel(dt, spName, RptTitle);
			}
			catch (Exception e) {
				Error(e.ToString());
			}
			return tbl;
		} // TableReport

		public ATag BuildNewClientReport(Report report, DateTime today) {
			return new Body().Add<Class>("Body")
				.Append(new H1().Append(new Text(report.GetTitle(today))))
				.Append(AdsReport(today))
				.Append(CustomerReport(today));
		} // BuildNewClientReport

		public ATag BuildDailyStatsReportBody(Report report, DateTime today, DateTime tomorrow) {
			return new Body().Add<Class>("Body")
				.Append( new H1().Append(new Text(report.GetTitle(today))) )
				.Append( DailyStatsReport(today, tomorrow) );
		} // BuildDailyStatsReportBody

		public ATag BuildInWizardReport(Report report, DateTime today, DateTime tomorrow) {
			return new Html.Tags.Body().Add<Class>("Body")
				.Append(new H1().Append(new Text(report.GetTitle(today))))
				.Append(new P().Append(new Text("Clients that enetered Shops but did not complete:")))

				.Append(new P().Append(TableReport("RptNewClients", today, tomorrow, GetHeaderAndFields(ReportType.RPT_IN_WIZARD), true)))

				.Append(new P().Append(new Text("Clients that just enetered their email:")))

				.Append(new P().Append(TableReport("RptNewClientsStep1", today, tomorrow, GetHeaderAndFields(ReportType.RPT_IN_WIZARD), true)));
		} // BuildInWizardReport

		public ATag BuildPlainedPaymentReport(Report report, DateTime today) {
			return new Body().Add<Class>("Body")
				.Append(new H1().Append(new Text(report.GetTitle(today))))
				.Append(PaymentReport(today));
		} // BuildPlainedPaymentReport

		#endregion report generators

		#region Excel generators

		public Workbook BuildNewClientXls(Report report, DateTime today) {
			var title = report.GetTitle(today);
			var wb = new Workbook();

			try {
				DataTable dt = DB.ExecuteReader("RptAdsReport", new QueryParameter("@time", DB.DateToString(today)));
				wb = AddSheetToExcel(dt, title, "RptAdsReport");
			}
			catch (Exception e) {
				Error(e.ToString());
			} // try

			try {
				DataTable dt = DB.ExecuteReader("RptCustomerReport", new QueryParameter("@DateStart", DB.DateToString(today)));
				wb = AddSheetToExcel(dt, title, "RptCustomerReport", String.Empty, wb);
			}
			catch (Exception e) {
				Error(e.ToString());
			} // try

			return wb;
		} // BuildNewClientXls

		public Workbook BuildPlainedPaymentXls(Report report, DateTime today) {
			var title = report.GetTitle(today);
			var wb = new Workbook();

			try {
				DataTable dt = DB.ExecuteReader("RptPaymentReport",
					new QueryParameter("@DateStart", DB.DateToString(today)),
					new QueryParameter("@DateEnd", DB.DateToString(DateTime.Today.AddDays(3)))
				);

				wb = AddSheetToExcel(dt, title, "RptPaymentReport");
			}
			catch (Exception e) {
				Error(e.ToString());
			} // try

			return wb;
		} // BuildPlainedPaymentXls

		public Workbook BuildDailyStatsXls(Report report, DateTime today, DateTime tomorrow) {
			var title = report.GetTitle(today);
			var wb = new Workbook();

			try {
				DataTable dt = DB.ExecuteReader("RptDailyStats",
					new QueryParameter("@DateStart", DB.DateToString(today)),
					new QueryParameter("@DateEnd", DB.DateToString(tomorrow))
				);

				wb = AddSheetToExcel(dt, title, "RptDailyStats");
			}
			catch (Exception e) {
				Error(e.ToString());
			} // try

			return wb;
		} // BuildDailyStatsXls

		public Workbook BuildInWizardXls(Report report, DateTime today, DateTime tomorrow) {
			var title = report.GetTitle(today);
			var sometext = String.Empty;
			var wb = new Workbook();

			try {
				DataTable dt = DB.ExecuteReader("RptNewClients",
					new QueryParameter("@DateStart", DB.DateToString(today)),
					new QueryParameter("@DateEnd", DB.DateToString(tomorrow))
				);

				sometext = "Clients that entered Shops but did not complete:";
				wb = AddSheetToExcel(dt, title, "RptNewClients", sometext);
			}
			catch (Exception e) {
				Error(e.ToString());
			} // try

			try {
				DataTable dt = DB.ExecuteReader("RptNewClientsStep1",
					new QueryParameter("@DateStart", DB.DateToString(today)),
					new QueryParameter("@DateEnd", DB.DateToString(tomorrow))
				);

				sometext = "Clients that just entered their email:";
				wb = AddSheetToExcel(dt, title, "RptNewClientsStep1", sometext, wb);
			}
			catch (Exception e) {
				Error(e.ToString());
			} // try

			return wb;
		} // BuildInWizardXls

		public Workbook XlsReport(string spName, DateTime startDate, DateTime endDate, string rptTitle = "") {
			var wb = new Workbook();

			try {
				DataTable dt = DB.ExecuteReader(spName,
					new QueryParameter("@DateStart", DB.DateToString(startDate)),
					new QueryParameter("@DateEnd", DB.DateToString(endDate))
				);

				wb = AddSheetToExcel(dt, rptTitle, spName, String.Empty);
			}
			catch (Exception e) {
				Error(e.ToString());
			} // try

			return wb;
		} // XlsReport

		#endregion Excel generators

		#endregion public

		#region protected

		protected AConnection DB { get; private set; }

		#endregion protected

		#region private

		#region Is... methods

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

		#endregion Is... methods

		#region method GetHeaderAndFields

		private ColumnInfo[] GetHeaderAndFields(ReportType type) {
			DataTable dt = DB.ExecuteReader("RptScheduler_GetHeaderAndFields", new QueryParameter("@Type", type.ToString()));

			string sHeader = null;
			string sFields = null;

			foreach (DataRow row in dt.Rows) {
				sHeader = row[0].ToString();
				sFields = row[1].ToString();
			} // for each

			return Report.ParseHeaderAndFields(sHeader, sFields);
		} // GetHeadersAndFields

		#endregion method GetHeaderAndFields
		
		#region report generators

		private ATag DailyStatsReport(DateTime today, DateTime tomorrow) {
			Div oRpt = new Div();

			Table tbl = null;
			Tbody tbody = null;

			try {
				DataTable dt = DB.ExecuteReader("RptDailyStats",
					new QueryParameter("@DateStart", DB.DateToString(today)),
					new QueryParameter("@DateEnd", DB.DateToString(tomorrow))
				);

				bool firstLoansLine = true;
				bool firstApplicationLine = true;
				bool firstPaymentLine = true;
				bool firstRegisterLine = true;

				foreach (DataRow row in dt.Rows) {
					string firstVal = row[0].ToString();

					if (IsApplicationHeader(firstVal, firstApplicationLine)) {
						tbl = new Table();

						ATag oTr = new Tr().Add<Class>("HR")
							.Append(new Th().Append(new Text("Application")))
							.Append(new Th().Append(new Text("#")))
							.Append(new Th().Append(new Text("Decision")))
							.Append(new Th().Append(new Text("Value")));

						oTr.ApplyToChildren<Class>("H");

						tbl.Add<ID>("tableReportData")
						.Append(new Thead().Append(oTr)); 

						tbody = new Tbody();
						tbl.Append(tbody);

						oRpt.Append(new P().Append(tbl));

						firstApplicationLine = false;
					} // if first application line

					if (IsApplicationLine(firstVal)) {
						var oTr = new Tr();
						tbody.Append(oTr);

						for (var i = 1; i <= 4; i++)
							oTr.Append(new Td().Add<Class>("L").Append(new Text(row[i].ToString())));
					} // if is application line

					if (IsLoanHeader(firstVal, firstLoansLine)) {
						tbl = new Table();

						ATag oTr = new Tr().Add<Class>("HR")
							.Append(new Th().Append(new Text("Loans")))
							.Append(new Th().Append(new Text("#")))
							.Append(new Th().Append(new Text("Value")));

						oTr.ApplyToChildren<Class>("H");

						tbl.Add<ID>("tableReportData")
						.Append(new Thead().Append(oTr));

						tbody = new Tbody();
						tbl.Append(tbody);

						oRpt.Append(new P().Append(tbl));

						firstLoansLine = false;
					} // if is loan header

					if (IsLoanLine(firstVal)) {
						var oTr = new Tr();
						tbody.Append(oTr);

						foreach (var i in new[] { 1, 2, 4 })
							oTr.Append(new Td().Add<Class>("L").Append(new Text(row[i].ToString())));
					} // if is loan line

					if (IsPaymentHeader(firstVal, firstPaymentLine)) {
						tbl = new Table();

						ATag oTr = new Tr().Add<Class>("HR")
							.Append(new Th().Append(new Text("Payments")))
							.Append(new Th().Append(new Text("Value")));

						oTr.ApplyToChildren<Class>("H");

						tbl.Add<ID>("tableReportData")
						.Append(new Thead().Append(oTr));

						tbody = new Tbody();
						tbl.Append(tbody);

						oRpt.Append(new P().Append(tbl));

						firstLoansLine = false;
					} // if is first payment line

					if (IsPaymentLine(firstVal)) {
						var oTr = new Tr();
						tbody.Append(oTr);

						foreach (var i in new[] { 2, 4 })
							oTr.Append(new Td().Add<Class>("L").Append(new Text(row[i].ToString())));
					} // if payment line

					if (IsRegisterHeader(firstVal, firstRegisterLine)) {
						tbl = new Table();

						tbl.Add<ID>("tableReportData")
						.Append(new Thead().Append(new Tr().Add<Class>("HR")
							.Append(new Th().Add<Class>("H").Append(new Text("Registers")))
						));

						tbody = new Tbody();
						tbl.Append(tbody);

						oRpt.Append(new P().Append(tbl));

						firstLoansLine = false;
					} // if is register header

					if (IsRegisterLine(firstVal))
						tbody.Append( new Tr().Append(new Td().Add<Class>("L").Append(new Text(row[2].ToString()))) );
				} // for each data row
			}
			catch (Exception e) {
				Error(e.ToString());
			}

			return oRpt;
		} // DailyStatsReport

		private ATag CustomerReport(DateTime today) {
			Table tbl = new Table();

			try {
				DataTable dt = DB.ExecuteReader("RptCustomerReport", new QueryParameter("@DateStart", DB.DateToString(today)));

				ATag oTr = new Tr().Add<Class>("HR")
					.Append(new Th().Append(new Text("Email")))
					.Append(new Th().Append(new Text("Status")))
					.Append(new Th().Append(new Text("Wizard Finished")))
					.Append(new Th().Append(new Text("Account #")))
					.Append(new Th().Append(new Text("Credit Offer")))
					.Append(new Th().Append(new Text("Source Ad")));

				oTr.ApplyToChildren<Class>("H");

				tbl.Add<Class>("Report").Append( new Thead().Append(oTr) );

				Tbody tbody = new Tbody();
				tbl.Append(tbody);

				foreach (DataRow row in dt.Rows) {
					oTr = new Tr()
						.Append(new Td().Add<Class>("L").Append(new Text(row["Name"].ToString())))
						.Append(new Td().Add<Class>("L").Append(new Text(row["Status"].ToString())))
						.Append(new Td().Add<Class>("L").Append(new Text(row["IsSuccessfullyRegistered"].ToString())))
						.Append(new Td().Add<Class>("L").Append(new Text(row["AccountNumber"].ToString())))
						.Append(new Td().Add<Class>("R").Append(new Text(row["CreditSum"].ToString())))
						.Append(new Td().Add<Class>("L").Append(new Text(row["ReferenceSource"].ToString())));

					tbody.Append(oTr);
				} // for each data row
			}
			catch (Exception e) {
				Error(e.ToString());
			}

			return tbl;
		} // CustomerReport

		private ATag AdsReport(DateTime today) {
			Table tbl = new Table();

			try {
				DataTable dt = DB.ExecuteReader("RptAdsReport", new QueryParameter("@time", DB.DateToString(today)));

				ATag oTr = new Tr().Add<Class>("HR")
					.Append(new Th().Append(new Text("Ad Name")))
					.Append(new Th().Append(new Text("#")))
					.Append(new Th().Append(new Text("Total Credit Approved")));

				oTr.ApplyToChildren<Class>("H");

				tbl.Add<Class>("Report").Append( new Thead().Append(oTr) );

				Tbody tbody = new Tbody();
				tbl.Append(tbody);

				foreach (DataRow row in dt.Rows) {
					oTr = new Tr()
						.Append(new Td().Add<Class>("L").Append(new Text(row["ReferenceSource"].ToString())))
						.Append(new Td().Add<Class>("L").Append(new Text(row["TotalUsers"].ToString())))
						.Append(new Td().Add<Class>("R").Append(new Text(row["TotalCredit"].ToString())));

					tbody.Append(oTr);
				} // foreach data row
			}
			catch (Exception e) {
				Error(e.ToString());
			}

			return tbl;
		} // AdsReport

		private ATag PaymentReport(DateTime today) {
			Table tbl = new Table();

			try {
				DataTable dt = DB.ExecuteReader("RptPaymentReport",
					new QueryParameter("@DateStart", DB.DateToString(today)),
					new QueryParameter("@DateEnd", DB.DateToString(DateTime.Today.AddDays(3)))
				);

				ATag oTr = new Tr().Add<Class>("HR")
					.Append(new Th().Append(new Text("Id")))
					.Append(new Th().Append(new Text("Name")))
					.Append(new Th().Append(new Text("Email")))
					.Append(new Th().Append(new Text("Date")))
					.Append(new Th().Append(new Text("Amount")));

				oTr.ApplyToChildren<Class>("H");

				tbl.Add<ID>("tableReportData").Add<Class>("Report").Append( new Thead().Append(oTr) );

				var tbody = new Tbody();
				tbl.Append(tbody);

				foreach (DataRow row in dt.Rows) {
					oTr = new Tr()
						.Append(new Td().Add<Class>("L").Append(new Text(row["Id"].ToString())))
						.Append(new Td().Add<Class>("L").Append(new Text(row["Firstname"] + " " + row["SurName"])))
						.Append(new Td().Add<Class>("L").Append(new Text(row["Name"].ToString())))
						.Append(new Td().Add<Class>("L").Append(new Text(row["DATE"].ToString())))
						.Append(new Td().Add<Class>("R").Append(new Text(row["AmountDue"].ToString())));

					tbody.Append(oTr);
				} // foreach data row
			}
			catch (Exception e) {
				Error(e.ToString());
			}

			return tbl;
		} // PaymentReport

		#endregion report generators

		#region private static

		private static bool IsNumber(object value) {
			return IsInt(value) || IsFloat(value);
		} // IsNumber

		private static bool IsInt(object value) {
			return value is sbyte
				|| value is byte
				|| value is short
				|| value is ushort
				|| value is int
				|| value is uint
				|| value is long
				|| value is ulong;
		} // IsInt

		private static bool IsFloat(object value) {
			return value is float
				|| value is double
				|| value is decimal;
		} // IsFloat

		private static string NumStr(object oNumber, string sFormat) {
			if (oNumber is sbyte  ) return ((sbyte  )oNumber).ToString(sFormat, FormatInfo);
			if (oNumber is byte   ) return ((byte   )oNumber).ToString(sFormat, FormatInfo);
			if (oNumber is short  ) return ((short  )oNumber).ToString(sFormat, FormatInfo);
			if (oNumber is ushort ) return ((ushort )oNumber).ToString(sFormat, FormatInfo);
			if (oNumber is int    ) return ((int    )oNumber).ToString(sFormat, FormatInfo);
			if (oNumber is uint   ) return ((uint   )oNumber).ToString(sFormat, FormatInfo);
			if (oNumber is long   ) return ((long   )oNumber).ToString(sFormat, FormatInfo);
			if (oNumber is ulong  ) return ((ulong  )oNumber).ToString(sFormat, FormatInfo);
			if (oNumber is float  ) return ((float  )oNumber).ToString(sFormat, FormatInfo);
			if (oNumber is double ) return ((double )oNumber).ToString(sFormat, FormatInfo);
			if (oNumber is decimal) return ((decimal)oNumber).ToString(sFormat, FormatInfo);

			throw new Exception(string.Format("Unsupported type: {0}", oNumber.GetType()));
		} // NumStr

		private static Workbook AddSheetToExcel(DataTable dt, String title, String sheetName = "", String someText = "", Workbook wb = null) {
			InitAspose();

			const int fc = 1; // first column
			const int fr = 4; // first row
			const int frn = 1; // first row for title

			if (wb == null) { // first initialization, if we will use it multimple times.
				wb = new Workbook();
				wb.Worksheets.Clear();
			} // if

			if (String.IsNullOrEmpty(sheetName))
				sheetName = "Report"; // default sheetName

			var sheet = wb.Worksheets.Add(sheetName); // add new specific sheet to document.

			sheet.Cells.Merge(frn, fc, 1, dt.Columns.Count);
			sheet.Cells[frn, fc].PutValue(title.Replace("<h1>", "").Replace("</h1>", ""));
			sheet.Cells.Merge(frn + 1, fc, 1, dt.Columns.Count);
			sheet.Cells[frn + 1, fc].PutValue(someText);
			sheet.Cells.ImportDataTable(dt, true, fr, fc);
			sheet.AutoFitColumns();

			sheet.Cells.SetColumnWidth(0, 1);

			var titleStyle = sheet.Cells[fc, fr].GetStyle();
			var headerStyle = sheet.Cells[fc, fr].GetStyle();
			var lightStyle = sheet.Cells[fc, fr].GetStyle();
			var darkStyle = sheet.Cells[fc, fr].GetStyle();
			var footerStyle = sheet.Cells[fc, fr].GetStyle();

			titleStyle.VerticalAlignment = TextAlignmentType.Center;
			titleStyle.HorizontalAlignment = TextAlignmentType.Center;
			titleStyle.Pattern = BackgroundType.Solid;
			titleStyle.Font.IsBold = true;
			titleStyle.ForegroundColor = ColorTranslator.FromHtml("#EEEEEE");
			titleStyle.BackgroundColor = ColorTranslator.FromHtml("#EEEEEE");
			titleStyle.Font.Color = ColorTranslator.FromHtml("#666666");
			sheet.Cells[frn, fc].SetStyle(titleStyle);

			headerStyle.VerticalAlignment = TextAlignmentType.Center;
			headerStyle.HorizontalAlignment = TextAlignmentType.Center;
			headerStyle.Borders[BorderType.BottomBorder].Color = Color.Black;
			headerStyle.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Medium;
			headerStyle.Borders[BorderType.LeftBorder].Color = Color.Black;
			headerStyle.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Medium;
			headerStyle.Borders[BorderType.RightBorder].Color = Color.Black;
			headerStyle.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Medium;
			headerStyle.Borders[BorderType.TopBorder].Color = Color.Black;
			headerStyle.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Medium;
			headerStyle.Pattern = BackgroundType.Solid;
			headerStyle.Font.IsBold = true;
			headerStyle.ForegroundColor = ColorTranslator.FromHtml("#9AB1D1");
			headerStyle.BackgroundColor = ColorTranslator.FromHtml("#9AB1D1");
			headerStyle.Font.Color = ColorTranslator.FromHtml("#FFFFFF");

			lightStyle.VerticalAlignment = TextAlignmentType.Center;
			lightStyle.HorizontalAlignment = TextAlignmentType.Left;
			lightStyle.Pattern = BackgroundType.Solid;
			lightStyle.Font.IsBold = false;
			lightStyle.Borders[BorderType.LeftBorder].Color = Color.Black;
			lightStyle.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Medium;
			lightStyle.Borders[BorderType.RightBorder].Color = Color.Black;
			lightStyle.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Medium;
			lightStyle.ForegroundColor = ColorTranslator.FromHtml("#FFFFFF");
			lightStyle.BackgroundColor = ColorTranslator.FromHtml("#FFFFFF");
			lightStyle.Font.Color = ColorTranslator.FromHtml("#000000");

			darkStyle.VerticalAlignment = TextAlignmentType.Center;
			darkStyle.HorizontalAlignment = TextAlignmentType.Left;
			darkStyle.Pattern = BackgroundType.Solid;
			darkStyle.Font.IsBold = false;
			darkStyle.Borders[BorderType.LeftBorder].Color = Color.Black;
			darkStyle.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Medium;
			darkStyle.Borders[BorderType.RightBorder].Color = Color.Black;
			darkStyle.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Medium;
			darkStyle.ForegroundColor = ColorTranslator.FromHtml("#F9F9F9");
			darkStyle.BackgroundColor = ColorTranslator.FromHtml("#F9F9F9");
			darkStyle.Font.Color = ColorTranslator.FromHtml("#000000");

			footerStyle.Borders[BorderType.TopBorder].Color = Color.Black;
			footerStyle.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Medium;

			for (var it = fc; it < dt.Columns.Count + fc; it++)
				sheet.Cells[fr, it].SetStyle(headerStyle);

			for (var row = fr + 1; row <= dt.Rows.Count + fr; row++) {
				for (var column = fc; column < dt.Columns.Count + fc; column++)
					sheet.Cells[row, column].SetStyle(lightStyle);

				if (++row > (dt.Rows.Count + fr))
					continue;

				for (var column = fc; column < dt.Columns.Count + fc; column++)
					sheet.Cells[row, column].SetStyle(darkStyle);
			} // for

			for (var it = fc; it < dt.Columns.Count + fc; it++)
				sheet.Cells[fr + dt.Rows.Count + 1, it].SetStyle(footerStyle);

			return wb;
		} // AddSheetToExcel

		private static readonly CultureInfo FormatInfo = new CultureInfo("en-GB");

		#endregion private static

		#endregion private
	} // class BaseReportHandler
} // namespace Reports
