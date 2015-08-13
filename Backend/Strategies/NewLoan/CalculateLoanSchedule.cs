﻿namespace Ezbob.Backend.Strategies.NewLoan {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using DbConstants;
	using Ezbob.Backend.CalculateLoan.LoanCalculator;
	using Ezbob.Backend.CalculateLoan.Models;
	using Ezbob.Backend.CalculateLoan.Models.Helpers;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Database;
	using PaymentServices.Calculators;

	/// <summary>
	/// Create loan Schedule and setup/arrangement/servicing Fees
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
			if (model.CustomerID == 0) {
				this.Result.Error = NL_ExceptionCustomerNotFound.DefaultMessage;
				return;
			}

			// input validation
			if (model.Loan == null) {
				this.Result.Error = string.Format("Expected input data not found (NL_Model initialized by: Loan.IssuedTime). Customer {0}", model.CustomerID);
				return;
			}

			// use default
			//if (model.CalculatorImplementation == null) {
			//	message = string.Format("Expected input data not found (NL_Model initialized by: CalculatorImplementation (example: model. ).");
			//	model.Error = message;
			//	this.Result = model;
			//	return;
			//}

			OfferForLoan dataForLoan = DB.FillFirst<OfferForLoan>(
				"NL_OfferForLoan",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerID", model.CustomerID),
				new QueryParameter("@Now", model.IssuedTime)
			);

			if (dataForLoan == null) {
				this.Result.Error = NL_ExceptionOfferNotValid.DefaultMessage;
				return;
			}

			Log.Debug("OfferDataForLoan: {0}", dataForLoan);

			try {
				// from offer => Loan
				model.InitialAmount = dataForLoan.LoanLegalAmount;
				model.InitialRepaymentCount = dataForLoan.LoanLegalRepaymentPeriod;
				model.Loan.LoanTypeID = dataForLoan.LoanTypeID; // if need a string: get description from NLLoanTypes Enum
				model.Loan.LoanSourceID = dataForLoan.LoanSourceID;
				model.Loan.OfferID = dataForLoan.OfferID;
				model.Loan.LoanStatusID = (int)NLLoanStatuses.Live;
				model.InitialRepaymentIntervalTypeID = dataForLoan.RepaymentIntervalTypeID;
				model.InitialInterestRate = dataForLoan.MonthlyInterestRate;
				model.Loan.InterestOnlyRepaymentCount = dataForLoan.InterestOnlyRepaymentCount;
				model.Loan.Position = dataForLoan.LoansCount;

				// from offer => Offer
				model.Offer = new NL_Offers {
					BrokerSetupFeePercent = dataForLoan.BrokerSetupFeePercent,
					SetupFeeAddedToLoan = dataForLoan.SetupFeeAddedToLoan,
				};

				// init calculator's model - loan's details
				LoanCalculatorModel nlCalculatorModel = new LoanCalculatorModel();

				// init calculator
				ALoanCalculator nlCalculator = new LegacyLoanCalculator(nlCalculatorModel);
				if (model.CalculatorImplementation.GetType() == typeof(BankLikeLoanCalculator)) {
					nlCalculator = new BankLikeLoanCalculator(nlCalculatorModel);
				}

				// calculator's model - set discounts
				if (dataForLoan.DiscountPlan != null) {
					string[] stringSeparator = { "," };
					char[] removeChar = { ',' };
					string[] result = dataForLoan.DiscountPlan.Trim(removeChar).Split(stringSeparator, StringSplitOptions.None);
					model.DiscountPlan = new decimal[result.Length];
					var i = 0;
					foreach (string s in result)
						model.DiscountPlan.SetValue(Decimal.Parse(s), i++);
				}

				List<NL_OfferFees> offerFees = DB.Fill<NL_OfferFees>(
					"NL_OfferFeesGet",
					CommandSpecies.StoredProcedure,
					new QueryParameter("@OfferID", dataForLoan.OfferID)
				);

				foreach (NL_OfferFees nof in offerFees)
					model.Fees.Add(new NLFeeItem { OfferFees = nof });

				/*
				if (offerFees != null) {

					var setupFee = offerFees.FirstOrDefault(f => f.LoanFeeTypeID == (int)FeeTypes.SetupFee);
					var servicingFee = offerFees.FirstOrDefault(f => f.LoanFeeTypeID == (int)FeeTypes.ServicingFee); // equal to "setup spreaded"

					// calculator's model - setup fee
					if (setupFee != null) {

						var feeCalculator = new SetupFeeCalculator(setupFee.Percent, dataForLoan.BrokerSetupFeePercent);

						decimal setupFeeAmount = feeCalculator.Calculate(model.InitialAmount);
						model.BrokerComissions = feeCalculator.CalculateBrokerFee(model.InitialAmount);

						Log.Debug("setupFeeAmount: {0}, brokerComissions: {1}", setupFeeAmount, model.BrokerComissions);

						nlCalculatorModel.Fees.Add(new Fee(model.IssuedTime, setupFeeAmount, FeeTypes.SetupFee));
					}

					// calculator's model - servicing fees
					if (servicingFee != null) {

						var feeCalculator = new SetupFeeCalculator(servicingFee.Percent, dataForLoan.BrokerSetupFeePercent);

						decimal servicingFeeAmount = feeCalculator.Calculate(model.InitialAmount);
						model.BrokerComissions = feeCalculator.CalculateBrokerFee(model.InitialAmount);

						Log.Debug("servicingFeeAmount: {0}", servicingFeeAmount); // "spreaded" amount

						int schedulesCount = shedules.Count;

						decimal iFee = Decimal.Round(servicingFeeAmount / schedulesCount);
						decimal firstFee = (servicingFeeAmount - iFee * (schedulesCount - 1));

						foreach (ScheduledItem scheduleItem in shedules) {
							nlCalculatorModel.Fees.Add(new Fee(scheduleItem.Date, (schedulesCount > 0) ? firstFee : iFee, FeeTypes.ServicingFee));
							schedulesCount = 0; // reset count, because it used as firstFee/iFee flag
						}
					}
				}
				*/

				// get schedules and fees
				List<ScheduledItemWithAmountDue> sheduleFeeList = nlCalculator.CreateScheduleAndPlan(model);

				Log.Debug("SCHEDULES plan + fees: {0}", sheduleFeeList);

				// fill in result NL_Model
				model.Schedule = new List<NLScheduleItem>();
				model.Fees = new List<NLFeeItem>();

				foreach (var s in sheduleFeeList) {

					model.Schedule.Add(new NLScheduleItem() {
						ScheduleItem = new NL_LoanSchedules() {
							InterestRate = s.InterestRate,
							PlannedDate = s.Date,
							Position = s.Position,
							Principal = s.Principal,
							LoanScheduleStatusID = (int)NLScheduleStatuses.StillToPay
						}
					});

					//  add appropriate "spread" fees model.Fees list
					var servicingFeeSchedule = nlCalculatorModel.Fees.FirstOrDefault(f => f.AssignDate.Date == s.Date.Date && f.FeeType == FeeTypes.ServicingFee);

					if (servicingFeeSchedule != null) {
						model.Fees.Add(new NLFeeItem() {
							Fee = new NL_LoanFees() {
								Amount = servicingFeeSchedule.Amount,
								AssignTime = servicingFeeSchedule.AssignDate,
								Notes = "spread (servicing) fee",
								LoanFeeTypeID = (int)FeeTypes.ServicingFee
							}
						});
					} // "spread" fees
				}

				// setup fee
				var setupFeeSchedule = nlCalculatorModel.Fees.FirstOrDefault(f => f.FeeType == FeeTypes.SetupFee);
				if (setupFeeSchedule != null) {
					model.Fees.Add(new NLFeeItem() {
						Fee = new NL_LoanFees() {
							Amount = setupFeeSchedule.Amount,
							AssignTime = setupFeeSchedule.AssignDate,
							Notes = "setup fee",
							LoanFeeTypeID = (int)FeeTypes.SetupFee
						}
					});
				}

				// set APR - TBD
				model.APR = nlCalculator.CalculateApr(model.IssuedTime);

				Log.Debug("model.Loan: {0}, model.Offer: {1}, model.APR: {2}", model.Loan, model.Offer, model.APR);
				Log.Debug("Schedule: ");
				model.Schedule.ForEach(s=>Log.Debug(s.ScheduleItem.ToString()));
				Log.Debug("Fees: ");
				model.Fees.ForEach(f => Log.Debug(f.Fee));

				// init result by the model
				this.Result = model;

				// ReSharper disable once CatchAllClause
			} catch (Exception ex) {
				this.Result = model;
				this.Result.Error = string.Format("Failed to calculate Schedule (NL_Model.NlScheduleItems list) for customer {0}, err: {1}", model.CustomerID, ex);
			}

		}//Execute

	}//class CalculateLoanSchedule
}//ns