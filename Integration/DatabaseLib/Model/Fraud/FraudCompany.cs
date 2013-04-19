using FluentNHibernate.Mapping;

namespace EZBob.DatabaseLib.Model.Fraud
{
    public class FraudCompany
    {
        public virtual int Id { get; set; }
        public virtual string CompanyName { get; set; }
        public virtual string RegistrationNumber { get; set; }
        public virtual FraudUser FraudUser { get; set; }
    }
    public sealed class FraudCompanyMap : ClassMap<FraudCompany>
    {
        public FraudCompanyMap()
        {
            Id(x => x.Id).GeneratedBy.HiLo("100");
            Map(x => x.CompanyName).Length(200);
            Map(x => x.RegistrationNumber).Length(50);
            References(x => x.FraudUser, "FraudUserId");
        }
    }
}
