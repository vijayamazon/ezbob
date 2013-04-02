using System;
using FluentNHibernate.Mapping;

namespace EZBob.DatabaseLib.Model.Database
{
    public class ExportResult
    {
        public virtual int Id { get; set; }
        public virtual string FileName { get; set; }
        public virtual byte[] BinaryBody { get; set; }
        public virtual int FileType { get; set; }
        public virtual DateTime? CreationDate { get; set; }
        public virtual int SourceTemplateId { get; set; }
        public virtual int ApplicationId { get; set; }
        public virtual string Status { get; set; }
        public virtual string StatusMode { get; set; }
        public virtual string NodeName { get; set; }
        public virtual int SignedDocumentId { get; set; }
    }
    public class ExportResultMap : ClassMap<ExportResult>
    {
        public ExportResultMap()
        {
            Table("Export_Results");
            Id(x => x.Id).GeneratedBy.Assigned().Column("Id");
            Map(x => x.FileName);
            Map(x => x.BinaryBody).LazyLoad();
            Map(x => x.FileType);
            Map(x => x.CreationDate);
            Map(x => x.SourceTemplateId);
            Map(x => x.ApplicationId);
            Map(x => x.Status);
            Map(x => x.StatusMode);
            Map(x => x.NodeName);
            Map(x => x.SignedDocumentId);
        }
    }
}
