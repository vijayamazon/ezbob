using FluentNHibernate.Mapping;

namespace EZBob.DatabaseLib.Model.Database
{
	using Loans;
	using NHibernate.Type;

	public sealed class CashRequestMap : ClassMap<CashRequest> 
    {
        public CashRequestMap()
        {
            Table("CashRequests");
            LazyLoad();
            Id(x => x.Id);
            Map(x => x.HasLoans);
            References(x => x.Customer, "IdCustomer");
            Map(x => x.IdUnderwriter);
            Map(x => x.CreationDate);
            Map(x => x.SystemDecision).CustomType<SystemDecisionType>();
            Map(x => x.UnderwriterDecision).CustomType<CreditResultStatusType>();
            Map(x => x.SystemDecisionDate);
            Map(x => x.UnderwriterDecisionDate);
            Map(x => x.EscalatedDate);
            Map(x => x.SystemCalculatedSum);
            Map(x => x.ManagerApprovedSum);
            Map(x => x.MedalType).CustomType<MedalType>();
            Map(x => x.EscalationReason);
            Map(x => x.InterestRate);
            Map(x => x.APR);
            Map(x => x.RepaymentPeriod);
            Map(x => x.UseSetupFee);
			Map(x => x.UseBrokerSetupFee);
            Map(x => x.EmailSendingBanned);
            Map(x => x.UnderwriterComment).Length(200);
            References(x => x.LoanType, "LoanTypeId");
            Map(x => x.LoanTemplate).CustomType("StringClob");
            Map(x => x.IsLoanTypeSelectionAllowed);
            References(x => x.DiscountPlan, "DiscountPlanId");
            Map(x => x.OfferStart).CustomType<UtcDateTimeType>();
            Map(x => x.OfferValidUntil).CustomType<UtcDateTimeType>();
            References(x => x.LoanSource, "LoanSourceID");
            Map(x => x.IsCustomerRepaymentPeriodSelectionAllowed);
	        HasMany(x => x.LoanLegals)
		        .AsBag()
		        .KeyColumn("CashRequestsId")
		        .Cascade.All()
		        .Inverse();
	        Map(x => x.AnnualTurnover, "AnualTurnover");
        }
    }
}
