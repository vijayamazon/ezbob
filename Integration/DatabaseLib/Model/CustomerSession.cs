using System;
using FluentNHibernate.Mapping;

namespace EZBob.DatabaseLib.Model
{
    public class CustomerSession
    {
        public virtual int Id { get; set; }
        public virtual int CustomerId { get; set; }
        public virtual DateTime StartSession { get; set; }
        public virtual string Ip { get; set; }
        public virtual bool IsPasswdOk { get; set; }
        public virtual string ErrorMessage { get; set; }
    }

    public sealed class CustomerSessionMap : ClassMap<CustomerSession>
    {
        public CustomerSessionMap()
        {
            Table("CustomerSession");
            Id(x => x.Id);
            Map(x => x.CustomerId);
            Map(x => x.StartSession);
            Map(x => x.Ip).Length(50);
            Map(x => x.IsPasswdOk);
            Map(x => x.ErrorMessage).Length(50);
        }
    }
}
