using System;
using System.Collections.Generic;
using EzBob.Web.Areas.Underwriter.Controllers.Reports;

namespace EzBob.Web.Areas.Underwriter.Models.Reports
{
    public class ReportTableColumn
    {
        public string Caption { get; set; }
        public string Width { get; set; }
        public string FieldName { get; set; }
        public ReportTableCreator.DataType DataType { get; set; }
        public string Format { get; set; }
        public List<ReportTableColumn> Childs { get; set; }
        public IImageProvider ImageProvider { get; set; }

        public Func<object, object> FieldValue { get; set; }
    }
}