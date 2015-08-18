namespace Ezbob.Backend.Strategies.NewLoan {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using DbConstants;
	using Ezbob.Backend.CalculateLoan.LoanCalculator;
	using Ezbob.Backend.CalculateLoan.LoanCalculator.Exceptions;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Database;
	using NHibernate.Linq;

	/// <summary>
	/// Create loan Schedule and setup/arrangement/servicing Fees
	/// Arguments: NL_Model with CustomerID, CalculatorImplementation (BankLikeLoanCalculator|LegacyLoanCalculator), Histories.NL_LoanHistory.EventTime
	/// If the customer has valid offer, NL_Model Result contains Schedule, Fees, APR, Error
	/// </summary>
	public class CalculateLoanSchedule : AStrategy {
		public CalculateLoanSchedule(NL_Model nlModel) {
			model = nlModel;
			this.Result = nlModel;
		}//constructor

		public override string Name { get { return "CalculateLoanSchedule"; } }
		public NL_Model model { get; set; } // input
		public NL_Model Result; // output

		public override void Execute() {

			if (model.CustomerID == 0) {
				this.Result.Error = NL_ExceptionCustomerNotFound.DefaultMessage;
				return;
			}
			
			var history = model.Histories.OrderBy(h => h.EventTime).LastOrDefault();

			if (history == null) {
				this.Result.Error = NL_ExceptionRequiredDataNotFound.LastHistory;
				return;
			}

			if (history.EventTime == DateTime.MinValue) {
				this.Result.Error = NL_ExceptionRequiredDataNotFound.HistoryEventTime;
				return;
			}

			OfferForLoan dataForLoan = DB.FillFirst<OfferForLoan>(
				"NL_OfferForLoan",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerID", model.CustomerID),
				new QueryParameter("@Now", history.EventTime)
			);

			if (dataForLoan.OfferID == 0) {
				this.Result.Error = NL_ExceptionOfferNotValid.DefaultMessage;
				return;
			}

			Log.Debug("OfferDataForLoan: {0}", dataForLoan);

			try {

				// from offer => Loan
				model.Loan.OfferID = dataForLoan.OfferID;
				model.Loan.LoanTypeID = dataForLoan.LoanTypeID; // if need a string: get description from NLLoanTypes Enum
				model.Loan.LoanStatusID = (int)NLLoanStatuses.Live;
				model.Loan.LoanSourceID = dataForLoan.LoanSourceID;
				// EzbobBankAccountID - TODO
				model.Loan.InterestOnlyRepaymentCount = dataForLoan.InterestOnlyRepaymentCount;
				model.Loan.Position = dataForLoan.LoansCount;
				model.Loan.CreationTime = DateTime.UtcNow;

				// from offer => history initial/re-scheduling data
				history.Amount = dataForLoan.LoanLegalAmount;
				history.RepaymentCount = dataForLoan.LoanLegalRepaymentPeriod;
				history.RepaymentIntervalTypeID = dataForLoan.RepaymentIntervalTypeID;
				history.InterestRate = dataForLoan.MonthlyInterestRate;
				history.EventTime = DateTime.UtcNow;

				// from offer => Offer
				model.Offer = new NL_Offers {
					BrokerSetupFeePercent = dataForLoan.BrokerSetupFeePercent,
					SetupFeeAddedToLoan = dataForLoan.SetupFeeAddedToLoan,
				};

				// offer-fees
				List<NL_OfferFees> offerFees = DB.Fill<NL_OfferFees>(
					"NL_OfferFeesGet",
					CommandSpecies.StoredProcedure,
					new QueryParameter("@OfferID", dataForLoan.OfferID)
					);

				foreach (NL_OfferFees offerFee in offerFees)
					model.Fees.Add(new NLFeeItem {
						OfferFees = offerFee
					});

				if (dataForLoan.DiscountPlanID > 0) {
					var discounts = DB.Fill<NL_DiscountPlanEntries>(
						"NL_DiscountPlanEntriesGet",
						CommandSpecies.StoredProcedure,
						new QueryParameter("@DiscountPlanID", dataForLoan.DiscountPlanID)
						);

					foreach (NL_DiscountPlanEntries dpe in discounts) {
						model.DiscountPlan.Insert(dpe.PaymentOrder, Decimal.Parse(dpe.InterestDiscount.ToString(CultureInfo.InvariantCulture)));
					}

					Log.Debug("Discounts: {0}", model.DiscountPlan);
				}

				// init calculator
				ALoanCalculator nlCalculator = new LegacyLoanCalculator(model);
				if (model.CalculatorImplementation.GetType() == typeof(BankLikeLoanCalculator)) {
					nlCalculator = new BankLikeLoanCalculator(model);
				}

				// model should contain Schedule and Fees after this invocation
				nlCalculator.CreateSchedule();

				// update relevant history entry
				model.Histories.Where(h => h.EventTime == history.EventTime)
					.ForEach(h => h = history);

				// set APR 
				model.APR = nlCalculator.CalculateApr(history.EventTime);

				Log.Debug("model.Loan: {0}, model.Offer: {1}, model.APR: {2}, history: {3}", model.Loan, model.Offer, model.APR, history);
				Log.Debug("Schedule: ");
				model.Schedule.ForEach(s => Log.Debug(s.ScheduleItem.ToString()));
				Log.Debug("Fees: ");
				model.Fees.ForEach(f => Log.Debug(f.Fee));
				Log.Debug("Histories: ");
				model.Histories.ForEach(h => Log.Debug(h));

				// init result by the model
				this.Result = model;

			} catch (NoInitialDataException noDataException) {
				this.Result.Error = noDataException.Message;
			} catch (InvalidInitialAmountException amountException) {
				this.Result.Error = amountException.Message;
			} catch (InvalidInitialInterestRateException interestRateException) {
				this.Result.Error = interestRateException.Message;
			} catch (InvalidInitialRepaymentCountException paymentsException) {
				this.Result.Error = paymentsException.Message;
			} catch (InvalidInitialInterestOnlyRepaymentCountException xxException) {
				this.Result.Error = xxException.Message;
				// ReSharper disable once CatchAllClause
			} catch (Exception ex) {
				this.Result = model;
				this.Result.Error = string.Format("Failed to calculate Schedule (NL_Model.NlScheduleItems list) for customer {0}, err: {1}", model.CustomerID, ex);
			}

		}//Execute

	}//class CalculateLoanSchedule
}//ns