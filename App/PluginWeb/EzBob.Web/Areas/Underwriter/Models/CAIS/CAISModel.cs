using System.Globalization;
using EZBob.DatabaseLib.Model.Database;

namespace EzBob.Web.Areas.Underwriter.Models.CAIS
{
	using Ezbob.Backend.Models;

	public class CaisModel
    {
        public string Date { get; set; }
        public string FileName { get; set; }
        public string Type { get; set; }
        public string OfItems { get; set; }
        public string GoodUsers { get; set; }
        public string CaisUploadStatus { get; set; }
        public string Defaults { get; set; }
        public int Id { get; set; }

        public static CaisModel FromModel(CaisReportsHistory caisReport) {
	        return new CaisModel {
		        Id = caisReport.Id,
		        CaisUploadStatus = caisReport.UploadStatus.ToString(),
		        Date = FormattingUtils.FormatDateTimeToString(caisReport.Date),
		        GoodUsers = caisReport.GoodUsers.ToString(),
		        FileName = caisReport.FileName,
		        Type = caisReport.Type.ToString(),
		        OfItems = caisReport.OfItems.ToString(),
		        Defaults = caisReport.Defaults.HasValue ? caisReport.Defaults.Value.ToString(CultureInfo.InvariantCulture) : "-"
	        };
        }
    }

    public class CaisSendModel
    {
        public int Id { get; set; }
    }
}