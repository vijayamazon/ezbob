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

			OfferForLoan dataForLoan = DB.FillFirst<OfferForLoan>("NL_OfferForLoan", CommandSpecies.StoredProcedure, new QueryParameter("CustomerID", model.CustomerID), new QueryParameter("@Now", model.Loan.IssuedTime));

			if (dataForLoan == null) {
				this.Result.Error = NL_ExceptionOfferNotValid.DefaultMessage;
				return;
			}

			Log.Debug("OfferDataForLoan: {0}", dataForLoan);

			try {
	
				// from offer => Loan
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

				// from offer => Offer
				model.Offer = new NL_Offers() { BrokerSetupFeePercent = dataForLoan.BrokerSetupFeePercent, SetupFeeAddedToLoan = dataForLoan.SetupFeeAddedToLoan };

				// init calculator's model - loan's details
				LoanCalculatorModel nlCalculatorModel = new LoanCalculatorModel {
					LoanIssueTime = model.Loan.IssuedTime, // input arg
					RepaymentIntervalType = (RepaymentIntervalTypes)Enum.ToObject(typeof(RepaymentIntervalTypesId), model.Loan.RepaymentIntervalTypeID), // offer|LoanLegal
					LoanAmount = model.Loan.InitialLoanAmount, // offer|LoanLegal
					RepaymentCount = model.Loan.RepaymentCount, //  offer|LoanLegal
					MonthlyInterestRate = model.Loan.InterestRate, //  offer|LoanLegal
					InterestOnlyRepayments = model.Loan.InterestOnlyRepaymentCount, //  offer|LoanLegal
				};

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
					decimal[] dpe = new decimal[result.Length];
					var i = 0;
					foreach (string s in result) {
						dpe.SetValue(Decimal.Parse(s), i++);
					}
					nlCalculatorModel.SetDiscountPlan(dpe);
				}

				Log.Debug("nlCalculator model: {0}", nlCalculatorModel);

				// init schedule - dates, intervals
				List<ScheduledItem> shedules = nlCalculator.CreateSchedule();

				Log.Debug("SCHEDULES primary = {0}", shedules);

				// FEES: untill fees 2.0 implemented, suppose: Percent == OneTimePartPercent

				// get offers' fees
				List<NL_OfferFees> offerFees = DB.Fill<NL_OfferFees>("NL_OfferFeesGet", CommandSpecies.StoredProcedure, new QueryParameter("@OfferID", dataForLoan.OfferID));

				if (offerFees != null) {

					var setupFee = offerFees.FirstOrDefault(f => f.LoanFeeTypeID == (int)FeeTypes.SetupFee);
					var servicingFee = offerFees.FirstOrDefault(f => f.LoanFeeTypeID == (int)FeeTypes.ServicingFee); // equal to "setup spreaded"

					// calculator's model - setup fee
					if (setupFee != null) {

						var feeCalculator = new SetupFeeCalculator(setupFee.PercentOfIssued, dataForLoan.BrokerSetupFeePercent);

						decimal setupFeeAmount = feeCalculator.Calculate(model.Loan.InitialLoanAmount);
						model.BrokerComissions = feeCalculator.CalculateBrokerFee(model.Loan.InitialLoanAmount);

						Log.Debug("setupFeeAmount: {0}, brokerComissions: {1}", setupFeeAmount, model.BrokerComissions);

						nlCalculatorModel.Fees.Add(new Fee(model.Loan.IssuedTime, setupFeeAmount, FeeTypes.SetupFee));
					}

					// calculator's model - servicing fees
					if (servicingFee != null) {

						var feeCalculator = new SetupFeeCalculator(servicingFee.PercentOfIssued, dataForLoan.BrokerSetupFeePercent);

						decimal servicingFeeAmount = feeCalculator.Calculate(model.Loan.InitialLoanAmount);
						model.BrokerComissions = feeCalculator.CalculateBrokerFee(model.Loan.InitialLoanAmount);

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

				// get schedules and fees
				List<ScheduledItemWithAmountDue> sheduleFeeList = nlCalculator.CreateScheduleAndPlan();

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

					//  add appropriate "spreaded" fees model.Fees list
					var servicingFeeSchedule = nlCalculatorModel.Fees.FirstOrDefault(f => f.AssignDate.Date == s.Date.Date && f.FType == FeeTypes.ServicingFee);

					if (servicingFeeSchedule != null) {
						model.Fees.Add(new NLFeeItem() {
							Fee = new NL_LoanFees() {
								Amount = servicingFeeSchedule.Amount,
								AssignTime = servicingFeeSchedule.AssignDate,
								Notes = "spreaded (servicing) fee",
								LoanFeeTypeID = (int)FeeTypes.ServicingFee
							}
						});
					} // "spreaded" fees
				}

				// setup fee
				var setupFeeSchedule = nlCalculatorModel.Fees.FirstOrDefault(f => f.FType == FeeTypes.SetupFee);
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
				model.APR = nlCalculator.CalculateApr(model.Loan.IssuedTime);

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