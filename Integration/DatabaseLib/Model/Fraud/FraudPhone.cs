using FluentNHibernate.Mapping;

namespace EZBob.DatabaseLib.Model.Fraud
{
    public class FraudPhone
    {
        public virtual int Id { get; set; }
        public virtual string PhoneNumber { get; set; }
        public virtual FraudUser FraudUser { get; set; }
    }

    public sealed class FraudPhoneMap : ClassMap<FraudPhone>
    {
        public FraudPhoneMap()
        {
            Id(x => x.Id);
            Map(x => x.PhoneNumber).Length(50);
            References(x => x.FraudUser, "FraudUserId");
        }
    }
}
