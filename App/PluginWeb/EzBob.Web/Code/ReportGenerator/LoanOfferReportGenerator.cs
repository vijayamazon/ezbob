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
        
        public byte[] GenerateReport(LoanOffer loanOffer, bool isExcel, bool isShowDetails)
        {
            var worksheet = _workbook.Worksheets[_workbook.Worksheets.ActiveSheetIndex];
            worksheet.Name = "Payment Schedule";
           
            int row = 1;
            CreateXlsHeader(worksheet, row);
            
            foreach (var item in loanOffer.Schedule)
            {
                row++;
                worksheet.Cells[row, 0].PutValue(item.Date.ToString("dd/MM/yyyy",CultureInfo.InvariantCulture));
                worksheet.Cells[row, 1].PutValue(FormattingUtils.FormatPounds(item.LoanRepayment));
                worksheet.Cells[row, 2].PutValue(FormattingUtils.FormatPounds(item.Interest));
                worksheet.Cells[row, 3].PutValue("-");
                worksheet.Cells.Merge(row, 4, 1, 3);
                worksheet.Cells[row, 4].PutValue(FormattingUtils.FormatPounds(item.AmountDue));
                SetCellStyle(worksheet, row, false);
            }
        
            row = row+2;
            worksheet.Cells.Merge(row, 0, 2, 1);
            worksheet.Cells[row,1].PutValue("Loan");
            worksheet.Cells[row + 1, 1].PutValue(FormattingUtils.FormatPounds(loanOffer.TotalPrincipal));

            worksheet.Cells.Merge(row, 2, 2, 1);
            worksheet.Cells.Merge(row, 3, 2, 1);

            worksheet.Cells[row, 4].PutValue("Cost");
            worksheet.Cells[row + 1, 4].PutValue(FormattingUtils.FormatPounds(loanOffer.TotalInterest));

            worksheet.Cells.Merge(row, 5, 2, 1);
            worksheet.Cells[row + 1, 4].PutValue(FormattingUtils.FormatPounds(loanOffer.TotalInterest));

            worksheet.Cells[row, 6].PutValue("Cost");
            worksheet.Cells[row + 1, 6].PutValue(FormattingUtils.FormatPounds(loanOffer.TotalInterest));

            var filePath = System.Web.HttpContext.Current.Server.MapPath("~/Content/img/image-money64.png");
            worksheet.Pictures.Add(row, 0, filePath, 100, 50);

            filePath = System.Web.HttpContext.Current.Server.MapPath("~/Content/img/plus.png");
            worksheet.Pictures.Add(row, 2, filePath, 100, 80);

            filePath = System.Web.HttpContext.Current.Server.MapPath("~/Content/img/image-money32.png");
            worksheet.Pictures.Add(row, 3, filePath, 100, 80);

            filePath = System.Web.HttpContext.Current.Server.MapPath("~/Content/img/arrow-right.png");
            worksheet.Pictures.Add(row, 5, filePath, 100, 80);
            row++;
            row++;
            worksheet.Cells.Merge(row, 0, 1, 7);

            worksheet.Cells[row, 0].PutValue(string.Format("Real Loan Cost ={0}% Apr={1}%", loanOffer.RealInterestCost * 100, loanOffer.Apr));
            if (isShowDetails)
            {
                row++;
                row++;
                worksheet.Cells[row, 0].PutValue("Offered credit line: ");
                worksheet.Cells[row, 1].PutValue(loanOffer.Details.OfferedCreditLine);
                row++;
                worksheet.Cells[row, 0].PutValue("Repayment period: ");
                worksheet.Cells[row, 1].PutValue(loanOffer.Details.RepaymentPeriod);
                row++;
                worksheet.Cells[row, 0].PutValue("Interest rate: ");
                worksheet.Cells[row, 1].PutValue(string.Format("{0}%", loanOffer.Details.InterestRate*100));
                row++;
                worksheet.Cells[row, 0].PutValue("Loan type: ");
                worksheet.Cells[row, 1].PutValue(loanOffer.Details.LoanType);
            }

            return ConvertFormat(_workbook, isExcel ? FileFormatType.Excel2003 : FileFormatType.Pdf);
        }

        public static byte[] ConvertFormat(Workbook workbook, FileFormatType format)
        {
            using (var streamForDoc = new MemoryStream())
            {
                workbook.Save(streamForDoc, format);
                return streamForDoc.ToArray();
            }
        }
       
        public void CreateXlsHeader(Worksheet worksheet, int row)
        {
            SetCellStyle(worksheet, row, true);
            worksheet.Cells[row, 0].PutValue("Due Date");
            worksheet.Cells[row, 1].PutValue("Principal");
            worksheet.Cells[row, 2].PutValue("Interest");
            worksheet.Cells[row, 3].PutValue("Fees");
            worksheet.Cells.Merge(row, 4, 1, 3);
            worksheet.Cells[row, 4].PutValue("Total");
          
            SetHeaderBackgroundColor(worksheet, row);
            worksheet.AutoFitColumns();
            worksheet.AutoFitRows();
        }

        private void SetHeaderBackgroundColor(Worksheet worksheet, int row)
        {
            for (int i = 0; i <= 6; i++)
            {
                worksheet.Cells[row, i].Style.BackgroundColor = Color.Blue;
                worksheet.Cells[row, i].Style.Font.Color = Color.Black;
            }
        }

        private static void SetCellStyle(Worksheet worksheet, int row, bool isBold)
        {
            for (int i = 0; i <= 6; i++)
            {
                worksheet.Cells.SetRowHeight(row, i);
                worksheet.Cells[row, i].Style.Font.Size = 11;
                worksheet.Cells[row, i].Style.Font.Name = "Calibri";
                worksheet.Cells[row, i].Style.Font.IsBold = isBold;
                worksheet.Cells[row, i].Style.Font.Color = Color.Green;
                
                worksheet.Cells[row, i].Style.HorizontalAlignment =  TextAlignmentType.Center;
                worksheet.Cells[row, i].Style.VerticalAlignment =  TextAlignmentType.Center;
                worksheet.Cells[row, i].Style.ShrinkToFit = true;
            
                worksheet.Cells[row, i].Style.Borders[BorderType.BottomBorder].Color = Color.Black;
                worksheet.Cells[row, i].Style.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Medium;
                worksheet.Cells[row, i].Style.Borders[BorderType.LeftBorder].Color = Color.Black;
                worksheet.Cells[row, i].Style.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Medium;
                worksheet.Cells[row, i].Style.Borders[BorderType.RightBorder].Color = Color.Black;
                worksheet.Cells[row, i].Style.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Medium;
                worksheet.Cells[row, i].Style.Borders[BorderType.TopBorder].Color = Color.Black;
                worksheet.Cells[row, i].Style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Medium;
                worksheet.Cells[row, i].Style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Medium;
                worksheet.Cells.SetRowHeight(row, 60);
            }
            worksheet.AutoFitRows();
            worksheet.AutoFitColumns();

        } 
    }
}