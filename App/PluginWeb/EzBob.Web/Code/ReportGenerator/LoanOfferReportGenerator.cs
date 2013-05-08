using System.Drawing;
using System.Globalization;
using System.IO;
using Aspose.Cells;
using EzBob.Models;

namespace EzBob.Web.Code.ReportGenerator
{
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

            int row = 4;
            int column = 1;

            HeaderReportGenerator.CreateHeader(worksheet, header, column -1);

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
                worksheet.Cells[row, column+1].PutValue(FormattingUtils.FormatPounds(item.LoanRepayment));
                worksheet.Cells[row, column + 2].PutValue(FormattingUtils.FormatPounds(item.Interest));
                var fee =loanOffer.SetupFee > 0 && i == 0 ? loanOffer.SetupFee : 0;
                if (item.Fees>0) fee += item.Fees;
                var res = fee != 0 ? FormattingUtils.FormatPounds(fee) : "-";
                var res1 = loanOffer.SetupFee>0 && i == 0 ? "*" : string.Empty;
                worksheet.Cells[row, column + 3].PutValue(res + res1);
                worksheet.Cells.Merge(row, column + 4, 1, 3);
                worksheet.Cells[row, column + 4].PutValue(FormattingUtils.FormatPounds(item.AmountDue));
                SetCellStyle(worksheet, row, column, false);
            }

            row = CreateTotalBlock(loanOffer, row, column, worksheet);

            if (loanOffer.Details.IsModified)
            {
                row++;
                worksheet.Cells.Merge(row, column, 1, 7);
                worksheet.Cells[row, column ].PutValue("Offer was manually modified");
                worksheet.Cells[row, column ].Style.Font.IsBold = true;
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
            worksheet.Cells[row, column+1].PutValue(FormattingUtils.FormatPounds(details.OfferedCreditLine));
            worksheet.Cells[row, column + 1].Style.Font.IsBold = true;
            row++;
            worksheet.Cells[row, column ].PutValue("Repayment period: ");
            worksheet.Cells[row, column + 1].PutValue(details.RepaymentPeriod);
            worksheet.Cells[row, column + 1].Style.HorizontalAlignment = TextAlignmentType.Left;
            worksheet.Cells[row, column + 1].Style.Font.IsBold = true;
            row++;
            worksheet.Cells[row, column ].PutValue("Interest rate: ");
            worksheet.Cells[row, column + 1].PutValue(string.Format("{0:0}%", details.InterestRate * 100));
            worksheet.Cells[row, column + 1].Style.Font.IsBold = true;
            row++;
            worksheet.Cells[row, column ].PutValue("Loan type: ");
            worksheet.Cells[row, column + 1].PutValue(details.LoanType);
            worksheet.Cells[row, column + 1].Style.Font.IsBold = true;
        }

       

        private  int CreateTotalBlock(LoanOffer loanOffer, int row, int column, Worksheet worksheet)
        {
            row += 2;
            worksheet.Cells.Merge(row, column, 2, 1);
            worksheet.Cells[row, column + 1].PutValue("Loan");
            worksheet.Cells[row + 1, column + 1].PutValue(FormattingUtils.FormatPounds(loanOffer.TotalPrincipal));

            worksheet.Cells.Merge(row, column + 2, 2, 1);
            worksheet.Cells.Merge(row, column + 3, 2, 1);

            worksheet.Cells[row, column + 4].PutValue("Cost");
            worksheet.Cells[row + 1, column + 4].PutValue(FormattingUtils.FormatPounds(loanOffer.TotalInterest));

            worksheet.Cells.Merge(row, column + 5, 2, 1);

            worksheet.Cells[row, column + 6].PutValue("Total");
            worksheet.Cells[row + 1, column + 6].PutValue(FormattingUtils.FormatPounds(loanOffer.Total));

            var filePath = System.Web.HttpContext.Current.Server.MapPath("~/Content/img/image-money64.png");

            worksheet.Pictures.Add(row, column + 0, filePath, 50, 50);

            filePath = System.Web.HttpContext.Current.Server.MapPath("~/Content/img/plus.png");
            worksheet.Pictures.Add(row, column + 2, filePath, 100, 80);

            filePath = System.Web.HttpContext.Current.Server.MapPath("~/Content/img/image-money32.png");
            worksheet.Pictures.Add(row, column + 3, filePath, 100, 80);

            filePath = System.Web.HttpContext.Current.Server.MapPath("~/Content/img/arrow-right.png");
            worksheet.Pictures.Add(row, column + 5, filePath, 100, 80);

            row += 2;

            worksheet.Cells.Merge(row, column + 0, 1, 7);
            worksheet.Cells[row, column + 0].PutValue(string.Format("Real Loan Cost ={0:0.00}%,    Apr={1}%",
                                                           loanOffer.RealInterestCost*100, loanOffer.Apr));
            return row;
        }

        public static byte[] ConvertFormat(Workbook workbook, FileFormatType format)
        {
            using (var streamForDoc = new MemoryStream())
            {
                workbook.Save(streamForDoc, format);
                return streamForDoc.ToArray();
            }
        }

        public void CreateXlsHeader(Worksheet worksheet, int row, int column)
        {
            SetCellStyle(worksheet, row, column, true);
            worksheet.Cells[row, column].PutValue("Due Date");
            worksheet.Cells[row, column + 1].PutValue("Principal");
            worksheet.Cells[row, column + 2].PutValue("Interest");
            worksheet.Cells[row, column + 3].PutValue("Fees");
            worksheet.Cells.Merge(row, column + 4, 1, 3);
            worksheet.Cells[row, column + 4].PutValue("Total");

            SetHeaderBackgroundColor(worksheet, row, column);
            worksheet.AutoFitRows();
        }

        private void SetHeaderBackgroundColor(Worksheet worksheet, int row, int column)
        {
            for (int i = 0; i <= 6; i++)
            {
                worksheet.Cells[row, column + i].Style.BackgroundColor = Color.Blue;
                worksheet.Cells[row, column + i].Style.Font.Color = Color.Black;
            }
        }

        private static void SetCellStyle(Worksheet worksheet, int row, int column, bool isBold)
        {
            for (int i = 0; i <= 6; i++)
            {
                worksheet.Cells.SetRowHeight(row, i);
                worksheet.Cells[row, column + i].Style.Font.Size = 11;
                worksheet.Cells[row, column + i].Style.Font.Name = "Calibri";
                worksheet.Cells[row, column + i].Style.Font.IsBold = isBold;
                worksheet.Cells[row, column + i].Style.Font.Color = Color.Green;

                worksheet.Cells[row, column + i].Style.HorizontalAlignment = TextAlignmentType.Center;
                worksheet.Cells[row, column + i].Style.VerticalAlignment = TextAlignmentType.Center;
                worksheet.Cells[row, column + i].Style.ShrinkToFit = true;

                worksheet.Cells[row, column + i].Style.Borders[BorderType.BottomBorder].Color = Color.Black;
                worksheet.Cells[row, column + i].Style.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Medium;
                worksheet.Cells[row, column + i].Style.Borders[BorderType.LeftBorder].Color = Color.Black;
                worksheet.Cells[row, column + i].Style.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Medium;
                worksheet.Cells[row, column + i].Style.Borders[BorderType.RightBorder].Color = Color.Black;
                worksheet.Cells[row, column + i].Style.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Medium;
                worksheet.Cells[row, column + i].Style.Borders[BorderType.TopBorder].Color = Color.Black;
                worksheet.Cells[row, column + i].Style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Medium;
                worksheet.Cells[row, column + i].Style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Medium;
                worksheet.Cells.SetRowHeight(row, 60);
            }
            worksheet.AutoFitRows();
        }
    }
}