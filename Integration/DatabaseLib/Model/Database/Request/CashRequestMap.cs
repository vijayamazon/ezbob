namespace EZBob.DatabaseLib.Model.Database {
	using FluentNHibernate.Mapping;
	using NHibernate.Type;

	public sealed class CashRequestMap : ClassMap<CashRequest> {
		public CashRequestMap() {
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
			Map(x => x.ApprovedRepaymentPeriod);
			Map(x => x.UseSetupFee);
			Map(x => x.UseBrokerSetupFee);
			Map(x => x.ManualSetupFeeAmount);
			Map(x => x.ManualSetupFeePercent);
			Map(x => x.BrokerSetupFeePercent);
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

			References(x => x.QuickOffer, "QuickOfferID").Nullable().Cascade.All();

			Map(x => x.ExpirianRating);
			Map(x => x.ScorePoints);
			Map(x => x.Originator).CustomType<CashRequestOriginatorType>();
			HasMany(x => x.DecisionHistories).KeyColumn("CashRequestId").Cascade.All().Inverse();
			Map(x => x.AutoDecisionID);
			Map(x => x.SpreadSetupFee);
			Map(x => x.ProductSubTypeID);
			Map(x => x.HasApprovalChance);
			Map(x => x.UwUpdatedFees);
		} // constructor
	} // class CashRequestMap
} // namespace
