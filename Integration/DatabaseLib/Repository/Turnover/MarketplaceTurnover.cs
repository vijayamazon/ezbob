namespace EZBob.DatabaseLib.Repository.Turnover {
	using System;
	using System.Linq;
	using ApplicationMng.Repository;
	using FluentNHibernate.Mapping;
	using NHibernate;
	using NHibernate.Type;

	public class MarketplaceTurnover {
		public virtual long AggID { get; set; }
		public virtual DateTime TheMonth { get; set; }
		public virtual bool IsActive { get; set; }
		public virtual decimal Turnover { get; set; }
		public virtual DateTime UpdatingEnd { get; set; }
		public virtual int CustomerMarketPlaceUpdatingHistoryID { get; set; }
		public virtual int CustomerMarketPlaceID { get; set; }
		public virtual int CustomerID { get; set; }
		public virtual bool IsMarketplaceDisabled { get; set; }
		public virtual Guid MarketplaceInternalID { get; set; }
		public virtual bool IsPaymentAccount { get; set; }

		public override bool Equals(Object obj) {
			var t = obj as MarketplaceTurnover;

			if (t == null)
				return false;

			return (
				CustomerMarketPlaceUpdatingHistoryID == t.CustomerMarketPlaceUpdatingHistoryID &&
				TheMonth == t.TheMonth
			);
		} // Equals

		public override int GetHashCode() {
			return (CustomerMarketPlaceUpdatingHistoryID + "|" + TheMonth).GetHashCode();
		} // GetHashCode

		public override string ToString() {
			return string.Join("|",
				AggID,
				CustomerMarketPlaceUpdatingHistoryID,
				CustomerID,
				CustomerMarketPlaceID,
				TheMonth,
				Turnover
			);
		} // ToString
	} // class MarketplaceTurnover

	public class MarketplaceTurnoverRepository : NHibernateRepositoryBase<MarketplaceTurnover> {
		public MarketplaceTurnoverRepository(ISession session) : base(session) { }

		public IQueryable<MarketplaceTurnover> GetByCustomerId(int customerID) {
			return GetAll().Where(x => x.CustomerID == customerID);
		} // GetByCustomerId

		public IQueryable<MarketplaceTurnover> GetByCustomerAndDate(int customerID, DateTime calculationDate) {
			return GetAll().Where(x =>
				x.CustomerID == customerID &&
				x.UpdatingEnd < calculationDate &&
				!x.IsMarketplaceDisabled
			);
		} // GetByCustomerAndDate
	} // class MarketplaceTurnoverRepository

	public sealed class MarketplaceTurnoverMap : ClassMap<MarketplaceTurnover> {
		public MarketplaceTurnoverMap() {
			Table("MarketplaceTurnover");
			ReadOnly();

			Map(x => x.AggID);
			Map(x => x.Turnover).Precision(18).Scale(2);
			Map(x => x.TheMonth).CustomType<UtcDateTimeType>();
			Map(x => x.IsActive);
			Map(x => x.UpdatingEnd).CustomType<UtcDateTimeType>();
			Map(x => x.CustomerMarketPlaceUpdatingHistoryID);
			Map(x => x.CustomerMarketPlaceID, "CustomerMarketPlaceId");
			Map(x => x.CustomerID, "CustomerId");
			Map(x => x.IsMarketplaceDisabled);
			Map(x => x.MarketplaceInternalID);
			Map(x => x.IsPaymentAccount);

			CompositeId()
				.KeyProperty(x => x.CustomerMarketPlaceUpdatingHistoryID)
				.KeyProperty(x => x.TheMonth)
				.KeyProperty(x => x.AggID);
		} // constructor
	} // class MarketplaceTurnoverMap
} // namespace
