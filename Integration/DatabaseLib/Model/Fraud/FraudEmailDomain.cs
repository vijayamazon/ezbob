using FluentNHibernate.Mapping;

namespace EZBob.DatabaseLib.Model.Fraud
{
    public class FraudEmailDomain
    {
        public virtual int Id { get; set; }
        public virtual string EmailDomain { get; set; }
        public virtual FraudUser FraudUser { get; set; }
    }

    public sealed class FraudEmailDomainMap : ClassMap<FraudEmailDomain>
    {
        public FraudEmailDomainMap()
        {
            Id(x => x.Id);
            Map(x => x.EmailDomain).Length(250);
            References(x => x.FraudUser, "FraudUserId");
        }
    }
}