namespace EZBob.DatabaseLib.Model.Database {
	using System;
	using FluentNHibernate.Mapping;
	using NHibernate.Type;

	#region class QuickOffer

	public class QuickOffer {
		#region public

		public virtual int ID { get; set; }
		public virtual decimal Amount { get; set; }
		public virtual DateTime CreationDate { get; set; }
		public virtual DateTime ExpirationDate { get; set; }
		public virtual int Aml { get; set; }
		public virtual int BusinessScore { get; set; }
		public virtual DateTime IncorporationDate { get; set; }
		public virtual decimal TangibleEquity { get; set; }
		public virtual decimal TotalCurrentAssets { get; set; }

		#endregion public
	} // class QuickOffer

	#endregion class QuickOffer

	#region class QuickOfferMap

	public class QuickOfferMap : ClassMap<QuickOffer> {
		public QuickOfferMap() {
			Table("QuickOffer");
			Cache.ReadWrite().Region("LongTerm").ReadWrite();

			Id(x => x.ID, "QuickOfferID");
			Map(x => x.Amount);
			Map(x => x.CreationDate).CustomType<UtcDateTimeType>();
			Map(x => x.ExpirationDate).CustomType<UtcDateTimeType>();
			Map(x => x.Aml);
			Map(x => x.BusinessScore);
			Map(x => x.IncorporationDate).CustomType<UtcDateTimeType>();
			Map(x => x.TangibleEquity);
			Map(x => x.TotalCurrentAssets);
		} // constructor
	} // class QuickOfferMap

	#endregion class QuickOfferMap
} // namespace EZBob.DatabaseLib.Model.Database
