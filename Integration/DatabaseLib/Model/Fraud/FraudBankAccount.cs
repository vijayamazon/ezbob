using FluentNHibernate.Mapping;

namespace EZBob.DatabaseLib.Model.Fraud
{
    public class FraudBankAccount
    {
        public virtual int Id { get; set; }
        public virtual string BankAccount { get; set; }
        public virtual string SortCode { get; set; }
        public virtual FraudUser FraudUser { get; set; }
    }

    public sealed class FraudBankAccountMap : ClassMap<FraudBankAccount>
    {
        public FraudBankAccountMap()
        {
            Id(x => x.Id);
            Map(x => x.BankAccount).Length(50);
            Map(x => x.SortCode).Length(50);
            References(x => x.FraudUser, "FraudUserId");
        }
    }
}
