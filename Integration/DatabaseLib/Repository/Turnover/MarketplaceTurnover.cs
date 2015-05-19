namespace EZBob.DatabaseLib.Repository.Turnover {
	using System;
	using System.Linq;
	using ApplicationMng.Repository;
	using EZBob.DatabaseLib.Model.Database;
	using FluentNHibernate.Mapping;
	using NHibernate;
	using NHibernate.Type;

	public class MarketplaceTurnover {
		public virtual long AggID { get; set; }
		public virtual DateTime TheMonth { get; set; }
		public virtual bool IsActive { get; set; }
		public virtual decimal Turnover { get; set; }
		public virtual DateTime UpdatingEnd { get; set; }

		public virtual MP_CustomerMarketplaceUpdatingHistory CustomerMarketPlaceUpdatingHistory { get; set; }

		public virtual MP_CustomerMarketPlace CustomerMarketPlace { get; set; }

		public virtual Customer Customer { get; set; }

		public override bool Equals(Object obj) {
			var t = obj as MarketplaceTurnover;

			if (t == null)
				return false;

			return (
				CustomerMarketPlaceUpdatingHistory.Id == t.CustomerMarketPlaceUpdatingHistory.Id &&
				TheMonth == t.TheMonth
			);
		} // Equals

		public override int GetHashCode() {
			return (CustomerMarketPlaceUpdatingHistory.Id + "|" + TheMonth).GetHashCode();
		} // GetHashCode

		public override string ToString() {
			return string.Join("|",
				AggID,
				CustomerMarketPlaceUpdatingHistory.Id,
				Customer.Id,
				CustomerMarketPlace.Id,
				TheMonth,
				Turnover,
				UpdatingEnd
			);
		} // ToString
	} // class MarketplaceTurnover

	public class MarketplaceTurnoverRepository : NHibernateRepositoryBase<MarketplaceTurnover> {
		public MarketplaceTurnoverRepository(ISession session) : base(session) { }

		public IQueryable<MarketplaceTurnover> GetByCustomerId(int customerID) {
			return GetAll().Where(x => x.Customer.Id == customerID);
		} // GetByCustomerId

		public IQueryable<MarketplaceTurnover> GetByCustomerAndDate(int customerID, DateTime calculationDate) {
			return GetAll().Where(x =>
				x.Customer.Id == customerID &&
				x.UpdatingEnd < calculationDate &&
				x.CustomerMarketPlace.Disabled == false
			);
		} // GetByCustomerAndDate
	} // class MarketplaceTurnoverRepository

	public sealed class MarketplaceTurnoverMap : ClassMap<MarketplaceTurnover> {
		public MarketplaceTurnoverMap() {
			Table("MarketplaceTurnover");
			ReadOnly();

			Map(x => x.AggID);
			Map(x => x.Turnover).Precision(18).Scale(2);
			Map(x => x.TheMonth);
			Map(x => x.IsActive);

			Map(x => x.UpdatingEnd).CustomType<UtcDateTimeType>();

			References(x => x.CustomerMarketPlaceUpdatingHistory, "CustomerMarketPlaceUpdatingHistoryID").Cascade.None();

			References(x => x.CustomerMarketPlace, "CustomerMarketPlaceId").Cascade.None();
			References(x => x.Customer, "CustomerId").Cascade.None();

			CompositeId()
				.KeyReference(x => x.CustomerMarketPlaceUpdatingHistory, "CustomerMarketPlaceUpdatingHistoryID")
				.KeyProperty(x => x.TheMonth)
				.KeyProperty(x => x.AggID);
		} // constructor
	} // class MarketplaceTurnoverMap
} // namespace
