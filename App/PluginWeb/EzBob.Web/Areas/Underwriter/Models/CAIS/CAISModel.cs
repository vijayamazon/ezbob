using System.Collections.Generic;
using System.Globalization;
using EZBob.DatabaseLib.Model.Database;
using System.Linq;
using EzBob.Web.Code;

namespace EzBob.Web.Areas.Underwriter.Models.CAIS
{
    public class CaisModel
    {
        public virtual string Date { get; set; }
        public virtual string FileName { get; set; }
        public virtual string Type { get; set; }
        public virtual string OfItems { get; set; }
        public virtual string GoodUsers { get; set; }
        public virtual string CaisUploadStatus { get; set; }
        public virtual string FilePath { get; set; }

        public static List<CaisModel> FromModel(IEnumerable<CaisReportsHistory>  reportsHistory)
        {
            var model = reportsHistory.OrderByDescending(x => x.Date);
            return model
                .Select(x => new CaisModel
                    {
                        CaisUploadStatus = x.UploadStatus.ToString(),
                        Date = FormattingUtils.FormatDateTimeToString(x.Date),
                        GoodUsers = x.GoodUsers.ToString(),
                        FileName = x.FileName,
                        FilePath = x.FilePath,
                        Type = x.Type.ToString(),
                        OfItems = x.OfItems.ToString()
                    }).ToList();
        }
    }
}