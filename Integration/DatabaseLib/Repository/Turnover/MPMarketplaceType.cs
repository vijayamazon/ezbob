namespace EZBob.DatabaseLib.Repository.Turnover {
	using System;
	using System.Collections.Generic;
	using ApplicationMng.Repository;
	using FluentNHibernate.Mapping;
	using NHibernate;

	public class MPMarketplaceType {

		public MPMarketplaceType() {
			//this.IsPaymentAccount = false;
			//this.CustomerMarketPlaces = new HashSet<MPCustomerMarketPlace>();
		}

		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
		public virtual Guid InternalId { get; set; }
		public virtual string Description { get; set; }
		public virtual bool? ActiveWizardOnline { get; set; }
		public virtual bool? ActiveDashboardOnline { get; set; }
		public virtual bool? ActiveWizardOffline { get; set; }
		public virtual bool? ActiveDashboardOffline { get; set; }
		public virtual int? PriorityOnline { get; set; }
		public virtual int? PriorityOffline { get; set; }
		public virtual int? GroupId { get; set; }
		public virtual string Ribbon { get; set; }
		public virtual bool? MandatoryOnline { get; set; }
		public virtual bool? MandatoryOffline { get; set; }
		public virtual bool IsPaymentAccount { get; set; }

	//	public virtual ISet<MPCustomerMarketPlace> CustomerMarketPlaces { get; set; }
	}

	public class MPMarketplaceTypeRepository : NHibernateRepositoryBase<MPMarketplaceType> {
		public MPMarketplaceTypeRepository(ISession session) : base(session) { }

	}

	public class MPMarketplaceTypeMap : ClassMap<MPMarketplaceType> {

		public MPMarketplaceTypeMap() {

			Table("MP_MarketplaceType");
			
			Id(x => x.Id).Column("Id");

			Map(x => x.Name);
			Map(x => x.InternalId);
			Map(x => x.Description);
			Map(x => x.ActiveWizardOnline);
			Map(x => x.ActiveDashboardOnline);
			Map(x => x.ActiveWizardOffline);
			Map(x => x.ActiveDashboardOffline);
			Map(x => x.PriorityOnline);
			Map(x => x.PriorityOffline);
			Map(x => x.GroupId);
			Map(x => x.Ribbon);
			Map(x => x.MandatoryOnline);
			Map(x => x.MandatoryOffline);
			Map(x => x.IsPaymentAccount);
		//	Map(x => x.CustomerMarketPlaces);

		//	HasMany(x => x.CustomerMarketPlaces).KeyColumn("MarketPlaceId").LazyLoad().Inverse().Cascade.All();

		}
	}
}