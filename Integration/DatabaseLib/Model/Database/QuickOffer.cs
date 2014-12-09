namespace EZBob.DatabaseLib.Model.Database {
	using System;
	using FluentNHibernate.Mapping;
	using NHibernate.Type;

	public class QuickOffer {

		public virtual int ID { get; set; }
		public virtual decimal Amount { get; set; }
		public virtual DateTime CreationDate { get; set; }
		public virtual DateTime ExpirationDate { get; set; }
		public virtual int Aml { get; set; }
		public virtual int BusinessScore { get; set; }
		public virtual DateTime IncorporationDate { get; set; }
		public virtual decimal TangibleEquity { get; set; }
		public virtual decimal TotalCurrentAssets { get; set; }
		public virtual int ImmediateTerm { get; set; }
		public virtual decimal ImmediateInterestRate { get; set; }
		public virtual decimal ImmediateSetupFee { get; set; }
		public virtual decimal PotentialAmount { get; set; }
		public virtual int PotentialTerm { get; set; }
		public virtual decimal PotentialInterestRate { get; set; }
		public virtual decimal PotentialSetupFee { get; set; }

	} // class QuickOffer

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

			Map(x => x.ImmediateTerm);
			Map(x => x.ImmediateInterestRate);
			Map(x => x.ImmediateSetupFee);
			Map(x => x.PotentialAmount);
			Map(x => x.PotentialTerm);
			Map(x => x.PotentialInterestRate);
			Map(x => x.PotentialSetupFee);
		} // constructor
	} // class QuickOfferMap

} // namespace EZBob.DatabaseLib.Model.Database
