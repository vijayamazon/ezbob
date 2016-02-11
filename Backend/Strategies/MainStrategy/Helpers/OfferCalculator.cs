namespace Ezbob.Backend.Strategies.MainStrategy.Helpers {
	using System.Linq;
	using AutomationCalculator.Common;
	using DbConstants;
	using Ezbob.Backend.ModelsWithDB;
	using Ezbob.Backend.Strategies.PricingModel;
	using Ezbob.Logger;
	using EZBob.DatabaseLib.Model.Database.Loans;

	internal class OfferCalculator {
		public OfferCalculator(
			AutoDecisionFlowTypes flowType,
			int customerID,
			int loanSourceID,
			int loanAmount,
			int repaymentPeriod,
			bool hasLoans,
			bool aspireToMinSetupFee,
			int smallLoanScenarioLimit,
			decimal minInterestRate,
			decimal maxInterestRate,
			decimal minSetupFee,
			decimal maxSetupFee,
			ASafeLog log
		) {
			this.flowType = flowType;
			this.customerID = customerID;
			this.loanSourceID = loanSourceID;
			this.loanAmount = loanAmount;
			this.repaymentPeriod = repaymentPeriod;
			this.hasLoans = hasLoans;
			this.aspireToMinSetupFee = aspireToMinSetupFee;
			this.smallLoanScenarioLimit = smallLoanScenarioLimit;
			this.minInterestRate = minInterestRate;
			this.maxInterestRate = maxInterestRate;
			this.minSetupFee = minSetupFee;
			this.maxSetupFee = maxSetupFee;
			this.log = log.Safe();

			Success = false;
			InterestRate = 0;
			SetupFee = 0;
		} // constructor

		public bool Success { get; private set; }

		/// <summary>
		/// Between 0 (which is 0%) and 1 (which is 100%).
		/// </summary>
		public decimal InterestRate { get; private set; }

		/// <summary>
		/// Between 0 (which is 0%) and 1 (which is 100%).
		/// </summary>
		public decimal SetupFee { get; private set; }

		public OfferCalculator Calculate() {
			var calculatorModel = InitCalcualtorModel();

			if (calculatorModel == null)
				return this;

			var calculator = new PricingModelCalculator(this.customerID, calculatorModel) {
				ThrowExceptionOnError = false,
				CalculateApr = false,
				TargetLoanSource = (LoanSourceName)this.loanSourceID
			};
			calculator.Execute();

			if (!string.IsNullOrWhiteSpace(calculator.Error)) {
				this.log.Warn("Calculator error: {0}", calculator.Error);
				return this;
			} // if

			PricingSourceModel calculatorOutput = (calculatorModel.PricingSourceModels == null)
				? null
				: calculatorModel.PricingSourceModels.FirstOrDefault(
					r => r.IsPreferable && ((int)r.LoanSource == this.loanSourceID)
				);

			if (calculatorOutput == null) {
				this.log.Warn("Calculator output not found for loan source '{0}'.", this.loanSourceID);
				return this;
			} // if

			InterestRate = calculatorOutput.InterestRate;
			SetupFee = calculatorOutput.SetupFee;

			Success = true;

			if ((this.minInterestRate <= InterestRate) && (InterestRate <= this.maxInterestRate))
				return this;

			if ((this.minSetupFee == this.maxSetupFee) || !this.aspireToMinSetupFee)
				InterestRate = this.minInterestRate;
			else
				InterestRate = this.maxInterestRate;

			return this;
		} // Calculate

		private PricingModelModel InitCalcualtorModel() {
			PricingCalcuatorScenarioNames scenarioName;

			if (this.loanAmount <= this.smallLoanScenarioLimit)
				scenarioName = PricingCalcuatorScenarioNames.SmallLoan;
			else if (this.hasLoans)
				scenarioName = PricingCalcuatorScenarioNames.BasicRepeating;
			else
				scenarioName = PricingCalcuatorScenarioNames.BasicNew;

			var generator = new GetPricingModelModel(this.customerID, scenarioName) { LoadFromLastCashRequest = false, };
			generator.Execute();

			if (!string.IsNullOrWhiteSpace(generator.Error)) {
				this.log.Warn("Init calculator model error '{0}'.", generator.Error);
				return null;
			} // if

			generator.Model.FlowType = this.flowType;
			generator.Model.LoanAmount = this.loanAmount;
			generator.Model.SetupFeePercents = this.aspireToMinSetupFee ? this.minSetupFee : this.maxSetupFee;
			generator.Model.LoanTerm = this.repaymentPeriod;

			return generator.Model;
		} // InitCalculatorModel

		private readonly AutoDecisionFlowTypes flowType;
		private readonly int customerID;
		private readonly int loanSourceID;
		private readonly int loanAmount;
		private readonly int repaymentPeriod;
		private readonly bool hasLoans;
		private readonly bool aspireToMinSetupFee;
		private readonly int smallLoanScenarioLimit;
		private readonly decimal minInterestRate;
		private readonly decimal maxInterestRate;
		private readonly decimal minSetupFee;
		private readonly decimal maxSetupFee;
		private readonly ASafeLog log;
	} // class OfferCalculator
} // namespace
