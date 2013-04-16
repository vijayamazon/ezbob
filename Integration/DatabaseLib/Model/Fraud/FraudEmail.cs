using FluentNHibernate.Mapping;

namespace EZBob.DatabaseLib.Model.Fraud
{
    public class FraudEmail
    {
        public virtual int Id { get; set; }
        public virtual string Email { get; set; }
        public virtual FraudUser FraudUser { get; set; }
    }

    public sealed class FraudEmailMap : ClassMap<FraudEmail>
    {
        public FraudEmailMap()
        {
            Id(x => x.Id);
            Map(x => x.Email).Length(250);
            References(x => x.FraudUser, "FraudUserId");
        }
    }
}
