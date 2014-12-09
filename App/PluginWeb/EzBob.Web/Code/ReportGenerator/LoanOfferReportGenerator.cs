namespace EzBob.Web.Code.ReportGenerator
{
	using System;
	using System.Drawing;
	using System.Globalization;
	using System.IO;
	using Aspose.Cells;
	using Ezbob.Backend.Models;
	using Models;

	public class LoanOfferReportGenerator
	{
		private readonly Workbook _workbook;

		public LoanOfferReportGenerator()
		{
			_workbook = new Workbook();
		}

		public byte[] GenerateReport(LoanOffer loanOffer, bool isExcel, bool isShowDetails, string header)
		{
			var worksheet = _workbook.Worksheets[_workbook.Worksheets.ActiveSheetIndex];
			worksheet.Name = "Loan Offer";

			int row = 6;
			int column = 1;

			_workbook.ChangePalette(Color.FromArgb(197,197,197), 55);
			_workbook.ChangePalette(Color.FromArgb(221, 221, 221), 54);
			_workbook.ChangePalette(Color.FromArgb(123, 178, 36), 53);

			HeaderReportGenerator.CreateHeader(worksheet, header, column - 1, column, 7);

			worksheet.Cells.SetColumnWidth(0, 1);
			worksheet.Cells.SetColumnWidth(1, 16);
			worksheet.Cells.SetColumnWidth(2, 15);

			CreateXlsHeader(worksheet, row, column);
			var i = 0;
			foreach (var item in loanOffer.Schedule)
			{
				row++;
				i++;
				worksheet.Cells[row, column].PutValue(item.Date.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture));
				worksheet.Cells[row, column + 1].PutValue("£ " + FormattingUtils.FormatMiddle(item.LoanRepayment));
				worksheet.Cells[row, column + 2].PutValue("£ " + FormattingUtils.FormatMiddle(item.Interest));
				worksheet.Cells[row, column + 3].PutValue(FormattingUtils.FormatMiddle(item.InterestRate * 100) + "%");
				var fee = loanOffer.SetupFee > 0 && i == 1 ? loanOffer.SetupFee : 0;
				if (item.Fees > 0) fee += item.Fees;
				var res = fee != 0 ? "£ " + FormattingUtils.FormatMiddle(fee) : "-";
				var res1 = loanOffer.SetupFee > 0 && i == 1 ? "*" : string.Empty;
				worksheet.Cells[row, column + 4].PutValue(res + res1);
				//worksheet.Cells.Merge(row, column + 5, 1, 3);
				worksheet.Cells[row, column + 5].PutValue("£ " + FormattingUtils.FormatMiddle(item.AmountDue));
				SetCellStyle(worksheet, row, column, false);
			}

			row = CreateTotalBlock(loanOffer, row, column, worksheet);

			if (loanOffer.Details.IsModified)
			{
				row++;
				worksheet.Cells.Merge(row, column, 1, 7);
				worksheet.Cells[row, column].PutValue("Offer was manually modified");
				worksheet.Cells[row, column].Style.Font.IsBold = true;
			}
			if (isShowDetails)
			{
				CreateDetails(loanOffer.Details, row, column, worksheet);
			}

			return ConvertFormat(_workbook, isExcel ? FileFormatType.Excel2003 : FileFormatType.Pdf);
		}

		private static void CreateDetails(LoanOfferDetails details, int row, int column, Worksheet worksheet)
		{
			row += 2;
			worksheet.Cells[row, column].PutValue("Offered credit line: ");
			worksheet.Cells[row, column + 1].PutValue("£ " + FormattingUtils.FormatPounds(details.OfferedCreditLine));
			worksheet.Cells[row, column + 1].Style.HorizontalAlignment = TextAlignmentType.Left;
			worksheet.Cells[row, column + 1].Style.Font.IsBold = true;
			row++;
			worksheet.Cells[row, column].PutValue("Repayment period: ");
			worksheet.Cells[row, column + 1].PutValue(details.RepaymentPeriod);
			worksheet.Cells[row, column + 1].Style.HorizontalAlignment = TextAlignmentType.Left;
			worksheet.Cells[row, column + 1].Style.Font.IsBold = true;
			row++;
			worksheet.Cells[row, column].PutValue("Interest rate: ");
			worksheet.Cells[row, column + 1].PutValue(FormattingUtils.FormatMiddle(details.InterestRate * 100) + "%");
			worksheet.Cells[row, column + 1].Style.HorizontalAlignment = TextAlignmentType.Left;
			worksheet.Cells[row, column + 1].Style.Font.IsBold = true;
			row++;
			worksheet.Cells[row, column].PutValue("Loan type: ");
			worksheet.Cells[row, column + 1].PutValue(details.LoanType);
			worksheet.Cells[row, column + 1].Style.HorizontalAlignment = TextAlignmentType.Left;
			worksheet.Cells[row, column + 1].Style.Font.IsBold = true;
		}

		private int CreateTotalBlock(LoanOffer loanOffer, int row, int column, Worksheet worksheet)
		{
			row += 2;
			worksheet.Cells.Merge(row, column, 2, 1);
			worksheet.Cells[row, column + 1].PutValue("Loan");
			worksheet.Cells[row + 1, column + 1].PutValue("£ " + FormattingUtils.FormatMiddle(loanOffer.TotalPrincipal));

			worksheet.Cells.Merge(row, column + 2, 2, 1);
			worksheet.Cells.Merge(row, column + 3, 2, 1);

			worksheet.Cells[row, column + 4].PutValue("Cost");
			worksheet.Cells[row + 1, column + 4].PutValue("£ " + FormattingUtils.FormatMiddle(loanOffer.TotalInterest));

			worksheet.Cells.Merge(row, column + 5, 2, 1);

			worksheet.Cells[row, column + 6].PutValue("Total");
			worksheet.Cells[row + 1, column + 6].PutValue("£ " + FormattingUtils.FormatMiddle(loanOffer.Total));

			var filePath = System.Web.HttpContext.Current.Server.MapPath("~/Content/img/image-money64.png");

			worksheet.Pictures.Add(row, column + 0, filePath, 50, 50);

			filePath = System.Web.HttpContext.Current.Server.MapPath("~/Content/img/plus.png");
			worksheet.Pictures.Add(row, column + 2, filePath, 100, 80);

			filePath = System.Web.HttpContext.Current.Server.MapPath("~/Content/img/image-money32.png");
			worksheet.Pictures.Add(row, column + 3, filePath, 100, 80);

			filePath = System.Web.HttpContext.Current.Server.MapPath("~/Content/img/arrow-right.png");
			worksheet.Pictures.Add(row, column + 5, filePath, 100, 80);

			row += 2;

			worksheet.Cells.Merge(row, column + 0, 2, 7);
			worksheet.Cells[row, column + 0].PutValue(string.Format("Real Loan Cost ={0:0.00}%,    Apr={1}%",
														   loanOffer.RealInterestCost * 100, loanOffer.Apr));

			//worksheet.Cells[row, column].GetMergedRange().SetOutlineBorder(BorderType.BottomBorder, CellBorderType.Thin, Color.FromArgb(197, 197, 197));
			//worksheet.Cells[row, column].GetMergedRange().SetOutlineBorder(BorderType.LeftBorder, CellBorderType.Thin, Color.FromArgb(197, 197, 197));
			//worksheet.Cells[row, column].GetMergedRange().SetOutlineBorder(BorderType.RightBorder, CellBorderType.Thin, Color.FromArgb(197, 197, 197));
			//worksheet.Cells[row, column].GetMergedRange().SetOutlineBorder(BorderType.TopBorder, CellBorderType.Thin, Color.FromArgb(197, 197, 197));
			return row+1;
		}

		public static byte[] ConvertFormat(Workbook workbook, FileFormatType format)
		{
			using (var streamForDoc = new MemoryStream())
			{
				//Start magic. After new build we get an exception 'Unsupported sfnt version' on first load
				//On seccond request It works well. 
				try
				{
					workbook.Save(streamForDoc, format);
					return streamForDoc.ToArray();
				}
				catch (Exception)
				{
					workbook.Save(streamForDoc, format);
					return streamForDoc.ToArray();
				}
				//End magic

			}
		}

		public void CreateXlsHeader(Worksheet worksheet, int row, int column)
		{
			SetCellStyle(worksheet, row, column, true);
			worksheet.Cells[row, column].PutValue("Due Date");
			worksheet.Cells[row, column + 1].PutValue("Principal");
			worksheet.Cells[row, column + 2].PutValue("Interest");
			worksheet.Cells[row, column + 3].PutValue("Rate");
			worksheet.Cells[row, column + 4].PutValue("Fees");
			//worksheet.Cells.Merge(row, column + 5, 1, 3);
			worksheet.Cells[row, column + 5].PutValue("Total");

			SetHeaderBackgroundColor(worksheet, row, column);
			worksheet.AutoFitRows();
		}

		private void SetHeaderBackgroundColor(Worksheet worksheet, int row, int column)
		{
			for (int i = 0; i <= 7; i++)
			{
				worksheet.Cells[row, column + i].Style.BackgroundColor = Color.Blue;
				worksheet.Cells[row, column + i].Style.Font.Color = Color.Black;
			}
		}

		private static void SetCellStyle(Worksheet worksheet, int row, int column, bool isHeader)
		{
			var grey = Color.FromArgb(197, 197, 197);
			for (int i = 0; i <= 5; i++)
			{
				worksheet.Cells.SetRowHeight(row, i);
				worksheet.Cells[row, column + i].Style.Font.Size = isHeader ? 9 : 11;
				worksheet.Cells[row, column + i].Style.Font.Name = "Tahoma";
				worksheet.Cells[row, column + i].Style.Font.IsBold = isHeader;
				worksheet.Cells[row, column + i].Style.Font.Color = Color.FromArgb(123, 178, 36);
				worksheet.Cells[row, column + i].Style.Pattern = BackgroundType.Solid;
				worksheet.Cells[row, column + i].Style.BackgroundColor = row % 2 == 0 ? Color.FromArgb(221, 221, 221) : Color.White;
				worksheet.Cells[row, column + i].Style.HorizontalAlignment = TextAlignmentType.Center;
				worksheet.Cells[row, column + i].Style.VerticalAlignment = TextAlignmentType.Center;
				worksheet.Cells[row, column + i].Style.ShrinkToFit = true;

				if (i == 0)
				{
					worksheet.Cells[row, column + i].Style.Borders[BorderType.LeftBorder].Color = grey;
					worksheet.Cells[row, column + i].Style.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;
				}
				if (i == 5)
				{
					worksheet.Cells[row, column + i].Style.Borders[BorderType.RightBorder].Color = grey;
					worksheet.Cells[row, column + i].Style.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;
					worksheet.Cells[row, column + i].Style.Font.IsBold = true;
				}
				worksheet.Cells[row, column + i].Style.Borders[BorderType.BottomBorder].Color = grey;
				worksheet.Cells[row, column + i].Style.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;
				worksheet.Cells[row, column + i].Style.Borders[BorderType.TopBorder].Color = grey;
				worksheet.Cells[row, column + i].Style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;
				worksheet.Cells.SetRowHeight(row, 100);
			}
			worksheet.AutoFitRows();
		}
	}
}
