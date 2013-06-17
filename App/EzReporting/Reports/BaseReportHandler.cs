using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.Remoting.Contexts;
using System.Web;
using Ezbob.Database;
using Ezbob.Logger;
using global::Html;
using PreMailerDotNet;
using Aspose.Cells;


namespace Reports {
	using Mailer;

	public class BaseReportHandler : SafeLog {
		public const string DefaultToEMail = "dailyreports@ezbob.com";
		public const string DefaultFromEMailPassword = "ezbob2012";
		public const string DefaultFromEMail = "ezbob@ezbob.com";
		private static readonly CultureInfo FormatInfo = new CultureInfo("en-GB");
	    public static string DefaultAttachment = "";

		private static string Lock = "";

		public BaseReportHandler(AConnection oDB, ASafeLog log = null) : base(log) {
			DB = oDB;
		} // constructor

		protected AConnection DB { get; private set; }

		public ATag BuildPlainedPaymentReport(Report report, string today, string tomorrow) {
			return new Body().Add<Class>("Body")
				.Append(new H1().Append(new Text(report.Title + " " + today)))
				.Append(PaymentReport(today));
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
				Columns = Report.ParseHeaderAndFields(row["Header"].ToString(), row["Fields"].ToString()),
				ToEmail = (string.IsNullOrEmpty(row["ToEmail"].ToString()) ? defaultToEmail : row["ToEmail"].ToString()),
				IsMonthToDate = (bool)row["IsMonthToDate"]
			});
		} // AddReportToList

		public void SendReport(string subject, ATag mailBody, string toAddressStr = DefaultToEMail, string period = "Daily", Workbook wb = null )
		{

            var email = new Html.Html();

		    email
		        .Append(new Head().Append(Report.GetStyle()))
		        .Append(mailBody);


			string sEmail = PreMailer.MoveCssInline(email.ToString(), true);

            lock (Lock) {
				Mailer.SendMail(
					DefaultFromEMail,
					DefaultFromEMailPassword,
					"EZBOB " + period + " " + subject + " Client Report",
					sEmail,
					toAddressStr,
                    wb
				);
            } // lock

            if (!String.IsNullOrEmpty(DefaultAttachment)) 
            {
                try
                {
                    File.Delete(DefaultAttachment);
                }
                catch (Exception e)
                {
                    Error(e.ToString());
                }
            }

            Debug("Mail {0} sent to: {1}", subject, toAddressStr);
			Debug("Before embedding styles: {0}", email.ToString());
			Debug("After embedding styles: {0}", sEmail);
		} // SendReport

		public ATag BuildNewClientReport(Report report, string today, string tomorrow) {
			return new Body().Add<Class>("Body")
				.Append(new H1().Append(new Text(report.Title + " " + today)))
				.Append(AdsReport(today))
				.Append(CustomerReport(today));
		} // BuildNewClientReport

		private ATag CustomerReport(string today) {
			Table tbl = new Table();

			try {
				DataTable dt = DB.ExecuteReader("RptCustomerReport", new QueryParameter("@DateStart", today));

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

		private ATag AdsReport(string today) {
			Table tbl = new Table();

			try {
				DataTable dt = DB.ExecuteReader("RptAdsReport", new QueryParameter("@time", today));

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

		private ATag PaymentReport(string today) {
			Table tbl = new Table();

			try {
				DataTable dt = DB.ExecuteReader("RptPaymentReport",
					new QueryParameter("@DateStart", today),
					new QueryParameter("@DateEnd", DateTime.Today.AddDays(3).ToString("yyyy-MM-dd"))
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

		public ATag BuildDailyStatsReportBody(Report report, string today, string tomorrow) {
			return new Body().Add<Class>("Body")
				.Append( new H1().Append(new Text(report.Title + " " + today)) )
				.Append( DailyStatsReport(today, tomorrow) );
		} // BuildDailyStatsReportBody

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

		private ATag DailyStatsReport(string today, string tomorrow) {
			Div oRpt = new Div();

			Table tbl = null;
			Tbody tbody = null;

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

		public ATag BuildInWizardReport(Report report, string today, string tomorrow) {
            return new Html.Body().Add<Class>("Body")
				.Append(new H1().Append(new Text(report.Title + " " + today)))
				.Append(new P().Append(new Text("Clients that enetered Shops but did not complete:")))

				.Append(new P().Append(TableReport("RptNewClients", today, tomorrow, GetHeaderAndFields(ReportType.RPT_IN_WIZARD), true)))

				.Append(new P().Append(new Text("Clients that just enetered their email:")))

				.Append(new P().Append(TableReport("RptNewClientsStep1", today, tomorrow, GetHeaderAndFields(ReportType.RPT_IN_WIZARD), true)));
		} // BuildInWizardReport



        public Workbook BuildNewClientXls(Report report, string today, string tomorrow)
        {
            var title = report.Title + " " + today;
            var wb = new Workbook();
            try
            {
                DataTable dt = DB.ExecuteReader("RptAdsReport", new QueryParameter("@time", today));
                wb = AddSheetToExcel(dt, title, "RptAdsReport");
            }
            catch (Exception e)
            {
                Error(e.ToString());
            }
            try
            {
                DataTable dt = DB.ExecuteReader("RptCustomerReport", new QueryParameter("@DateStart", today));
                wb = AddSheetToExcel(dt, title, "RptCustomerReport", String.Empty, wb);
            }
            catch (Exception e)
            {
                Error(e.ToString());
            }
            return wb;
        } // BuildNewClientXls

        public Workbook BuildPlainedPaymentXls(Report report, string today, string tomorrow)
        {
            var title = report.Title + " " + today;
            var wb = new Workbook();
            try
            {
                DataTable dt = DB.ExecuteReader("RptPaymentReport",
                    new QueryParameter("@DateStart", today),
                    new QueryParameter("@DateEnd", DateTime.Today.AddDays(3).ToString("yyyy-MM-dd"))
                    );
                wb = AddSheetToExcel(dt, title, "RptPaymentReport");
            }
            catch (Exception e)
            {
                Error(e.ToString());
            }
            return wb;
        } // BuildPlainedPaymentXls


        public Workbook BuildDailyStatsXls(Report report, string today, string tomorrow)
        {
            var title = report.Title + " " + today;
            var wb = new Workbook();
            try
            {
                DataTable dt = DB.ExecuteReader("RptDailyStats",
                    new QueryParameter("@DateStart", today),
                    new QueryParameter("@DateEnd", tomorrow)
                   );
                wb = AddSheetToExcel(dt, title, "RptDailyStats");
            }
            catch (Exception e)
            {
                Error(e.ToString());
            }
            return wb;
        } // BuildDailyStatsXls

        public Workbook BuildInWizardXls(Report report, string today, string tomorrow)
        {
            var title = report.Title + " " + today;
            var sometext = String.Empty;
            var wb = new Workbook();
            try
            {
                DataTable dt = DB.ExecuteReader("RptNewClients",
                    new QueryParameter("@DateStart", today),
                    new QueryParameter("@DateEnd", tomorrow)
                   );
                sometext = "Clients that entered Shops but did not complete:";
                wb = AddSheetToExcel(dt, title, "RptNewClients", sometext);
            }
            catch (Exception e)
            {
                Error(e.ToString());
            }
            try
            {
                DataTable dt = DB.ExecuteReader("RptNewClientsStep1",
                    new QueryParameter("@DateStart", today),
                    new QueryParameter("@DateEnd", tomorrow)
                   );
                sometext = "Clients that just entered their email:";
                wb = AddSheetToExcel(dt, title, "RptNewClientsStep1", sometext, wb);
            }
            catch (Exception e)
            {
                Error(e.ToString());
            }
            return wb;
        } // BuildInWizardXls

        public Workbook XlsReport(string spName, string startDate, string endDate, String rptTitle = "")
        {
            var wb = new Workbook();
            try
            {
                DataTable dt = DB.ExecuteReader(spName,
                    new QueryParameter("@DateStart", startDate),
                    new QueryParameter("@DateEnd", endDate)
                   );
                wb = AddSheetToExcel(dt, rptTitle, spName, String.Empty);
            }
            catch (Exception e)
            {
                Error(e.ToString());
            }
            return wb;
        } // XlsReport

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

        public ATag TableReport(string spName, string startDate, string endDate, ColumnInfo[] columns, bool isSharones = false, String RptTitle = "")
        {
			var tbl = new Table().Add<Class>("Report");

			try {
				DataTable dt = DB.ExecuteReader(spName,
					new QueryParameter("@DateStart", startDate),
					new QueryParameter("@DateEnd", endDate)
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

        public static void InitAspose()
        {
            var license = new License();

            using (var s = Assembly.GetExecutingAssembly().GetManifestResourceStream("Reports.Aspose.Total.lic"))
            {
                s.Position = 0;
                license.SetLicense(s);
            }
        }

        private static Workbook AddSheetToExcel(DataTable dt, String title, String sheetName = "", String someText = "", Workbook wb = null)
	    {
            InitAspose();

	        const int fc = 1; // first column
            const int fr = 4; // first row
            const int frn = 1; // first row for title

            if (wb == null) // first initialization, if we will use it multimple times.
            {
                wb = new Workbook();
                wb.Worksheets.Clear();
            } 
            if (String.IsNullOrEmpty(sheetName)) sheetName = "Report"; // default sheetName
            var sheet = wb.Worksheets.Add(sheetName); // add new specific sheet to document.

            sheet.Cells.Merge(frn, fc, 1, dt.Columns.Count);
            sheet.Cells[frn, fc].PutValue(title.Replace("<h1>", "").Replace("</h1>",""));
            sheet.Cells.Merge(frn + 1, fc, 1, dt.Columns.Count);
            sheet.Cells[frn+1, fc].PutValue(someText);
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



            for (var it = fc; it < dt.Columns.Count + fc; it++ )
                sheet.Cells[fr, it].SetStyle(headerStyle);

            for (var row = fr+1; row <= dt.Rows.Count + fr; row++)
            {
                for (var column = fc; column < dt.Columns.Count + fc; column++)
                    sheet.Cells[row, column].SetStyle(lightStyle);
                if (++row > (dt.Rows.Count + fr)) continue;
                for (var column = fc; column < dt.Columns.Count + fc; column++)
                    sheet.Cells[row, column].SetStyle(darkStyle);
            }

            for (var it = fc; it < dt.Columns.Count + fc; it++)
                sheet.Cells[fr + dt.Rows.Count + 1, it].SetStyle(footerStyle);

            //DefaultAttachment = filename + ".xlsx";
            //wb.Save(DefaultAttachment);
	        return wb;
	    }




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

		private string NumStr(object oNumber, string sFormat) {
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
	} // class BaseReportHandler
} // namespace Reports
