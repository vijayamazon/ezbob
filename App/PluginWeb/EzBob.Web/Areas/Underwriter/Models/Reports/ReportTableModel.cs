using System.Collections.Generic;

namespace EzBob.Web.Areas.Underwriter.Models.Reports
{
    public class ReportTableModel<T>
    {
        public List<ReportTableColumn> Columns { get; set; }
        public List<T> Data { get; set; }
        public T Total { get; set; }
    }
}