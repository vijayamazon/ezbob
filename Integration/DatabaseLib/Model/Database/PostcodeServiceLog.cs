using System;
using FluentNHibernate.Mapping;

namespace EZBob.DatabaseLib.Model.Database
{
    public class PostcodeServiceLog
    {
        public virtual long Id { get; set; }
        public virtual Customer Customer { get; set; }
        public virtual DateTime? InsertDate { get; set; }
        public virtual string RequestType { get; set; }
        public virtual string RequestData { get; set; }
        public virtual string ResponseData { get; set; }
        public virtual string Status { get; set; }
        public virtual string ErrorMessage { get; set; }
    }

    public class PostcodeServiceLogMap : ClassMap<PostcodeServiceLog>
    {
        public PostcodeServiceLogMap()
        {
            Table("PostcodeServiceLog");
            Id(x => x.Id);
            References(x => x.Customer, "CustomerId");
            Map(x => x.InsertDate);
            Map(x => x.RequestType).Length(200);
            Map(x => x.Status).Length(200);
            Map(x => x.RequestData).CustomType("StringClob");
            Map(x => x.ResponseData).CustomType("StringClob");
            Map(x => x.ErrorMessage).CustomType("StringClob");
        }
    }
}
