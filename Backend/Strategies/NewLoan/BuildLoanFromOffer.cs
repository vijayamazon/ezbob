namespace Ezbob.Backend.Strategies.NewLoan {
	using System;
	using System.Globalization;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Backend.Strategies.NewLoan.Exceptions;
	using Ezbob.Database;

	/// <summary>
	/// Loading last valid offer data for customer from DB and fill in NL_Model (Loan, last History) 
	/// </summary>
	public class BuildLoanFromOffer : AStrategy {

		public BuildLoanFromOffer(NL_Model model) {
			Result = model;
		} // constructor

		public override string Name { get { return "BuildLoanFromOffer"; } }
		public NL_Model Result { get; private set; }
		public string Error;

		/*	
			ValidateCustomer(cus); // continue (customer's data/status, finish wizard, bank account data)
			ValidateAmount(loanAmount, cus); // continue (loanAmount > customer.CreditSum)
			ValidateOffer(cus); // check offer validity dates - in AddLoan strategy
			ValidateLoanDelay(cus, now, TimeSpan.FromMinutes(1)); // checks if last loan was taken a minute before "now" - ?? to prevent multiple clicking on "create loan" button?
			ValidateRepaymentPeriodAndInterestRate(cus);
		*/
		// all validations moved to SP

		public override void Execute() {
			NL_AddLog(LogType.Info, "Strategy Start", Result, null, null, null);
			try {

				if (Result.CustomerID == 0) {
					this.Error = NL_ExceptionCustomerNotFound.DefaultMessage;
					NL_AddLog(LogType.Info, "Strategy Failed", Result, Result, this.Error, null);
					return;
				}

				OfferForLoan dataForLoan = DB.FillFirst<OfferForLoan>(
					"NL_SignedOfferForLoan",
					CommandSpecies.StoredProcedure,
					new QueryParameter("CustomerID", Result.CustomerID),
					new QueryParameter("@Now", Result.Loan.LastHistory().EventTime)
				);

			    if (!string.IsNullOrEmpty(dataForLoan.Error)) {
                    this.Error = string.Format(dataForLoan.Error + " dataForLoan: {0} ", Result); 
                    NL_AddLog(LogType.Info, "Strategy Failed", Result, Result, this.Error, null);
                    return;
			    }


				if (dataForLoan == null || dataForLoan.OfferID == 0) {
					this.Error = NL_ExceptionOfferNotValid.DefaultMessage;
					NL_AddLog(LogType.Info, "Strategy Failed", Result, Result, this.Error, null);
                    return;
				}

				Log.Debug(dataForLoan.ToString());

				if (dataForLoan.AvailableAmount < dataForLoan.LoanLegalAmount) {
					this.Error = string.Format("No available credit for current offer. New loan is not allowed. dataForLoan: {0} ", Result); // duplicate of ValidateAmount(loanAmount, cus); (loanAmount > customer.CreditSum)
					NL_AddLog(LogType.Info, "Strategy Failed - No available credit for current offer. New loan is not allowed", Result, Result, this.Error, null);
                    return;
				}

				if (!string.IsNullOrEmpty(Result.Loan.Refnum) && !string.IsNullOrEmpty(dataForLoan.ExistsRefnums) && dataForLoan.ExistsRefnums.Contains(Result.Loan.Refnum)) {
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
				Result.Loan.OfferID = dataForLoan.OfferID;
				Result.Loan.LoanTypeID = dataForLoan.LoanTypeID; // if need a string: get description from NLLoanTypes Enum
				Result.Loan.LoanSourceID = dataForLoan.LoanSourceID;
				// EzbobBankAccountID - TODO
				Result.Loan.Position = dataForLoan.LoansCount;

				NL_LoanHistory history = Result.Loan.LastHistory();

				// from offer => history initial/re-scheduling data
				history.InterestOnlyRepaymentCount = dataForLoan.InterestOnlyRepaymentCount;
				history.Amount = dataForLoan.LoanLegalAmount;
				history.RepaymentCount = dataForLoan.LoanLegalRepaymentPeriod;
				history.RepaymentIntervalTypeID = dataForLoan.RepaymentIntervalTypeID;
				history.InterestRate = dataForLoan.MonthlyInterestRate;
				history.LoanLegalID = dataForLoan.LoanLegalID;

				// from offer => Offer
				Result.Offer = new NL_Offers {
					BrokerSetupFeePercent = dataForLoan.BrokerSetupFeePercent,
					SetupFeeAddedToLoan = dataForLoan.SetupFeeAddedToLoan,
					OfferID = dataForLoan.OfferID
				};

				// offer-fees
				Result.Offer.OfferFees = DB.Fill<NL_OfferFees>("NL_OfferFeesGet", CommandSpecies.StoredProcedure, new QueryParameter("@OfferID", dataForLoan.OfferID));

				//Result.Offer.OfferFees.ForEach(ff => Log.Debug(ff));

				// discounts
				if (dataForLoan.DiscountPlanID > 0) {
					var discounts = DB.Fill<NL_DiscountPlanEntries>("NL_DiscountPlanEntriesGet",
						CommandSpecies.StoredProcedure,
						new QueryParameter("@DiscountPlanID", dataForLoan.DiscountPlanID)
					);
					foreach (NL_DiscountPlanEntries dpe in discounts) {
						Result.Offer.DiscountPlan.Add(Decimal.Parse(dpe.InterestDiscount.ToString(CultureInfo.InvariantCulture)));
					}
					//Log.Debug("Discounts");
					//Result.Offer.DiscountPlan.ForEach(d => Log.Debug(d));
				}

				NL_AddLog(LogType.Info, "Strategy End", Result, Result, this.Error, null);

			} catch (Exception ex) {
				NL_AddLog(LogType.Error, "Strategy Faild", Result, Result, ex.ToString(), ex.StackTrace);
			}
		} // Execute

	} // class BuildLoanFromOffer
} // ns