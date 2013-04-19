using EZBob.DatabaseLib.Model.Database;
using FluentNHibernate.Mapping;

namespace EZBob.DatabaseLib.Model.Fraud
{
    public class FraudShop
    {
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual MP_MarketplaceType Type { get; set; }
        public virtual FraudUser FraudUser { get; set; }
    }

    public sealed class FraudShopMap : ClassMap<FraudShop>
    {
        public FraudShopMap()
        {
            Id(x => x.Id).GeneratedBy.HiLo("100");
            Map(x => x.Name).Length(200);
            References(x => x.Type, "`Type`");
            References(x => x.FraudUser, "FraudUserId");
        }
    }
}
