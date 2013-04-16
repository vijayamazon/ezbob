using FluentNHibernate.Mapping;

namespace EZBob.DatabaseLib.Model.Fraud
{
    public class FraudBankAccount
    {
        public virtual int Id { get; set; }
        public virtual string Town { get; set; }
        public virtual string County { get; set; }
        public virtual FraudUser FraudUser { get; set; }
    }

    public sealed class FraudBankAccountMap : ClassMap<FraudBankAccount>
    {
        
    }
}
