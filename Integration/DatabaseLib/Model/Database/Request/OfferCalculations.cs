namespace EZBob.DatabaseLib.Model.Database.Request
{
    using System;
    using System.Linq;
    using ApplicationMng.Repository;
    using FluentNHibernate.Mapping;
    using NHibernate;
    using NHibernate.Type;

    public class OfferCalculations
	{
		public virtual int Id { get; set; }
		public virtual int CustomerId { get; set; }
        public virtual DateTime CalculationTime { get; set; }
        public virtual bool IsActive { get; set; }
        public virtual int Amount { get; set; }
        public virtual Medal MedalClassification { get; set; }
        public virtual string ScenarioName { get; set; }
        public virtual int Period { get; set; }
        public virtual bool IsEu { get; set; }
        public virtual decimal InterestRate { get; set; }

        public virtual decimal SetupFee { get; set; }
        public virtual string Error { get; set; }
        public virtual int LoanTypeId { get; set; }

	}

	public class OfferCalculationsMap : ClassMap<OfferCalculations>
	{
		public OfferCalculationsMap() {
		    Table("OfferCalculations");
			Id(x => x.Id);
			Map(x => x.CustomerId);
            Map(x => x.CalculationTime).CustomType<UtcDateTimeType>();
            Map(x => x.Amount);
            Map(x => x.MedalClassification).CustomType<MedalType>(); ;
            Map(x => x.ScenarioName);
            Map(x => x.Period);
            Map(x => x.IsEu);
            Map(x => x.IsActive);
            Map(x => x.InterestRate);
            Map(x => x.SetupFee);
            Map(x => x.Error);
            Map(x => x.LoanTypeId);
		}
	}

	public interface IOfferCalculationsRepository : IRepository<OfferCalculations> {
	    OfferCalculations GetActiveForCustomer(int customerId);
	}

	public class OfferCalculationsRepository : NHibernateRepositoryBase<OfferCalculations>, IOfferCalculationsRepository
	{
		public OfferCalculationsRepository(ISession session) : base(session) { }

        public OfferCalculations GetActiveForCustomer(int customerId) {
            return GetAll().FirstOrDefault(x => x.CustomerId == customerId && x.IsActive);
        }
	}

}
