using System.Web;
using Aspose.Cells;

namespace EzBob.Web.Code.ReportGenerator
{
    public class HeaderReportGenerator 
    {
        public static void CreateHeader(Worksheet worksheet, string header)
        {
            worksheet.Cells.Merge(0, 0, 2, 1);
            var filePath = HttpContext.Current.Server.MapPath("~/Content/img/ezbob-logo.png");
            worksheet.Pictures.Add(0, 0, filePath, 25, 30);
            worksheet.Cells.Merge(0, 1, 2, 6);
            worksheet.Cells[0, 1].PutValue(header);
        }
    }
}
