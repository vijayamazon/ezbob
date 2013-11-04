using FluentNHibernate.Mapping;
using Integration.ChannelGrabberConfig;

namespace EZBob.DatabaseLib.Model.Database {
	using Marketplaces;

	public class MP_MarketplaceType 
	{
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual System.Guid InternalId { get; set; }
        public virtual string Description { get; set; }
		public virtual bool Active { get; set; }
        public virtual bool IsPaymentAccount { get { return false; } }
        public virtual int UWPriority { get { return 0; } }
		public virtual bool IsOffline { get; set; }

		public virtual bool ActiveWizardOnline { get; set; }
		public virtual bool ActiveDashboardOnline { get; set; }
		public virtual bool ActiveWizardOffline { get; set; }
		public virtual bool ActiveDashboardOffline { get; set; }
		public virtual int? PriorityOnline { get; set; }
		public virtual int? PriorityOffline { get; set; }
		public virtual MP_MarketplaceGroup Group { get; set; }
		public virtual string Ribbon { get; set; }
		public virtual bool MandatoryOnline { get; set; }
		public virtual bool MandatoryOffline { get; set; }
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
            Map(x => x.IsOffline);
			Map(x => x.ActiveDashboardOffline);
			Map(x => x.ActiveDashboardOnline);
			Map(x => x.ActiveWizardOffline);
			Map(x => x.ActiveWizardOnline);
			Map(x => x.PriorityOffline).Nullable();
			Map(x => x.PriorityOnline).Nullable();
			Map(x => x.Ribbon).Length(50).Nullable();
			Map(x => x.MandatoryOnline);
			Map(x => x.MandatoryOffline);
			References(x => x.Group, "GroupId");

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

	public class SageMarketPlaceType : MP_MarketplaceType
	{
		public override bool IsPaymentAccount { get { return true; } }
		public override int UWPriority { get { return 14; } }
	}

	public class SageMarketPlaceTypeMap : SubclassMap<SageMarketPlaceType>
	{
		public SageMarketPlaceTypeMap()
		{
			DiscriminatorValue("Sage");
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
		public override int UWPriority { get { return 4; } } // UWPriority

		public override bool IsPaymentAccount { get {
			VendorInfo vi = Integration.ChannelGrabberConfig.Configuration.Instance.GetVendorInfo(this.Name);
			return vi.HasExpenses;
		} } // IsPaymentAccount
	} // class ChannelGrabberMarketPlaceType

	public class ChannelGrabberMarketPlaceTypeMap : SubclassMap<ChannelGrabberMarketPlaceType> {
		public ChannelGrabberMarketPlaceTypeMap() {
			DiscriminatorValue("ChannelGrabber");
		} // constructor
	} // class ChannelGrabberMarketPlaceTypeMap
} // namespace EZBob.DatabaseLib.Model.Database
