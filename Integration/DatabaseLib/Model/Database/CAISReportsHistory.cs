using System;

namespace EZBob.DatabaseLib.Model.Database
{
    public enum CaisType
    {
        Entrepreneur = 1,
        Business = 2
    }

    public enum CaisUploadStatus
    {
        Uploaded = 1,
        Generated = 2,
        UploadError = 3
    }

    public class CaisReportsHistory
    {
        public virtual int Id { get; set; }
        public virtual DateTime? Date { get; set; }
        public virtual string FileName { get; set; }
        public virtual CaisType? Type { get; set; }
        public virtual int? OfItems { get; set; }
        public virtual int? GoodUsers { get; set; }
        public virtual CaisUploadStatus? UploadStatus { get; set; }
        public virtual string FilePath { get; set; }
    }
}
