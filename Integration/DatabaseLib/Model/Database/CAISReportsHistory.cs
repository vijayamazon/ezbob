using System;

namespace EZBob.DatabaseLib.Model.Database
{
    public enum CaisReportType
    {
        Entrepreneur,
        Business
    }

    public enum CaisReportUploadStatus
    {
        Uploaded,
        Generated,
        Error
    }

    public class CaisReportsHistory
    {
        public virtual int Id { get; set; }
        public virtual DateTime Date { get; set; }
        public virtual string FileName { get; set; }
        public virtual CaisReportType Type { get; set; }
        public virtual int OfItems { get; set; }
        public virtual int GoodUsers { get; set; }
        public virtual CaisReportUploadStatus UploadStatus { get; set; }
        public virtual string FilePath { get; set; }
    }
}
