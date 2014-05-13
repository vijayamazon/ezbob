namespace EZBob.DatabaseLib.Model.Marketplaces.ChannelGrabber
{
	using System;
	using FluentNHibernate.Mapping;
	using NHibernate.Type;

	[Serializable]
	public class MP_VatReturnSummary
	{
		public virtual int Id { get; set; }
		public virtual int CustomerId { get; set; }
		public virtual int BusinessId { get; set; }
		public virtual DateTime CreationDate { get; set; }
		public virtual bool IsActive { get; set; }
		public virtual string Currency { get; set; }
		public virtual decimal PctOfAnnualRevenues { get; set; }
		public virtual decimal Revenues { get; set; }
		public virtual decimal Opex { get; set; }
		public virtual decimal TotalValueAdded { get; set; }
		public virtual decimal PctOfRevenues { get; set; }
		public virtual decimal Salaries { get; set; }
		public virtual decimal Tax { get; set; }
		public virtual decimal Ebida { get; set; }
		public virtual decimal PctOfAnnual { get; set; }
		public virtual decimal ActualLoanRepayment { get; set; }
		public virtual decimal FreeCashFlow { get; set; }
		public virtual decimal SalariesMultiplier { get; set; }
		public virtual int CustomerMarketplaceId { get; set; }
	}

	public class MP_VatReturnSummaryMap : ClassMap<MP_VatReturnSummary>
	{
		public MP_VatReturnSummaryMap()
		{
			Table("MP_VatReturnSummary");
			ReadOnly();
			Id(x => x.Id, "SummaryID");
			Map(x => x.CustomerId, "CustomerID");
			Map(x => x.BusinessId, "BusinessID");
			Map(x => x.CreationDate, "CreationDate").CustomType<UtcDateTimeType>();
			Map(x => x.IsActive, "IsActive");
			Map(x => x.Currency, "Currency").Length(3);
			Map(x => x.PctOfAnnualRevenues, "PctOfAnnualRevenues");
			Map(x => x.Revenues, "Revenues");
			Map(x => x.Opex, "Opex");
			Map(x => x.TotalValueAdded, "TotalValueAdded");
			Map(x => x.PctOfRevenues, "PctOfRevenues");
			Map(x => x.Salaries, "Salaries");
			Map(x => x.Tax, "Tax");
			Map(x => x.Ebida, "Ebida");
			Map(x => x.PctOfAnnual, "PctOfAnnual");
			Map(x => x.ActualLoanRepayment, "ActualLoanRepayment");
			Map(x => x.FreeCashFlow, "FreeCashFlow");
			Map(x => x.SalariesMultiplier, "SalariesMultiplier");
			Map(x => x.CustomerMarketplaceId, "CustomerMarketplaceID");
		}
	}
}

namespace EZBob.DatabaseLib.Model.Database.Repository
{
	using System.Linq;
	using ApplicationMng.Repository;
	using Marketplaces.ChannelGrabber;
	using NHibernate;

	public class VatReturnSummaryRepository : NHibernateRepositoryBase<MP_VatReturnSummary>
	{
		public VatReturnSummaryRepository(ISession session)
			: base(session)
		{
			
		}

		public MP_VatReturnSummary GetLastSummary(int mpId)
		{
			return GetAll().Where(x => x.CustomerMarketplaceId == mpId).OrderByDescending(x => x.CreationDate).FirstOrDefault();
		}

	}
}