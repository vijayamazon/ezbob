namespace EzBob.Backend.Strategies.PricingModel
{
	using System.Data;
	using ConfigManager;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class GetPricingModelModel : AStrategy
	{
		private readonly int customerId;

		public GetPricingModelModel(int customerId, AConnection oDb, ASafeLog oLog)
			: base(oDb, oLog)
		{
			this.customerId = customerId;
		}

		public override string Name {
			get { return "Get pricing model model"; }
		}

		public PricingModelModel Model { get; private set; }
		
		public override void Execute()
		{
			decimal defaultRateCompanyShare, defaultRateCustomerShare;
			decimal defaultRate = GetDefaultRate(out defaultRateCompanyShare, out defaultRateCustomerShare);
			int loanAmount, loanTerm;
			GetDataFromCashRequest(out loanAmount, out loanTerm);
			int interestOnlyPeriod = CurrentValues.Instance.PricingModelInterestOnlyPeriod;
			decimal tenurePercents = CurrentValues.Instance.PricingModelTenurePercents;
			decimal tenureMonths = tenurePercents * loanTerm;
			decimal setupFeePercents = CurrentValues.Instance.PricingModelSetupFee;
			decimal brokerSetupFeePercents = CurrentValues.Instance.PricingModelBrokerSetupFee;

			Model = new PricingModelModel
				{
					LoanAmount = loanAmount,
					DefaultRate = defaultRate,
					DefaultRateCompanyShare = defaultRateCompanyShare,
					DefaultRateCustomerShare = defaultRateCustomerShare,
					SetupFeePounds = setupFeePercents * loanAmount,
					SetupFeePercents = setupFeePercents,
					BrokerSetupFeePounds = brokerSetupFeePercents * loanAmount,
					BrokerSetupFeePercents = brokerSetupFeePercents,
					LoanTerm = loanTerm,
					InterestOnlyPeriod = interestOnlyPeriod,
					TenurePercents = tenurePercents,
					TenureMonths = tenureMonths,
					CollectionRate = CurrentValues.Instance.PricingModelCollectionRate,
					EuCollectionRate = CurrentValues.Instance.PricingModelEuCollectionRate,
					Cogs = CurrentValues.Instance.PricingModelCogs,
					DebtPercentOfCapital = CurrentValues.Instance.PricingModelDebtOutOfTotalCapital,
					CostOfDebt = CurrentValues.Instance.PricingModelCostOfDebtPA,
					OpexAndCapex = CurrentValues.Instance.PricingModelOpexAndCapex,
					ProfitMarkup = CurrentValues.Instance.PricingModelProfitMarkupPercentsOfRevenue
				};
		}

		private void GetDataFromCashRequest(out int loanAmount, out int loanTerm)
		{
			loanAmount = 0;
			loanTerm = 12;
			DataTable dt = DB.ExecuteReader(
				"GetLastCashRequestForPricingModel",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId)
			);

			if (dt.Rows.Count == 1)
			{
				var sr = new SafeReader(dt.Rows[0]);
				loanAmount = sr["ApprovedAmount"];
				loanTerm = sr["RepaymentPeriod"];
			}
		}

		private decimal GetDefaultRate(out decimal defaultRateCompanyShare, out decimal defaultRateCustomerShare)
		{
			defaultRateCompanyShare = CurrentValues.Instance.PricingModelDefaultRateCompanyShare;
			defaultRateCustomerShare = 1 - defaultRateCompanyShare;

			var instance = new GetPricingModelDefaultRate(customerId, defaultRateCompanyShare, DB, Log);
			instance.Execute();
			return instance.DefaultRate;
		}
	}
}
