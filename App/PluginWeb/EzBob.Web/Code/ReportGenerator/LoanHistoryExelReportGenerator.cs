using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Globalization;
using Aspose.Cells;
using EzBob.Models;

namespace EzBob.Web.Code.ExelReportGenarator
{
	using Ezbob.Backend.Models;

	public class LoanHistoryExelReportGenerator
    {
        private readonly Workbook _workbook;

        public LoanHistoryExelReportGenerator()
        {
            _workbook = new Workbook();
        }

        public byte[] GenerateReport(IList<LoanModel> loans , string fullName)
        {
            var worksheet = _workbook.Worksheets[_workbook.Worksheets.ActiveSheetIndex];
            worksheet.Name = "Loans History";
            CreateHead(worksheet, fullName);
            int row = 2;
            CreateXlsHeader(worksheet, row);
            row++;
            var sumLoanAmount = loans.Sum(x => x.LoanAmount);
            var sumRepayments = loans.Sum(x => x.Repayments);
            var sumBalance = loans.Sum(x => x.Balance);
            foreach (var loan in loans)
            {
                row++;
                worksheet.Cells[row, 0].PutValue(loan.RefNumber);

                worksheet.Cells[row, 1].PutValue(FormattingUtils.NumericFormats(loan.LoanAmount));
                worksheet.Cells[row, 2].PutValue(FormattingUtils.NumericFormats(loan.Repayments));
                worksheet.Cells[row, 3].PutValue(loan.Date.ToString("MMM dd yyyy", CultureInfo.CreateSpecificCulture("en-gb")));
                worksheet.Cells[row, 4].PutValue(FormattingUtils.NumericFormats(loan.Balance));
                worksheet.Cells[row, 5].PutValue(loan.StatusDescription);
                SetCellStyle(worksheet, row, false);
            }
            row++;
            worksheet.Cells[row, 0].PutValue("Total");
            worksheet.Cells[row, 1].PutValue(FormattingUtils.NumericFormats(sumLoanAmount));
            worksheet.Cells[row, 2].PutValue(FormattingUtils.NumericFormats(sumRepayments));
            worksheet.Cells[row, 4].PutValue(FormattingUtils.NumericFormats(sumBalance));
            SetCellStyle(worksheet, row, true);
            return _workbook.SaveToStream().ToArray();
        }

        private static void CreateHead(Worksheet worksheet, string headText)
        {
            worksheet.Cells.Merge(0, 0, 2, 6);
            worksheet.Cells[0, 0].PutValue(headText);
        }

        public void CreateXlsHeader(Worksheet worksheet, int row)
        {
            SetCellStyle(worksheet, row, true);
            worksheet.Cells[row, 0].PutValue("Loan ref number");
            worksheet.Cells[row, 1].PutValue("Loan amount");
            worksheet.Cells[row, 2].PutValue("Repayments");
            worksheet.Cells[row, 3].PutValue("Date Applied	");
            worksheet.Cells[row, 4].PutValue("Outstanding");
            worksheet.Cells[row, 5].PutValue("Status");

            worksheet.AutoFitRows();
            worksheet.AutoFitColumns();
        }

        private static void SetCellStyle(Worksheet worksheet, int row, bool isBold)
        {
            for (int i = 0; i < 6; i++)
            {
                worksheet.Cells[row, i].Style.Font.Size = 11;
                worksheet.Cells[row, i].Style.Font.Name = "Calibri";
                worksheet.Cells[row, i].Style.Font.IsBold = isBold;
                worksheet.Cells[row, i].Style.HorizontalAlignment = TextAlignmentType.Left;
                worksheet.Cells[row, i].Style.ShrinkToFit = true;

                worksheet.Cells[row, i].Style.Borders[BorderType.BottomBorder].Color = Color.Black;
                worksheet.Cells[row, i].Style.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Medium;
                worksheet.Cells[row, i].Style.Borders[BorderType.LeftBorder].Color = Color.Black;
                worksheet.Cells[row, i].Style.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Medium;
                worksheet.Cells[row, i].Style.Borders[BorderType.RightBorder].Color = Color.Black;
                worksheet.Cells[row, i].Style.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Medium;
                worksheet.Cells[row, i].Style.Borders[BorderType.TopBorder].Color = Color.Black;
                worksheet.Cells[row, i].Style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Medium;
            }
            worksheet.AutoFitRows();
            worksheet.AutoFitColumns();
        } 
    }
}
