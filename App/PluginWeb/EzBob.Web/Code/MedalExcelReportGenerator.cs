using System.Drawing;
using System.Globalization;
using Aspose.Cells;
using EzBob.Web.Areas.Underwriter.Models;

namespace EzBob.Web.Code
{
	using System.Linq;

	public class MedalExcelReportGenerator
    {
        private readonly Workbook _workbook;

        public MedalExcelReportGenerator()
        {
            _workbook = new Workbook();
        }

        public byte[] GenerateReport(MedalCalculators medalCalculators, string fullName)
        {
            var worksheet = _workbook.Worksheets[_workbook.Worksheets.ActiveSheetIndex];
            worksheet.Name = "Medal Calculations";
            CreateHead(worksheet, fullName);
            int row = 2;
            CreateXlsHeaderMedalResult(worksheet, row);
            row++;
            CreateXlsContentMedalResult(medalCalculators, row, worksheet);
            row = row + 3;
            CreateXlsHeaderMedalCharecteristic(row, worksheet);
	        var lastMedal = medalCalculators.DetailedHistory.MedalDetailsHistories.FirstOrDefault();
	        if (lastMedal != null) {
		        foreach (var medal in lastMedal.MedalCharacteristics) {
			        row++;
			        worksheet.Cells[row, 0].PutValue(medal.CustomerCharacteristic);
			        worksheet.Cells[row, 1].PutValue(medal.WeightUsed.ToString(CultureInfo.InvariantCulture) + "%");
			        worksheet.Cells[row, 2].PutValue(medal.ACParameters.ToString(CultureInfo.InvariantCulture) + " from " +
			                                         medal.MaxPoss + " ( " + medal.PointsObtainedPercent + "%) ");
			        SetCellStyle(worksheet, row, false);
		        }
		        row++;
		        worksheet.Cells[row, 0].PutValue("Total");
		        worksheet.Cells[row, 1].PutValue(lastMedal.TotalWeightUsed.ToString(CultureInfo.InvariantCulture) + "%");
		        worksheet.Cells[row, 2].PutValue(lastMedal.TotalACParameters.ToString(CultureInfo.InvariantCulture) + " from " +
		                                         lastMedal.TotalMaxPoss + " ( " + lastMedal.TotalPointsObtainedPercent + "%) ");
		        SetCellStyle(worksheet, row, true);
	        }
	        return _workbook.SaveToStream().ToArray();
        }

        private static void CreateXlsContentMedalResult(MedalCalculators medalCalculators, int row, Worksheet worksheet)
        {
            var medalResult = medalCalculators.Score;
            worksheet.Cells[row, 0].PutValue(medalResult.Medal);
            worksheet.Cells[row, 1].PutValue(medalResult.Points);
            worksheet.Cells[row, 2].PutValue(medalResult.Result);
            SetCellStyle(worksheet, 3, false);
        }

        private static void CreateHead(Worksheet worksheet, string headText)
        {
            worksheet.Cells.Merge(0, 0, 2, 3);
            worksheet.Cells[0, 0].PutValue(headText);
        }

        public void CreateXlsHeaderMedalResult(Worksheet worksheet, int row)
        {
            SetCellStyle(worksheet, row, true);
            worksheet.Cells[row, 0].PutValue("Medal");
            worksheet.Cells[row, 1].PutValue("Points");
            worksheet.Cells[row, 2].PutValue("Result");

            worksheet.AutoFitRows();
            worksheet.AutoFitColumns();
        }

        public void CreateXlsHeaderMedalCharecteristic(int row, Worksheet worksheet)
        {
            SetCellStyle(worksheet, row, true);
            worksheet.Cells[row, 0].PutValue("Customer Characteristic");
            worksheet.Cells[row, 1].PutValue("Weight Used");
            worksheet.Cells[row, 2].PutValue("Points Obtained");

            worksheet.AutoFitRows();
            worksheet.AutoFitColumns();
        }

        private static void SetCellStyle(Worksheet worksheet, int row, bool isBold)
        {
            for (int i = 0; i < 3; i++)
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
