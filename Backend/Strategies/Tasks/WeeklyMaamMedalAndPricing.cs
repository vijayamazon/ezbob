namespace Ezbob.Backend.Strategies.Tasks {
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Globalization;
	using System.IO;
	using DbConstants;
	using Ezbob.Backend.Strategies.AutomationVerification.KPMG;
	using Ezbob.ExcelExt;
	using Ezbob.Utils.MimeTypes;
	using global::Reports;
	using MailApi;
	using MailApi.Model;
	using OfficeOpenXml;
	using OfficeOpenXml.Style;

	public class WeeklyMaamMedalAndPricing : MaamMedalAndPricing {
		public WeeklyMaamMedalAndPricing(bool forceRunNow) : base(-1, -1) {
			DateTime dateTo = DateTime.UtcNow.Date;

			this.doRun = forceRunNow || dateTo.DayOfWeek == DayOfWeek.Saturday;

			this.today = dateTo.Date;

			DateTime dateFrom = dateTo.AddDays(-7);

			this.condition = string.Format(
				"AND '{0}' <= r.UnderwriterDecisionDate AND r.UnderwriterDecisionDate < '{1}'",
				dateFrom.Date.ToString("MMMM d yyyy", CultureInfo.InvariantCulture),
				this.today.ToString("MMMM d yyyy", CultureInfo.InvariantCulture)
			);
		} // constructor

		public override string Name {
			get { return "DatedMaamMedalAndPricing"; }
		} // Name

		public override void Execute() {
			if (!this.doRun) {
				Log.Debug("Not running: neither Saturday nor forced.");
				return;
			} // if

			Report rpt;

			try {
				rpt = new Report(DB, ReportType.RPT_WEEKLY_KPMG);
			} catch (Exception e) {
				Log.Warn(e, "Failed to load report by type '{0}'.", ReportType.RPT_WEEKLY_KPMG);
				return;
			} // try

			if (string.IsNullOrWhiteSpace(rpt.ToEmail)) {
				Log.Debug("Not running: no email recipients found.");
				return;
			} // if

			base.Execute();

			string emailText = string.Join(System.Environment.NewLine, CsvOutput);

			string baseAttachmentName = "weekly.kpmg." + this.today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

			var mime = new MimeTypeResolver();

			var attachments = new List<attachment> {
				new attachment {
					name = baseAttachmentName + ".txt",
					content = Mail.EncodeAttachment(emailText),
					type = "text/plain",
				},
				new attachment {
					name = baseAttachmentName + ".xlsx",
					content = CreateXlsx(baseAttachmentName + ".xlsx"),
					type = mime[".xlsx"],
				}
			};

			new Mail().Send(
				rpt.ToEmail,
				"See attached file.",
				null,
				ConfigManager.CurrentValues.Instance.MailSenderEmail,
				ConfigManager.CurrentValues.Instance.MailSenderName,
				rpt.Title.Trim() + " " + this.today,
				attachments: attachments
			);
		} // Execute

		protected override string Condition {
			get { return this.condition; }
		} // Condition

		private string CreateXlsx(string fileName) {
			var ep = new ExcelPackage();

			ExcelWorksheet sheet = ep.CreateSheet("Cash requests", false, CsvTitles);

			int curRow = 2;

			var stats = new List<Stats> {
				new Stats(true, true),
				new Stats(true, false),
				new Stats(false, true),
				new Stats(false, false),
			};

			foreach (Datum d in Data) {
				d.ToXlsx(sheet, curRow, CashRequestLoans, LoanSources);
				curRow++;

				foreach (var st in stats)
					st.Add(d);
			} // for each

			ep.AutoFitColumns();

			sheet = ep.CreateSheet("Statistics", false);
			int row = 1;

			foreach (var st in stats) {
				row = st.ToXlsx(sheet, row);
				row++;
			} // for each

			var ms = new MemoryStream();

			ep.SaveAs(ms);

			byte[] fileContent = ms.GetBuffer();

			string filePath = Path.Combine(Path.GetTempPath(), fileName);

			try {
				File.WriteAllBytes(filePath, fileContent);
				Log.Debug("Saved .xlsx file as {0}.", filePath);
			} catch (Exception e) {
				Log.Warn(e, "Failed to save .xlsx file as {0}.", filePath);
			} // try

			return Mail.EncodeAttachment(fileContent);
		} // CreateXlsx

		private readonly string condition;

		private readonly DateTime today;

		private readonly bool doRun;

		private class Stats {
			public Stats(bool takeMin, bool takeAll) {
				this.takeMin = takeMin;
				this.takeAll = takeAll;

				this.name =
					(this.takeMin ? "Minimum" : "Maximum") + " offer " +
					(this.takeAll ? "with" : "without") + " campaigns and duplicates";

				this.totalCount = 0;
				this.autoProcessedCount = 0;

				this.autoRejectedCount = 0;
				this.autoRerejectedCount = 0;

				this.autoApprovedCount = 0;
				this.autoReapprovedCount = 0;

				this.notAutoRejectedAndAutoApprovedCount = 0;
				this.notAutoRejectedCount = 0;

				this.manualRejectedCount = 0;
				this.manualAndAutoRejectedCount = 0;

				this.manualApprovedCount = 0;
				this.manualAndAutoApprovedCount = 0;

				this.manualAmount = 0;
				this.autoAmount = 0;

				this.manualDefaultCount = 0;
				this.autoDefaultCount = 0;

				this.manualRejectedAndAutoApprovedCount = 0;
				this.manualApprovedAndAutoRejectedCount = 0;
			} // constructor

			public void Add(Datum d) {
				if (!this.takeAll && (d.IsSuperseded || d.IsCampaign))
					return;

				AMedalAndPricing autoMax = d.AutoMax ?? d.AutoMin;

				this.totalCount++;

				if (d.AutomationDecision != DecisionActions.Waiting)
					this.autoProcessedCount++;

				if (d.AutomationDecision == DecisionActions.ReReject || d.AutomationDecision == DecisionActions.Reject) {
					this.autoRejectedCount++;

					if (d.AutomationDecision == DecisionActions.ReReject)
						this.autoRerejectedCount++;
				} // if

				if (this.takeMin) {
					if (d.AutomationDecision == DecisionActions.ReApprove || d.AutomationDecision == DecisionActions.Approve) {
						this.autoApprovedCount++;

						if (d.AutomationDecision == DecisionActions.ReApprove)
							this.autoReapprovedCount++;
					} // if
				} else {
					if (d.AutomationDecision == DecisionActions.ReApprove) {
						this.autoApprovedCount++;
						this.autoReapprovedCount++;
					} else if (d.AutomationDecision == DecisionActions.Waiting && autoMax.Amount > 0)
						this.autoApprovedCount++;
				} // if

				if (!d.IsAutoReRejected && !d.IsAutoRejected) {
					this.notAutoRejectedCount++;

					if (this.takeMin) {
						if (d.IsAutoReApproved || d.IsAutoApproved)
							this.notAutoRejectedAndAutoApprovedCount++;
					} else if (d.IsAutoReApproved || autoMax.Amount > 0)
						this.notAutoRejectedAndAutoApprovedCount++;
				} // if

				if (d.Manual.Decision == "Rejected") {
					this.manualRejectedCount++;

					if (d.IsAutoRejected || d.IsAutoReRejected)
						this.manualAndAutoRejectedCount++;

					if (this.takeMin) {
						if (d.AutomationDecision == DecisionActions.Approve || d.AutomationDecision == DecisionActions.ReApprove)
							this.manualRejectedAndAutoApprovedCount++;
					} else {
						if (d.AutomationDecision == DecisionActions.ReApprove)
							this.manualRejectedAndAutoApprovedCount++;
						else if (d.AutomationDecision == DecisionActions.Waiting && autoMax.Amount > 0)
							this.manualRejectedAndAutoApprovedCount++;
					} // if
				} // if

				if (d.Manual.Decision == "Approved") {
					this.manualApprovedCount++;

					this.manualAmount += d.Manual.Amount;

					if (d.HasDefaultLoan)
						this.manualDefaultCount++;

					if (this.takeMin) {
						if ((d.AutomationDecision == DecisionActions.ReApprove) || (d.AutomationDecision == DecisionActions.Approve))
							this.manualAndAutoApprovedCount++;
					} else {
						if (d.AutomationDecision == DecisionActions.ReApprove)
							this.manualAndAutoApprovedCount++;
						else if (d.AutomationDecision == DecisionActions.Waiting && autoMax.Amount > 0)
							this.manualAndAutoApprovedCount++;
					} // if

					if (d.AutomationDecision == DecisionActions.ReReject || d.AutomationDecision == DecisionActions.Reject)
						this.manualApprovedAndAutoRejectedCount++;
				} // if

				if (this.takeMin) {
					if (d.AutomationDecision == DecisionActions.ReApprove) {
						this.autoAmount += d.ReapprovedAmount;

						if (d.HasDefaultLoan)
							this.autoDefaultCount++;
					} else if (d.AutomationDecision == DecisionActions.Approve) {
						this.autoAmount += d.AutoMin.Amount;

						if (d.HasDefaultLoan)
							this.autoDefaultCount++;
					} // if
				} else {
					if (d.AutomationDecision == DecisionActions.ReApprove) {
						this.autoAmount += d.ReapprovedAmount;

						if (d.HasDefaultLoan)
							this.autoDefaultCount++;
					} else if (d.AutomationDecision == DecisionActions.Waiting && autoMax.Amount > 0) {
						this.autoAmount = autoMax.Amount;

						if (d.HasDefaultLoan)
							this.autoDefaultCount++;
					} // if
				} // if
			} // Add

			// ReSharper disable once UnusedMethodReturnValue.Local
			public int ToXlsx(ExcelWorksheet sheet, int row) {
				SetBorder(sheet.Cells[row, 1, row, LastColumnNumber]).Merge = true;
				sheet.SetCellValue(row, 1, this.name, bSetZebra: false, oBgColour: Color.Yellow, bIsBold: true);
				sheet.Cells[row, 1].Style.Font.Size = 16;
				row++;

				row = ToXlsxTotal(sheet, row);
				row = ToXlsxAutoProcessed(sheet, row);

				row = ToXlsxAutoRejected(sheet, row);
				row = ToXlsxAutoRerejected(sheet, row);

				row = ToXlsxAutoApproved(sheet, row);
				row = ToXlsxAutoReapproved(sheet, row);

				row = ToXlsxNotAutoRejectedAndAutoApproved(sheet, row);

				row = ToXlsxManualAndAutoRejected(sheet, row);

				row = ToXlsxManualAndAutoApproved(sheet, row);

				row = ToXlsxManualAndAutoAmount(sheet, row);

				row = ToXlsxDefaults(sheet, row);

				row = ToXlsxManualRejectAndAutoApprove(sheet, row);

				row = ToXlsxManualApproveAndAutoReject(sheet, row);

				for (int i = 1; i <= LastColumnNumber; i++)
					sheet.Column(i).AutoFit();

				return row;
			} // ToXlsx

			private int ToXlsxTotal(ExcelWorksheet sheet, int row) {
				SetRowTitle(sheet, row, "Total");
				return SetValue(sheet, row, new TitledValue("count", this.totalCount));
			} // ToXlsxTotal

			private int ToXlsxAutoProcessed(ExcelWorksheet sheet, int row) {
				SetRowTitle(sheet, row, "How many of applications were processed automatically?");

				return SetValue(
					sheet,
					row,
					new TitledValue("count", this.autoProcessedCount),
					new TitledValue("processed / total %", this.autoProcessedCount, this.totalCount)
				);
			} // ToXlsxAutoProcessed

			private int ToXlsxAutoRejected(ExcelWorksheet sheet, int row) {
				SetRowTitle(sheet, row, "How many of applications were rejected automatically?");

				return SetValue(
					sheet,
					row,
					new TitledValue("count", this.autoRejectedCount),
					new TitledValue("rejected / total %", this.autoRejectedCount, this.totalCount)
				);
			} // ToXlsxAutoRejected

			private int ToXlsxAutoRerejected(ExcelWorksheet sheet, int row) {
				SetRowTitle(sheet, row, "Out of auto rejected how many were re-rejected?");

				return SetValue(
					sheet,
					row,
					new TitledValue("count", this.autoRerejectedCount),
					new TitledValue("re-rejected / total %", this.autoRerejectedCount, this.totalCount),
					new TitledValue("re-rejected / rejected %", this.autoRerejectedCount, this.autoRejectedCount)
				);
			} // ToXlsxAutoRerejected

			private int ToXlsxAutoApproved(ExcelWorksheet sheet, int row) {
				SetRowTitle(sheet, row, "How many of applications were approved automatically?");

				return SetValue(
					sheet,
					row,
					new TitledValue("count", this.autoApprovedCount),
					new TitledValue("approved / total %", this.autoApprovedCount, this.totalCount)
				);
			} // ToXlsxAutoApproved

			private int ToXlsxAutoReapproved(ExcelWorksheet sheet, int row) {
				SetRowTitle(sheet, row, "Out of auto approved how many were re-approved?");

				return SetValue(
					sheet,
					row,
					new TitledValue("count", this.autoReapprovedCount),
					new TitledValue("re-approved / total %", this.autoReapprovedCount, this.totalCount),
					new TitledValue("re-approved / approved %", this.autoReapprovedCount, this.autoApprovedCount)
				);
			} // ToXlsxAutoReapproved

			private int ToXlsxNotAutoRejectedAndAutoApproved(ExcelWorksheet sheet, int row) {
				SetRowTitle(sheet, row, "How many of not auto-rejected were auto-approved?");

				return SetValue(
					sheet,
					row,
					new TitledValue("count", this.notAutoRejectedCount),
					new TitledValue("out of them auto approved count", this.notAutoRejectedAndAutoApprovedCount),
					new TitledValue(
						"auto approved / not auto rejected %",
						this.notAutoRejectedAndAutoApprovedCount,
						this.notAutoRejectedCount
					)
				);
			} // ToXlsxNotAutoRejectedAndAutoApproved

			private int ToXlsxManualAndAutoRejected(ExcelWorksheet sheet, int row) {
				SetRowTitle(sheet, row, "How many of manual rejections were also auto-rejected?");

				return SetValue(
					sheet,
					row,
					new TitledValue("manually rejected count", this.manualRejectedCount),
					new TitledValue("out of them auto rejected count", this.manualAndAutoRejectedCount),
					new TitledValue("auto rejected / manually rejected %", this.manualAndAutoRejectedCount, this.manualRejectedCount)
				);
			} // ToXlsxManualAndAutoRejected

			private int ToXlsxManualAndAutoApproved(ExcelWorksheet sheet, int row) {
				SetRowTitle(sheet, row, "How many of manually approved were also auto approved?");

				return SetValue(
					sheet,
					row,
					new TitledValue("manually approved count", this.manualApprovedCount),
					new TitledValue("out of them auto approved count", this.manualAndAutoApprovedCount),
					new TitledValue("auto approved / manually approved %", this.manualAndAutoApprovedCount, this.manualApprovedCount)
				);
			} // ToXlsxManualAndAutoApproved

			private int ToXlsxManualAndAutoAmount(ExcelWorksheet sheet, int row) {
				SetRowTitle(sheet, row, "Automatically approved amount vs. manual approve amount");

				return SetValue(
					sheet,
					row,
					new TitledValue("manually approved amount", this.manualAmount),
					new TitledValue("auto approved amount", this.autoAmount),
					new TitledValue("auto approved amount / manually approved amount %", this.autoAmount, this.manualAmount)
				);
			} // ToXlsxManualAndAutoAmount

			private int ToXlsxDefaults(ExcelWorksheet sheet, int row) {
				SetRowTitle(sheet, row, "What is default (loan wise) automatic vs. manual?");

				return SetValue(
					sheet,
					row,
					new TitledValue("manual default count", this.manualDefaultCount),
					new TitledValue("auto default count", this.autoDefaultCount),
					new TitledValue("manual default / total %", this.manualDefaultCount, this.totalCount),
					new TitledValue("auto default / total %", this.autoDefaultCount, this.totalCount)
				);
			} // ToXlsxDefaults

			private int ToXlsxManualRejectAndAutoApprove(ExcelWorksheet sheet, int row) {
				SetRowTitle(sheet, row, "How many of manually rejected were automatically approved?");

				return SetValue(
					sheet,
					row,
					new TitledValue("manually rejected count", this.manualRejectedCount),
					new TitledValue("out of them auto approved count", this.manualRejectedAndAutoApprovedCount),
					new TitledValue("auto approved / manually rejected %", this.manualRejectedAndAutoApprovedCount, this.manualRejectedCount)
				);
			} // ToXlsxManualRejectAndAutoApprove

			private int ToXlsxManualApproveAndAutoReject(ExcelWorksheet sheet, int row) {
				SetRowTitle(sheet, row, "How many of manually approved were automatically rejected?");

				return SetValue(
					sheet,
					row,
					new TitledValue("manually approved count", this.manualApprovedCount),
					new TitledValue("out of them auto rejected count", this.manualApprovedAndAutoRejectedCount),
					new TitledValue("auto rejected / manually approved %", this.manualApprovedAndAutoRejectedCount, this.manualApprovedCount)
				);
			} // ToXlsxManualApproveAndAutoReject

			private void SetRowTitle(ExcelWorksheet sheet, int row, string title) {
				SetBorder(sheet.Cells[row, 1]);
				sheet.SetCellValue(row, 1, title, true);
			} // SetRowTitle

			private int SetValue(ExcelWorksheet sheet, int row, params TitledValue[] values) {
				int column = 2;

				for (int i = 0; i < values.Length; i++) {
					SetOneValue(sheet, row, column, values[i]);
					column += 2;
				} // for

				for (; column <= LastColumnNumber; column += 2)
					SetOneValue(sheet, row, column, TitledValue.Default);

				return row + 1;
			} // SetValue

			private void SetOneValue(ExcelWorksheet sheet, int row, int column, TitledValue val) {
				sheet.SetCellValue(row, column, val.Title);
				sheet.SetCellValue(row, column + 1, val.Value, bIsBold: true);

				SetBorder(sheet.Cells[row, column]).Style.Border.Right.Style = ExcelBorderStyle.None;
				SetBorder(sheet.Cells[row, column + 1]).Style.Border.Left.Style = ExcelBorderStyle.None;
			} // SetOneValue

			private ExcelRange SetBorder(ExcelRange range) {
				range.Style.Border.BorderAround(ExcelBorderStyle.Thin, BorderColor);
				return range;
			} // SetBorder

			private class TitledValue {
				public static TitledValue Default {
					get { return defaultValue; }
				} // Default

				public TitledValue(string title = null, object val = null) {
					Title = title ?? string.Empty;
					Value = val ?? string.Empty;
				} // constructor

				public TitledValue(string title, decimal numerator, decimal denominator) {
					Title = title ?? string.Empty;
					Value = Math.Abs(denominator) < 0.000001m ? 0m : Math.Round(100m * numerator / denominator, 2);
				} // constructor

				public string Title { get; private set; }
				public object Value { get; private set; }

				private static readonly TitledValue defaultValue = new TitledValue();
			} // class TitledValue

			private static readonly Color BorderColor = Color.Black;

			private readonly bool takeMin;
			private readonly bool takeAll;
			private readonly string name;

			private int totalCount;
			private int autoProcessedCount;

			private int autoRejectedCount;
			private int autoRerejectedCount;

			private int autoApprovedCount;
			private int autoReapprovedCount;

			private int notAutoRejectedAndAutoApprovedCount;
			private int notAutoRejectedCount;

			private int manualRejectedCount;
			private int manualAndAutoRejectedCount;

			private int manualApprovedCount;
			private int manualAndAutoApprovedCount;

			private decimal manualAmount;
			private decimal autoAmount;

			private int manualDefaultCount;
			private int autoDefaultCount;

			private int manualRejectedAndAutoApprovedCount;
			private int manualApprovedAndAutoRejectedCount;

			private const int LastColumnNumber = 9;
		} // class Stats
	} // class WeeklyMaamMedalAndPricing
} // namespace

