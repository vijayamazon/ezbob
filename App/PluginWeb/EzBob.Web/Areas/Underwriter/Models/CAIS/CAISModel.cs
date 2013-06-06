using System.Collections.Generic;
using System.Globalization;
using EZBob.DatabaseLib.Model.Database;
using System.Linq;
using EzBob.Web.Code;

namespace EzBob.Web.Areas.Underwriter.Models.CAIS
{
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

        public static List<CaisModel> FromModel(IEnumerable<CaisReportsHistory>  reportsHistory)
        {
            var model = reportsHistory.OrderByDescending(x => x.Date);
            return model
                .Select(x => new CaisModel
                    {
                        Id = x.Id,
                        CaisUploadStatus = x.UploadStatus.ToString(),
                        Date = FormattingUtils.FormatDateTimeToString(x.Date),
                        GoodUsers = x.GoodUsers.ToString(),
                        FileName = x.FileName,
                        Type = x.Type.ToString(),
                        OfItems = x.OfItems.ToString(),
                        Defaults = x.Defaults.HasValue ? x.Defaults.Value.ToString(CultureInfo.InvariantCulture) : "-"
                    }).ToList();
        }
    }

    public class CaisSendModel
    {
        public int Id { get; set; }
    }
}