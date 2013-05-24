using System.Web;
using Aspose.Cells;

namespace EzBob.Web.Code.ReportGenerator
{
    public class HeaderReportGenerator 
    {
        public static void CreateHeader(Worksheet worksheet, string header, int column, int startHeaderMessageColumn, int lengthHeaderMessage)
        {
            worksheet.Cells.Merge(0, column + 1, 4, 1);
            var filePath = HttpContext.Current.Server.MapPath("~/Content/img/ezbob-logo.png");
            int pictureIndex = worksheet.Pictures.Add(0, column + 1, filePath, 25, 51);

            /* This feauture has been implemented in the last versions of Aspose Cells, so it will work after upgrade aspose cells.
            //Accessing the newly added picture
                Aspose.Cells.Drawing.Picture picture = worksheet.Pictures[pictureIndex];
            //Positioning the picture proportional to row height and colum width
                picture.UpperDeltaX = 200;
                picture.UpperDeltaY = 200;
            */

            //worksheet.Cells.Merge(1, column + 2, 2, 6);
            worksheet.Cells.Merge(4, startHeaderMessageColumn, 2, lengthHeaderMessage);
            worksheet.Cells[4, startHeaderMessageColumn].PutValue(header);
        }
    }
}
