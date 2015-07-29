namespace Ezbob.Backend.Strategies.NewLoan {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using DbConstants;
	using Ezbob.Backend.CalculateLoan.LoanCalculator;
	using Ezbob.Backend.CalculateLoan.Models;
	using Ezbob.Backend.CalculateLoan.Models.Helpers;
	using Ezbob.Backend.Models.NewLoan;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Database;
	using PaymentServices.Calculators;

	/// <summary>
	/// Create loan Schedule and setup Fees
	/// Arguments: NL_Model with CustomerID, CalculatorImplementation (BankLikeLoanCalculator|LegacyLoanCalculator), Loan.IssuedTime
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

			Log.Debug("------------------ customer {0}------------------------", model.CustomerID);

			string message;

			if (model.CustomerID == 0) {
				this.Result.Error = NL_ExceptionCustomerNotFound.DefaultMessage;
				return;
			}

			// input validation
			if (model.Loan == null) {
				message = string.Format("Expected input data not found (NL_Model initialized by: Loan.IssuedTime). Customer {0}", model.CustomerID);
				this.Result.Error = message;
				return;
			}

			if (model.CalculatorImplementation == null) {
				message = string.Format("Expected input data not found (NL_Model initialized by: CalculatorImplementation (example: model. ).");
				this.Result.Error = message;
				return;
			}

			OfferForLoan dataForLoan = DB.FillFirst<OfferForLoan>("NL_OfferForLoan", CommandSpecies.StoredProcedure, new QueryParameter("CustomerID", model.CustomerID), new QueryParameter("@Now", DateTime.UtcNow));

			if (dataForLoan == null) {
				this.Result.Error = NL_ExceptionOfferNotValid.DefaultMessage;
				return;
			}

			Log.Debug(dataForLoan.ToString());

			try {

				// from offer 
				model.Loan.InitialLoanAmount = dataForLoan.LoanLegalAmount;
				model.Loan.RepaymentCount = dataForLoan.LoanLegalRepaymentPeriod;
				model.Loan.LoanTypeID = dataForLoan.LoanTypeID; // if need a string: get description from NLLoanTypes Enum
				model.Loan.LoanSourceID = dataForLoan.LoanSourceID;
				model.Loan.OfferID = dataForLoan.OfferID;
				model.Loan.LoanStatusID = (int)NLLoanStatuses.Live; 
				model.Loan.RepaymentIntervalTypeID = dataForLoan.RepaymentIntervalTypeID;
				model.Loan.InterestRate = dataForLoan.MonthlyInterestRate;
				model.Loan.InterestOnlyRepaymentCount = dataForLoan.InterestOnlyRepaymentCount;
				model.Loan.Position = dataForLoan.LoansCount;

				model.Offer = new NL_Offers() {
					BrokerSetupFeePercent = dataForLoan.BrokerSetupFeePercent,
					//SetupFeePercent = dataForLoan.SetupFeePercent,
					//ServicingFeePercent = dataForLoan.ServicingFeePercent,
					SetupFeeAddedToLoan = dataForLoan.SetupFeeAddedToLoan
				};

				Log.Debug(model.Loan.ToString());
				Log.Debug(model.Offer.ToString());

				LoanCalculatorModel nlCalculatorModel = new LoanCalculatorModel {
					LoanIssueTime = model.Loan.IssuedTime, // input
					RepaymentIntervalType = (RepaymentIntervalTypes)Enum.ToObject(typeof(RepaymentIntervalTypesId), model.Loan.RepaymentIntervalTypeID), // offer|LoanLegal
					LoanAmount = model.Loan.InitialLoanAmount, // offer|LoanLegal
					RepaymentCount = model.Loan.RepaymentCount, //  offer|LoanLegal
					MonthlyInterestRate = model.Loan.InterestRate, //  offer|LoanLegal
					InterestOnlyRepayments = model.Loan.InterestOnlyRepaymentCount ?? 0 //  offer|LoanLegal
				};

				// set discounts
				if (dataForLoan.DiscountPlan != null) {
					string[] stringSeparator = { "," };
					char[] removeChar = { ',' };
					string[] result = dataForLoan.DiscountPlan.Trim(removeChar).Split(stringSeparator, StringSplitOptions.None);
					decimal[] dpe = new decimal[result.Length];
					var i = 0;
					foreach (string s in result) {
						dpe.SetValue(Decimal.Parse(s), i++);
					}
					nlCalculatorModel.SetDiscountPlan(dpe);
				}

				Log.Debug("calculatorModel: " + nlCalculatorModel);

				
				ALoanCalculator nlCalculator = new LegacyLoanCalculator(nlCalculatorModel);

				if (model.CalculatorImplementation.GetType() == typeof(BankLikeLoanCalculator)) {
					nlCalculator = new BankLikeLoanCalculator(nlCalculatorModel);
				}

				// get initial schedule plan
				List<ScheduledItemWithAmountDue> shedules = nlCalculator.CreateScheduleAndPlan();

				Log.Debug("SCHEDULES primary= {0}", shedules);

				// get offers' fees
				List<NL_OfferFees> offerFees = DB.Fill<NL_OfferFees>("NL_OfferFeesGet", CommandSpecies.StoredProcedure, new QueryParameter("@OfferID", dataForLoan.OfferID));

				var setup = offerFees.FirstOrDefault(f => f.LoanFeeTypeID == (int)FeeTypes.SetupFee);

				// FEES TODO
				// setup fee
				//if (setup != null) {

				//	var feeCalculator = new SetupFeeCalculator(setup.Percent, dataForLoan.BrokerSetupFeePercent);
				//	decimal setupFeeAmount = feeCalculator.Calculate(model.Loan.InitialLoanAmount);

				//	Log.Debug("setupFeeAmount initial: {0}", setupFeeAmount);

				//	var servicing = offerFees.FirstOrDefault(f => f.LoanFeeTypeID == (int)FeeTypes.ServicingFee);

				//	// CashRequest.SpreadSetupFee => ServicingFeePercent in "old loan": in NL - add service fees 
				//	if (servicing != null) {
				//		if (servicing.Percent != null) {
				//			//decimal servicingFeeAmount = setupFeeAmount * (decimal)servicing.Percent;
				//			//setupFeeAmount -= servicingFeeAmount;
				//			int schedulesCount = shedules.Count;

				//			decimal iFee = Decimal.Round(servicingFeeAmount / schedulesCount);
				//			decimal firstFee = (servicingFeeAmount - iFee * (schedulesCount - 1));

				//			foreach (ScheduledItemWithAmountDue s1 in shedules) {
				//				nlCalculatorModel.Fees.Add(new Fee(s1.Date, (schedulesCount > 0) ? firstFee : iFee, FeeTypes.ServicingFee));
				//				schedulesCount = 0; // reset count, because it used as firstFee/iFee flag
				//			}
				//		}
				//	}
				//}

				// get schedules with fees
				shedules = nlCalculator.CreateScheduleAndPlan();

				Log.Debug("SCHEDULES with fees: {0}", shedules);

				// fill in NL_Model
				model.Schedule = new List<NLScheduleItem>();
				model.Fees = new List<NLFeeItem>();

				foreach (var s in shedules) {

					// schedule item
					NL_LoanSchedules iItem = new NL_LoanSchedules() {
						InterestRate = s.InterestRate,
						PlannedDate = s.Date,
						Position = s.Position,
						Principal = s.Principal,
						LoanScheduleStatusID = (int)NLScheduleStatuses.StillToPay
					};

					model.Schedule.Add(new NLScheduleItem() { ScheduleItem = iItem });

					// set appropriate distributed setup fee and add it to the model.Fees list
					var fee = nlCalculatorModel.Fees.FirstOrDefault(f => f.AssignDate.Date == s.Date.Date && f.FType == FeeTypes.SetupFee);

					if (fee != null) {
						model.Fees.Add(new NLFeeItem() {
							Fee = new NL_LoanFees() {
								Amount = fee.Amount,
								AssignTime = fee.AssignDate,
								Notes = "distributed setup fee part",
								LoanFeeTypeID = (int)FeeTypes.SetupFee
							}
						});
					}// distributed setup fee
				}

				// FEES TODO
				// regular setupFee
				//if (setupFeeAmount > 0) {
				//	model.Fees.Add(new NLFeeItem() {
				//		Fee = new NL_LoanFees() {
				//			LoanFeeTypeID = (int)FeeTypes.SetupFee,
				//			Amount = setupFeeAmount,
				//			AssignTime = DateTime.UtcNow,
				//			CreatedTime = DateTime.UtcNow,
				//			Notes = "setup fee",
				//		}
				//	});
				//} // if

				// set APR - TBD
				model.APR = nlCalculator.CalculateApr();

				// init result by the model
				this.Result = model;

				// ReSharper disable once CatchAllClause
			} catch (Exception ex) {
				message = string.Format("Failed to calculate Schedule (NL_Model.NlScheduleItems list) for customer {0}, err: {1}", model.CustomerID, ex);
				this.Result.Error = message;
			}

		}//Execute

	}//class CalculateLoanSchedule
}//ns