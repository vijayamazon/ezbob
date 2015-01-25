namespace EZBob.DatabaseLib.Repository.Turnover {
	using System;
	using System.Collections.Generic;
	using ApplicationMng.Repository;
	using FluentNHibernate.Mapping;
	using NHibernate;

	public class _MPCustomerMarketPlace 
	{
		public _MPCustomerMarketPlace() {
			this.TokenExpired = 0;
			//this.MPCustomerMarketPlaceUpdatingHistories = new HashSet<MPCustomerMarketPlaceUpdatingHistory>();
		}

		public virtual int Id {get;set;}
		public virtual int CustomerId {get;set;}
		public virtual byte[] SecurityData {get;set;}
		public virtual string DisplayName {get;set;}
		public virtual DateTime? Created {get;set;}
		public virtual DateTime? Updated {get;set;}
		public virtual DateTime? UpdatingStart {get;set;}
		public virtual DateTime? UpdatingEnd {get;set;}
		public virtual string Warning {get;set;}
		public virtual string UpdateError {get;set;}
		public virtual int? UpdatingTimePassInSeconds {get; set;}
		public virtual int TokenExpired {get;set;}
		public virtual DateTime? OriginationDate {get;set;}
		public virtual bool? Disabled {get;set;}
		public virtual int? AmazonMarketPlaceId {get;set;}
		public virtual DateTime? LastTransactionDate {get;set;}
		//protected internal virtual MPMarketplaceType MPMarketplaceType {get;set;}

		//public virtual ISet<MPCustomerMarketPlaceUpdatingHistory> MPCustomerMarketPlaceUpdatingHistories {get;set;}

		
	}

	public class MPCustomerMarketPlaceMap : ClassMap<_MPCustomerMarketPlace> {

		public MPCustomerMarketPlaceMap() {

			Table("MP_CustomerMarketPlace");


			Id(x => x.Id).Column("Id");

			Map(x => x.CustomerId);
			Map(x => x.SecurityData);
			Map(x => x.DisplayName);
			Map(x => x.Created);
			Map(x => x.Updated);
			Map(x => x.UpdatingStart);
			Map(x => x.UpdatingEnd);
			Map(x => x.UpdatingTimePassInSeconds);
			Map(x => x.UpdateError);
			Map(x => x.Warning);
			Map(x => x.TokenExpired);
			Map(x => x.OriginationDate);
			Map(x => x.Disabled);
			Map(x => x.AmazonMarketPlaceId);
			Map(x => x.LastTransactionDate);
			//	Map(x => x.MPMarketplaceType);

			//	References(x => x.MPMarketplaceType, "MarketPlaceId").Cascade.None();

			//	HasMany(x => x.MPCustomerMarketPlaceUpdatingHistories).LazyLoad().Inverse().Cascade.All();

		}
	}

	public class MPCustomerMarketPlaceRepository : NHibernateRepositoryBase<_MPCustomerMarketPlace> {
		public MPCustomerMarketPlaceRepository(ISession session)
			: base(session) {

			Console.WriteLine("=======>HERE");

			


		}




	}

	
	
}
