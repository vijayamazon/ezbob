using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using Aspose.Cells;
using EzBob.Web.Areas.Customer.Models;

namespace EzBob.Web.Code.ReportGenerator
{
    public class LoanScheduleReportGenerator
    {
        private readonly Workbook _workbook;

        public LoanScheduleReportGenerator()
        {
            _workbook = new Workbook();
        }

        public byte[] GenerateReport(LoanDetails loanDetails, bool isExcell)
        {
            var worksheet = _workbook.Worksheets[_workbook.Worksheets.ActiveSheetIndex];
            worksheet.Name = "Payment Schedule";
           
            int row = 1;
            CreateXlsHeader(worksheet, row);
            
            var filePath = System.Web.HttpContext.Current.Server.MapPath("~/Content/img/payment-to-customer.png");
            foreach (var transaction in loanDetails.PacnetTransactions)
            {
                row++;
                worksheet.Pictures.Add(row, 0, filePath, 25, 25);
              
                worksheet.Cells[row, 1].PutValue(transaction.PostDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture));
                worksheet.Cells[row, 2].PutValue(FormattingUtils.FormatPoundsWidhDash(transaction.Amount));
                worksheet.Cells[row, 3].PutValue(FormattingUtils.FormatPoundsWidhDash(transaction.Interest));
                worksheet.Cells[row, 4].PutValue(FormattingUtils.FormatPoundsWidhDash(transaction.Fees));
                worksheet.Cells[row, 5].PutValue("-");
                worksheet.Cells[row, 6].PutValue(FormattingUtils.FormatPoundsWidhDash(transaction.Amount));
                worksheet.Cells[row, 7].PutValue(transaction.StatusDescription);
                
                
                SetCellStyle(worksheet, row, false);
            }
            filePath = System.Web.HttpContext.Current.Server.MapPath("~/Content/img/wizard-mark-completed.png");
            foreach (var transaction in loanDetails.Transactions)
            {
                row++;
                worksheet.Pictures.Add(row, 0, filePath, 100, 50);
                worksheet.Cells[row, 1].PutValue(transaction.PostDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture));
                worksheet.Cells[row, 2].PutValue(FormattingUtils.FormatPoundsWidhDash(transaction.LoanRepayment));
                worksheet.Cells[row, 3].PutValue(FormattingUtils.FormatPoundsWidhDash(transaction.Interest));
                worksheet.Cells[row, 4].PutValue(FormattingUtils.FormatPoundsWidhDash(transaction.Fees + transaction.Rollover));
                worksheet.Cells[row, 5].PutValue("-");
                worksheet.Cells[row, 6].PutValue(FormattingUtils.FormatPoundsWidhDash(transaction.Amount));
                worksheet.Cells[row, 7].PutValue(transaction.StatusDescription);
                
                SetCellStyle(worksheet, row, false);
            }
            filePath = System.Web.HttpContext.Current.Server.MapPath("~/Content/img/two_arrows.png");
            foreach (var transaction in loanDetails.Rollovers)
            {
                row++;
                worksheet.Pictures.Add(row, 0, filePath, 100, 50);
                worksheet.Cells[row, 1].PutValue(transaction.ExpiryDate);
                worksheet.Cells[row, 2].PutValue("-");
                worksheet.Cells[row, 3].PutValue("-");
                worksheet.Cells[row, 4].PutValue(FormattingUtils.FormatPoundsWidhDash(transaction.Payment));
                worksheet.Cells[row, 5].PutValue("-");
                worksheet.Cells[row, 6].PutValue("-");
                worksheet.Cells[row, 7].PutValue("Roll over");
                
                SetCellStyle(worksheet, row, false);
            }
            
            foreach (var transaction in loanDetails.Charges.Where(transaction => transaction.State != "Paid"))
            {
                row++;
                worksheet.Pictures.Add(row, 0, filePath, 100, 50);
                worksheet.Cells[row, 1].PutValue(transaction.Date.ToString("dd/MM/yyyy",CultureInfo.InvariantCulture));
                worksheet.Cells[row, 2].PutValue("-");
                worksheet.Cells[row, 3].PutValue("-");
                worksheet.Cells[row, 4].PutValue(FormattingUtils.FormatPoundsWidhDash(transaction.Amount));
                worksheet.Cells[row, 5].PutValue("-");
                worksheet.Cells[row, 6].PutValue("-");
                worksheet.Cells[row, 7].PutValue(transaction.State);
                SetCellStyle(worksheet, row, false);
            }
            filePath = System.Web.HttpContext.Current.Server.MapPath("~/Content/img/calendarx32.png");

            foreach (var transaction in loanDetails.Schedule.Where(transaction => (transaction.Status != "PaidEarly") && (transaction.Status != "PaidOnTime") && (transaction.Status != "Paid")))
            {
                row++;
                worksheet.Pictures.Add(row, 0, filePath, 100, 50);
                worksheet.Cells[row, 0].Style.HorizontalAlignment = TextAlignmentType.Center;
                worksheet.Cells[row, 0].Style.VerticalAlignment = TextAlignmentType.Center;
                worksheet.Cells[row, 1].PutValue(transaction.Date.ToString("dd/MM/yyyy",
                                                                           CultureInfo.InvariantCulture));
                worksheet.Cells[row, 2].PutValue(FormattingUtils.FormatPoundsWidhDash(transaction.LoanRepayment));
                worksheet.Cells[row, 3].PutValue(FormattingUtils.FormatPoundsWidhDash(transaction.Interest));
                worksheet.Cells[row, 4].PutValue(FormattingUtils.FormatPoundsWidhDash(transaction.LateCharges));
                worksheet.Cells[row, 5].PutValue("-");
                worksheet.Cells[row, 6].PutValue(FormattingUtils.FormatPoundsWidhDash(transaction.AmountDue));
                worksheet.Cells[row, 7].PutValue(transaction.StatusDescription);
                SetCellStyle(worksheet, row, false);
            }

            return ConvertFormat(_workbook, isExcell ? FileFormatType.Excel2003 : FileFormatType.Pdf);
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
            worksheet.Cells[row, 0].PutValue("");

            worksheet.Cells[row, 1].PutValue("Date");
            worksheet.Cells[row, 2].PutValue("Principal");
            worksheet.Cells[row, 3].PutValue("Interest");
            worksheet.Cells[row, 4].PutValue("Fees");
            worksheet.Cells[row, 5].PutValue("Rebate");
            worksheet.Cells[row, 6].PutValue("Total");
            worksheet.Cells[row, 7].PutValue("Status");
           
            SetHeaderBackgroundColor(worksheet, row);
            worksheet.AutoFitColumns();
            worksheet.AutoFitRows();

            
        }

        private void SetHeaderBackgroundColor(Worksheet worksheet, int row)
        {
            for (int i = 0; i < 8; i++)
            {
                worksheet.Cells[row, i].Style.BackgroundColor = Color.Blue;
                worksheet.Cells[row, i].Style.Font.Color = Color.Black;
            }
            
        }

        private static void SetCellStyle(Worksheet worksheet, int row, bool isBold)
        {
            for (int i = 0; i < 8; i++)
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

            //worksheet.AutoFitRows();
            worksheet.AutoFitColumns();

        } 
    }
}