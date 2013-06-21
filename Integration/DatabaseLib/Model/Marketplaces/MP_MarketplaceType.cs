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
        public virtual int UWPriority { get { return 0; } }
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
            DiscriminateSubClassesOnColumn("").Formula(
				"CASE Name " +
				Integration.ChannelGrabberConfig.Configuration.Instance.GetMarketplaceDiscriminator() +
				" ELSE Name END"
			);
        }
    }

    public class AmazonMarketPlaceType : MP_MarketplaceType
    {
        public override int UWPriority { get { return 2; } }
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
        public override int UWPriority { get { return 1; } }
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
        public override int UWPriority { get { return 1; } }
    }

    public class PayPalMarketPlaceTypeMap : SubclassMap<PayPalMarketPlaceType>
    {
        public PayPalMarketPlaceTypeMap()
        {
            DiscriminatorValue("Pay Pal");
        }
	}

    public class ShopifyType : MP_MarketplaceType
    {
        public override bool IsPaymentAccount { get { return true; } }
        public override int UWPriority { get { return 1; } }
    }

    public class ShopifyTypeMap : SubclassMap<ShopifyType>
    {
        public ShopifyTypeMap()
        {
            DiscriminatorValue("Shopify.com");
        }
	}

	public class EKMMarketPlaceType : MP_MarketplaceType
	{
		public override int UWPriority { get { return 3; } }
	}

	public class EKMMarketPlaceTypeMap : SubclassMap<EKMMarketPlaceType>
	{
		public EKMMarketPlaceTypeMap()
		{
			DiscriminatorValue("EKM");
		}
	}

	public class FreeAgentMarketPlaceType : MP_MarketplaceType
	{
		public override bool IsPaymentAccount { get { return true; } }
		public override int UWPriority { get { return 4; } }
	}

	public class FreeAgentMarketPlaceTypeMap : SubclassMap<FreeAgentMarketPlaceType>
	{
		public FreeAgentMarketPlaceTypeMap()
		{
			DiscriminatorValue("FreeAgent");
		}
	}


    public class PayPointMarketPlaceType : MP_MarketplaceType
    {
        public override bool IsPaymentAccount { get { return true; } }
        public override int UWPriority { get { return 2; } }
    }

    public class PayPointMarketPlaceTypeMap : SubclassMap<PayPointMarketPlaceType>
    {
        public PayPointMarketPlaceTypeMap()
        {
            DiscriminatorValue("PayPoint");
        }
    }

    public class YodleeMarketPlaceType : MP_MarketplaceType
    {
        public override bool IsPaymentAccount { get { return true; } }
        public override int UWPriority { get { return 3; } }
    }

    public class YodleeMarketPlaceTypeMap : SubclassMap<YodleeMarketPlaceType>
    {
        public YodleeMarketPlaceTypeMap()
        {
            DiscriminatorValue("Yodlee");
        }
    }

	public class ChannelGrabberMarketPlaceType : MP_MarketplaceType {
		public override int UWPriority { get { return 4; } } // constructor
	} // class ChannelGrabberMarketPlaceType

	public class ChannelGrabberMarketPlaceTypeMap : SubclassMap<ChannelGrabberMarketPlaceType> {
		public ChannelGrabberMarketPlaceTypeMap() {
			DiscriminatorValue("ChannelGrabber");
		} // constructor
	} // class ChannelGrabberMarketPlaceTypeMap
} // namespace EZBob.DatabaseLib.Model.Database
