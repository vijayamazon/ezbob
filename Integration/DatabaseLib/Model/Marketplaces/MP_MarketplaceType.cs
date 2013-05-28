using FluentNHibernate.Mapping;

namespace EZBob.DatabaseLib.Model.Database {
    
    public class MP_MarketplaceType 
	{
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual System.Guid InternalId { get; set; }
        public virtual string Description { get; set; }
		public virtual bool Active { get; set; }
        public virtual bool IsPaymentAccount { get { return false; } }
	}

    public class MP_MarketplaceTypeMap : ClassMap<MP_MarketplaceType>
    {

        public MP_MarketplaceTypeMap()
        {
            Table("MP_MarketplaceType");
            Not.LazyLoad();
            Cache.ReadWrite().Region("Longest").ReadWrite();
            Id(x => x.Id).GeneratedBy.Identity().Column("Id");
            Map(x => x.Name).Column("Name").Not.Nullable().Length(255);
            Map(x => x.InternalId).Not.Nullable();
            Map(x => x.Description);
            Map(x => x.Active);
            DiscriminateSubClassesOnColumn("Name");
        }
    }

    public class AmazonMarketPlaceType : MP_MarketplaceType
    {
    }

    public class AmazonMarketPlaceTypeMap : SubclassMap<AmazonMarketPlaceType>
    {
        public AmazonMarketPlaceTypeMap()
        {
            DiscriminatorValue("Amazon");
        }
    }

    public class eBayMarketPlaceType : MP_MarketplaceType
    {

    }

    public class eBayMarketPlaceTypeMap : SubclassMap<eBayMarketPlaceType>
    {
        public eBayMarketPlaceTypeMap()
        {
            DiscriminatorValue("eBay");
        }
    }

    public class PayPalMarketPlaceType : MP_MarketplaceType
    {
        public override bool IsPaymentAccount { get { return true; } }
    }

    public class PayPalMarketPlaceTypeMap : SubclassMap<PayPalMarketPlaceType>
    {
        public PayPalMarketPlaceTypeMap()
        {
            DiscriminatorValue("Pay Pal");
        }
    }

    public class EKMMarketPlaceType : MP_MarketplaceType
    {

    }

    public class EKMMarketPlaceTypeMap : SubclassMap<EKMMarketPlaceType>
    {
        public EKMMarketPlaceTypeMap()
        {
            DiscriminatorValue("EKM");
        }
    }


    public class VolusionMarketPlaceType : MP_MarketplaceType
    {

    }

    public class VolusionMarketPlaceTypeMap : SubclassMap<VolusionMarketPlaceType>
    {
        public VolusionMarketPlaceTypeMap()
        {
            DiscriminatorValue("Volusion");
        }
    }

    public class PayPointMarketPlaceType : MP_MarketplaceType
    {
        public override bool IsPaymentAccount { get { return true; } }
    }

    public class PayPointMarketPlaceTypeMap : SubclassMap<PayPointMarketPlaceType>
    {
        public PayPointMarketPlaceTypeMap()
        {
            DiscriminatorValue("PayPoint");
        }
    }

    public class PlayMarketPlaceType : MP_MarketplaceType
    {

    }

    public class PlayMarketPlaceTypeMap : SubclassMap<PlayMarketPlaceType>
    {
        public PlayMarketPlaceTypeMap()
        {
            DiscriminatorValue("Play");
        }
    }

    public class YodleeMarketPlaceType : MP_MarketplaceType
    {
        public override bool IsPaymentAccount { get { return true; } }
    }

    public class YodleeMarketPlaceTypeMap : SubclassMap<YodleeMarketPlaceType>
    {
        public YodleeMarketPlaceTypeMap()
        {
            DiscriminatorValue("Yodlee");
        }
    }
}
