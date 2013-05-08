using System;
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

        public byte[] GenerateReport(LoanDetails loanDetails, bool isExcell, string header)
        {
            int row = 4;
            var column = 1;
            var worksheet = _workbook.Worksheets[_workbook.Worksheets.ActiveSheetIndex];
            worksheet.Name = "Payment Schedule";
            HeaderReportGenerator.CreateHeader(worksheet, header, column);


            CreateXlsHeader(worksheet, row, column, isExcell);
            
            var filePath = System.Web.HttpContext.Current.Server.MapPath("~/Content/img/payment-to-customer.png");
            foreach (var transaction in loanDetails.PacnetTransactions)
            {
                row++;
                worksheet.Pictures.Add(row, column, filePath, 65, 25);

                worksheet.Cells[row, column + 1].PutValue(transaction.PostDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture));
                worksheet.Cells[row, column + 2].PutValue(FormattingUtils.FormatPoundsWidhDash(transaction.Amount));
                worksheet.Cells[row, column + 3].PutValue(FormattingUtils.FormatPoundsWidhDash(transaction.Interest));
                worksheet.Cells[row, column + 4].PutValue(FormattingUtils.FormatPoundsWidhDash(transaction.Fees));
                worksheet.Cells[row, column + 5].PutValue("-");
                worksheet.Cells[row, column + 6].PutValue(FormattingUtils.FormatPoundsWidhDash(transaction.Amount));
                worksheet.Cells[row, column + 7].PutValue(transaction.StatusDescription);
                if (isExcell) worksheet.Cells[row, column + 8].PutValue(transaction.Description);
                
                SetCellStyle(worksheet, row, column, false, isExcell);
            }
            filePath = System.Web.HttpContext.Current.Server.MapPath("~/Content/img/wizard-mark-completed.png");
            foreach (var transaction in loanDetails.Transactions)
            {
                row++;
                worksheet.Pictures.Add(row, column, filePath, 102, 30);

                worksheet.Cells[row, column + 1].PutValue(transaction.PostDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture));
                worksheet.Cells[row, column + 2].PutValue(FormattingUtils.FormatPoundsWidhDash(transaction.LoanRepayment));
                worksheet.Cells[row, column + 3].PutValue(FormattingUtils.FormatPoundsWidhDash(transaction.Interest));
                worksheet.Cells[row, column + 4].PutValue(FormattingUtils.FormatPoundsWidhDash(transaction.Fees + transaction.Rollover));
                worksheet.Cells[row, column + 5].PutValue("-");
                worksheet.Cells[row, column + 6].PutValue(FormattingUtils.FormatPoundsWidhDash(transaction.Amount));
                worksheet.Cells[row, column + 7].PutValue(transaction.StatusDescription);
                if (isExcell) worksheet.Cells[row, column + 8].PutValue(transaction.Description);
                
                SetCellStyle(worksheet, row, column, false, isExcell);
            }
            filePath = System.Web.HttpContext.Current.Server.MapPath("~/Content/img/two_arrows.png");
            foreach (var transaction in loanDetails.Rollovers)
            {
                row++;
                worksheet.Pictures.Add(row, column, filePath, 130, 50);
                worksheet.Cells[row, column + 1].PutValue(transaction.ExpiryDate);
                worksheet.Cells[row, column + 2].PutValue("-");
                worksheet.Cells[row, column + 3].PutValue("-");
                worksheet.Cells[row, column + 4].PutValue(FormattingUtils.FormatPoundsWidhDash(transaction.Payment));
                worksheet.Cells[row, column + 5].PutValue("-");
                worksheet.Cells[row, column + 6].PutValue("-");
                worksheet.Cells[row, column + 7].PutValue("Roll over");
                if (isExcell) worksheet.Cells[row, column + 8].PutValue("");
                
                SetCellStyle(worksheet, row, column, false, isExcell);
            }
            
            foreach (var transaction in loanDetails.Charges.Where(transaction => transaction.State != "Paid"))
            {
                row++;
                worksheet.Pictures.Add(row, column, filePath, 130, 50);
                worksheet.Cells[row, column + 1].PutValue(transaction.Date.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture));
                worksheet.Cells[row, column + 2].PutValue("-");
                worksheet.Cells[row, column + 3].PutValue("-");
                worksheet.Cells[row, column + 4].PutValue(FormattingUtils.FormatPoundsWidhDash(transaction.Amount));
                worksheet.Cells[row, column + 5].PutValue("-");
                worksheet.Cells[row, column + 6].PutValue("-");
                worksheet.Cells[row, column + 7].PutValue(transaction.State);
                if (isExcell) worksheet.Cells[row, column + 8].PutValue(transaction.Description);

                SetCellStyle(worksheet, row, column, false, isExcell);
            }

            filePath = System.Web.HttpContext.Current.Server.MapPath("~/Content/img/calendarx32.png");
            foreach (var transaction in loanDetails.Schedule.Where(transaction => (transaction.Status != "PaidEarly") && (transaction.Status != "PaidOnTime") && (transaction.Status != "Paid")))
            {
                row++;
                worksheet.Pictures.Add(row, column, filePath, 120, 45);

                worksheet.Cells[row, column + 1].PutValue(transaction.Date.ToString("dd/MM/yyyy",
                                                                           CultureInfo.InvariantCulture));
                worksheet.Cells[row, column + 2].PutValue(FormattingUtils.FormatPoundsWidhDash(transaction.LoanRepayment));
                worksheet.Cells[row, column + 3].PutValue(FormattingUtils.FormatPoundsWidhDash(transaction.Interest));
                worksheet.Cells[row, column + 4].PutValue(FormattingUtils.FormatPoundsWidhDash(transaction.LateCharges));
                worksheet.Cells[row, column + 5].PutValue("-");
                worksheet.Cells[row, column + 6].PutValue(FormattingUtils.FormatPoundsWidhDash(transaction.AmountDue));
                worksheet.Cells[row, column + 7].PutValue(transaction.StatusDescription);
                if (isExcell) worksheet.Cells[row, column + 8].PutValue("");

                SetCellStyle(worksheet, row, column, false, isExcell);
            }
            
            worksheet.Cells.SetColumnWidth(column -1, 1);
            worksheet.Cells.SetColumnWidth(column, 6);
            worksheet.Cells.SetColumnWidth(column + 1, 16);
            for (var item = 3; item < (9 + Convert.ToInt16(isExcell)); ++item)
            {
                worksheet.AutoFitColumn(item);
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

        public void CreateXlsHeader(Worksheet worksheet, int row, int column, bool isExcell)
        {
            SetCellStyle(worksheet, row, column, true, isExcell);
            worksheet.Cells[row, column    ].PutValue("");

            worksheet.Cells[row, column + 1].PutValue("Date");
            worksheet.Cells[row, column + 2].PutValue("Principal");
            worksheet.Cells[row, column + 3].PutValue("Interest");
            worksheet.Cells[row, column + 4].PutValue("Fees");
            worksheet.Cells[row, column + 5].PutValue("Rebate");
            worksheet.Cells[row, column + 6].PutValue("Total");
            worksheet.Cells[row, column + 7].PutValue("Status");
            if (isExcell) worksheet.Cells[row, column + 8].PutValue("Description");
           
            SetHeaderBackgroundColor(worksheet, row, column, isExcell);
            worksheet.AutoFitRow(row);

            
        }

        private void SetHeaderBackgroundColor(Worksheet worksheet, int row, int column, bool isExcell)
        {
            for (int i = 0; i < (8 + Convert.ToInt16(isExcell)); i++)
            {
                worksheet.Cells[row, column + i].Style.BackgroundColor = Color.Blue;
                worksheet.Cells[row, column + i].Style.Font.Color = Color.Black;
            }
            
        }

        private static void SetCellStyle(Worksheet worksheet, int row, int column, bool isBold, bool isExcell)
        {
            for (int i = 0; i < (8 + Convert.ToInt16(isExcell)); i++)
            {
                worksheet.Cells.SetRowHeight(row, column + i);
                worksheet.Cells[row, column + i].Style.Font.Size = 11;
                worksheet.Cells[row, column + i].Style.Font.Name = "Calibri";
                worksheet.Cells[row, column + i].Style.Font.IsBold = isBold;
                worksheet.Cells[row, column + i].Style.Font.Color = Color.Green;

                worksheet.Cells[row, column + i].Style.HorizontalAlignment = TextAlignmentType.Center;
                worksheet.Cells[row, column + i].Style.VerticalAlignment = TextAlignmentType.Center;
                worksheet.Cells[row, column + i].Style.ShrinkToFit = false;

                worksheet.Cells[row, column + i].Style.Borders[BorderType.BottomBorder].Color = Color.Black;
                worksheet.Cells[row, column + i].Style.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Medium;
                worksheet.Cells[row, column + i].Style.Borders[BorderType.LeftBorder].Color = Color.Black;
                worksheet.Cells[row, column + i].Style.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Medium;
                worksheet.Cells[row, column + i].Style.Borders[BorderType.RightBorder].Color = Color.Black;
                worksheet.Cells[row, column + i].Style.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Medium;
                worksheet.Cells[row, column + i].Style.Borders[BorderType.TopBorder].Color = Color.Black;
                worksheet.Cells[row, column + i].Style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Medium;
                worksheet.Cells[row, column + i].Style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Medium;
            }
            worksheet.Cells.SetRowHeight(row, 30);

        } 
    }
}