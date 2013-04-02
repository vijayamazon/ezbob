using System;
using ApplicationMng.Model;
using FluentNHibernate.Mapping;

namespace EZBob.DatabaseLib.Model.Database
{

    public class MP_AlertDocument
    {
        public virtual int Id { get; set; }
        public virtual string DocName { get; set; }
        public virtual DateTime? UploadDate { get; set; }
        public virtual Customer Customer { get; set; }
        public virtual User Employee { get; set; }
        public virtual string Description { get; set; }
        public virtual byte[] BinaryBody { get; set; }
    }

    public sealed class MP_AlertDocumentMap : ClassMap<MP_AlertDocument>
    {
        public MP_AlertDocumentMap()
        {
            Table("MP_AlertDocument");
            Id( x => x.Id );
            References(x => x.Employee, "UserId");
            References(x => x.Customer, "CustomerId");
            Map(x => x.DocName);
            Map(x => x.UploadDate);
            Map(x => x.Description).CustomType("StringClob").LazyLoad();
            Map(x => x.BinaryBody).Length(int.MaxValue).LazyLoad();
        }
    }
}

