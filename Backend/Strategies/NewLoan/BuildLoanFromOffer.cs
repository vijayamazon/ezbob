namespace Ezbob.Backend.Strategies.NewLoan {
	using System;
	using System.Globalization;
	using DbConstants;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Backend.Strategies.NewLoan.Exceptions;
	using Ezbob.Database;

	/// <summary>
	/// Loading last valid offer data for customer from DB and fill in NL_Model (Loan, last History) 
	/// </summary>
	public class BuildLoanFromOffer : AStrategy {

		public BuildLoanFromOffer(NL_Model model) {
			this.model = model;
			Result = this.model;
		} // constructor

		public override string Name { get { return "BuildLoanFromOffer"; } }
		public NL_Model Result { get; private set; }
		private readonly NL_Model model;
		public string Error;

		/*	
			ValidateCustomer(cus); // continue (customer's data/status, finish wizard, bank account data)
			ValidateAmount(loanAmount, cus); // continue (loanAmount > customer.CreditSum)
			ValidateOffer(cus); // check offer validity dates - in AddLoan strategy
			ValidateLoanDelay(cus, now, TimeSpan.FromMinutes(1)); // checks if last loan was taken a minute before "now" - ?? to prevent multiple clicking on "create loan" button?
			ValidateRepaymentPeriodAndInterestRate(cus);
		*/

		/// <exception cref="ArgumentNullException"><paramref /> is null. </exception>
		/// <exception cref="FormatException"><paramref /> is not in the correct format. </exception>
		/// <exception cref="OverflowException"><paramref /> represents a number less than <see cref="F:System.Decimal.MinValue" /> or greater than <see cref="F:System.Decimal.MaxValue" />. </exception>
		public override void Execute() {

			OfferForLoan dataForLoan = DB.FillFirst<OfferForLoan>(
				"NL_SignedOfferForLoan",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerID", this.model.CustomerID),
				new QueryParameter("@Now", this.model.Loan.LastHistory().EventTime)
			);

			if (dataForLoan == null) {
				this.Error = NL_ExceptionOfferNotValid.DefaultMessage;
				return;
			}

			Log.Debug(dataForLoan.ToString());

			if (dataForLoan.OfferID == 0) {
				this.Error = NL_ExceptionOfferNotValid.DefaultMessage;
				return;
			}

			if (dataForLoan.AvailableAmount < dataForLoan.LoanLegalAmount) {
				this.Error = string.Format("No available credit for current offer. New loan is not allowed. dataForLoan: {0} ", Result); // duplicate of ValidateAmount(loanAmount, cus); (loanAmount > customer.CreditSum)
				return;
			}

			if (!string.IsNullOrEmpty(this.model.Loan.Refnum) && !string.IsNullOrEmpty(dataForLoan.ExistsRefnums) && dataForLoan.ExistsRefnums.Contains(this.model.Loan.Refnum)) {
				this.Error = NL_ExceptionLoanExists.DefaultMessage;
			}

			/*** 
			//CHECK "Enough Funds" (uncomment WHEN old BE REMOVED from \App\PluginWeb\EzBob.Web\Code\LoanCreator.cs, method CreateLoan method)                    
			VerifyEnoughAvailableFunds enoughAvailableFunds = new VerifyEnoughAvailableFunds(model.Loan.InitialLoanAmount);
			enoughAvailableFunds.Execute();
			if (!enoughAvailableFunds.HasEnoughFunds) {
				  Log.Alert("No enough funds for loan: customer {0}; offer {1}", model.CustomerID, dataForLoan.Offer.OfferID);
			}
			****/

			// from offer => Loan
			this.model.Loan.OfferID = dataForLoan.OfferID;
			this.model.Loan.LoanTypeID = dataForLoan.LoanTypeID; // if need a string: get description from NLLoanTypes Enum
			this.model.Loan.LoanStatusID = (int)NLLoanStatuses.Live;
			this.model.Loan.LoanSourceID = dataForLoan.LoanSourceID;
			// EzbobBankAccountID - TODO
			this.model.Loan.Position = dataForLoan.LoansCount;
			this.model.Loan.CreationTime = DateTime.UtcNow;

			NL_LoanHistory history = this.model.Loan.LastHistory();

			// from offer => history initial/re-scheduling data
			history.InterestOnlyRepaymentCount = dataForLoan.InterestOnlyRepaymentCount;
			history.Amount = dataForLoan.LoanLegalAmount;
			history.RepaymentCount = dataForLoan.LoanLegalRepaymentPeriod;
			history.RepaymentIntervalTypeID = dataForLoan.RepaymentIntervalTypeID;
			history.InterestRate = dataForLoan.MonthlyInterestRate;
			history.EventTime = DateTime.UtcNow;

			// from offer => Offer
			this.model.Offer = new NL_Offers {
				BrokerSetupFeePercent = dataForLoan.BrokerSetupFeePercent,
				SetupFeeAddedToLoan = dataForLoan.SetupFeeAddedToLoan,
				OfferID = dataForLoan.OfferID,
				LoanLegalID = dataForLoan.LoanLegalID
			};

			// replace last history with updated one
			//int lastHistoryIndex = this.model.Loan.Histories.IndexOf(history);
			//this.model.Loan.Histories.RemoveAt(lastHistoryIndex);
			//this.model.Loan.Histories.Insert(lastHistoryIndex, history);

			// offer-fees
			this.model.Offer.OfferFees = DB.Fill<NL_OfferFees>(
				"NL_OfferFeesGet",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@OfferID", dataForLoan.OfferID)
				);

			this.model.Offer.OfferFees.ForEach(ff => Log.Debug(ff));

			// discounts
			if (dataForLoan.DiscountPlanID > 0) {
				var discounts = DB.Fill<NL_DiscountPlanEntries>(
					"NL_DiscountPlanEntriesGet",
					CommandSpecies.StoredProcedure,
					new QueryParameter("@DiscountPlanID", dataForLoan.DiscountPlanID)
				);
				foreach (NL_DiscountPlanEntries dpe in discounts) {
					this.model.Offer.DiscountPlan.Add(Decimal.Parse(dpe.InterestDiscount.ToString(CultureInfo.InvariantCulture)));
				}
				Log.Debug("Discounts");
				this.model.Offer.DiscountPlan.ForEach(d => Log.Debug(d));
			}

			Result = this.model;

		} // Execute


	} // class BuildLoanFromOffer
} // ns
