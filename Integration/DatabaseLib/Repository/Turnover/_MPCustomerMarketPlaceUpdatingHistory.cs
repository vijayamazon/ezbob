
namespace EZBob.DatabaseLib.Repository.Turnover {
	using System.Collections.Generic;
	using ApplicationMng.Repository;
	using FluentNHibernate.Mapping;
	using NHibernate;

	public class _MPCustomerMarketPlaceUpdatingHistory {

		public _MPCustomerMarketPlaceUpdatingHistory() {

			/*this.HmrcAggregations = new HashSet<HmrcAggregation>();
			this.YodleeAggregations = new HashSet<YodleeAggregation>();
			this.AmazonAggregations = new HashSet<AmazonAggregation>();
			this.EbayAggregations = new HashSet<EbayAggregation>();
			this.PayPalAggregations = new HashSet<PayPalAggregation>();*/

			//OnCreated();
		}

		public virtual int Id { get; set; }
		public virtual System.DateTime UpdatingStart { get; set; }
		public virtual System.DateTime? UpdatingEnd { get; set; }
		public virtual string Error { get; set; }
		/*public virtual string GetError() {
			return this.Error;
		}
		public virtual string SetError(string value) {
			return this.Error = value.Trim();
		}*/

		/*public virtual int? UpdatingTimePassInSeconds { get; set; }
		public virtual ISet<AmazonAggregation> AmazonAggregations { get; set; }
		public virtual ISet<EbayAggregation> EbayAggregations { get; set; }
		public virtual ISet<HmrcAggregation> HmrcAggregations { get; set; }
		public virtual ISet<PayPalAggregation> PayPalAggregations { get; set; }
		public virtual ISet<YodleeAggregation> YodleeAggregations { get; set; }*/
		//public virtual MPCustomerMarketPlace CustomerMarketPlace { get; set; }

	}

	public class MPCustomerMarketPlaceUpdatingHistoryRepository : NHibernateRepositoryBase<_MPCustomerMarketPlaceUpdatingHistory> {
	
		public MPCustomerMarketPlaceUpdatingHistoryRepository(ISession session)
			: base(session)
		{
		}

	}

	public class MPCustomerMarketPlaceUpdatingHistoryMap : ClassMap<_MPCustomerMarketPlaceUpdatingHistory> {

		public MPCustomerMarketPlaceUpdatingHistoryMap() {

			Table("MP_CustomerMarketPlaceUpdatingHistory");

			Id(x => x.Id).Column("Id");

			Map(x => x.UpdatingStart);
			Map(x => x.UpdatingEnd);
			//Map(x => x.UpdatingTimePassInSeconds);
			Map(x => x.Error).Nullable();
	
			/*HasMany(x => x.HmrcAggregations).LazyLoad().Inverse().Cascade.All();
			HasMany(x => x.YodleeAggregations).LazyLoad().Inverse().Cascade.All();
			HasMany(x => x.AmazonAggregations).LazyLoad().Inverse().Cascade.All();
			HasMany(x => x.EbayAggregations).LazyLoad().Inverse().Cascade.All();
			HasMany(x => x.PayPalAggregations).LazyLoad().Inverse().Cascade.All();*/

			//References(x => x.CustomerMarketPlace, "CustomerMarketPlaceId").Cascade.None();

		//HasMany(x => x.AmazonAggregations).KeyColumn("CustomerMarketPlaceUpdatingHistoryID").Inverse().Cascade.All();

		}
	}


}
