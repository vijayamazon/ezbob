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
	/// Create loan schedule.
	/// Arguments: NL_Model with CustomerID, CalculatorImplementation (BankLikeLoanCalculator|LegacyLoanCalculator), Loan.IssuedTime
	/// If the customer has valid offer, NL_Model Result contains Schedule 
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
				message = string.Format("No valid Customer ID {0} ", model.CustomerID);
				//Log.Alert(message);
				//throw new NL_ExceptionCustomerNotFound(message);
				this.Result.Error = message;
				return;
			}

			// input validation
			if (model.Loan == null) {
				message = string.Format("Expected input data not found (NL_Model initialized by: Loan.InitialLoanAmount, Loan.IssuedTime). Customer {0}", model.CustomerID);
				//Log.Alert(message);
				//throw new NL_ExceptionInputDataInvalid(message);
				this.Result.Error = message;
				return;
			}

			if (model.CalculatorImplementation == null) {
				message = string.Format("Expected input data not found (NL_Model initialized by: CalculatorImplementation (example: model. ).");
				//Log.Alert(message);
				//throw new NL_ExceptionInputDataInvalid(message);
				this.Result.Error = message;
				return;
			}

			OfferForLoan dataForLoan = DB.FillFirst<OfferForLoan>("NL_OfferForLoan", CommandSpecies.StoredProcedure, new QueryParameter("CustomerID", model.CustomerID), new QueryParameter("@Now", DateTime.UtcNow));

			if (dataForLoan == null) {
				message = string.Format("No valid offer found. Customer {0} ", model.CustomerID);
				//Log.Alert(message);
				//throw new NL_ExceptionOfferNotValid(message);
				this.Result.Error = message;
				return;
			}

			Log.Debug(dataForLoan.ToString());

			try {

				// from offer 
				model.Loan.RepaymentCount = dataForLoan.LoanLegalRepaymentPeriod;
				model.Loan.InitialLoanAmount = dataForLoan.LoanLegalAmount;
				model.Loan.RepaymentIntervalTypeID = dataForLoan.RepaymentIntervalTypeID;
				model.Loan.InterestRate = dataForLoan.MonthlyInterestRate;
				model.Loan.InterestOnlyRepaymentCount = dataForLoan.InterestOnlyRepaymentCount;

				Log.Debug(model.Loan.ToString());

				// 5. schedules
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
					string[] stringSeparator = {","};
					char[] removeChar = {','};
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

				List<ScheduledItemWithAmountDue> shedules = nlCalculator.CreateScheduleAndPlan();

				Log.Debug("SCHEDULES primary= {0}", shedules);

				// setup fee
				var feeCalculator = new SetupFeeCalculator(dataForLoan.SetupFeePercent, dataForLoan.BrokerSetupFeePercent);
				decimal setupFeeAmount = feeCalculator.Calculate(model.Loan.InitialLoanAmount);

				Log.Debug("setupFeeAmount: {0}", setupFeeAmount);

				// add setup fees if distributed
				if (dataForLoan.DistributedSetupFeePercent > 0) {

					decimal distributedFeeAmount = setupFeeAmount * dataForLoan.DistributedSetupFeePercent;
					int schedulesCount = shedules.Count;

					decimal iFee = Decimal.Round(distributedFeeAmount / schedulesCount);
					decimal firstFee = (distributedFeeAmount - iFee * (schedulesCount - 1));

					foreach (ScheduledItemWithAmountDue s1 in shedules) {
						nlCalculatorModel.Fees.Add(new Fee(s1.Date, (schedulesCount > 0) ? firstFee : iFee, FeeTypes.SetupFee));
						schedulesCount = 0; // reset count, because it used as firstFee/iFee flag
					}
				}

				shedules = nlCalculator.CreateScheduleAndPlan();

				Log.Debug("SCHEDULES with fees: {0}", shedules);

				// fill in NL_Model - composite NLScheduleItem
				model.Schedule = new List<NLScheduleItem>();
				foreach (var s in shedules) {

					// set schedule item
					NL_LoanSchedules iItem = new NL_LoanSchedules() {
						InterestRate = s.InterestRate,
						PlannedDate = s.Date,
						Position = s.Position,
						Principal = s.Principal,
						LoanScheduleStatusID = (int)NLScheduleStatuses.StillToPay
					};

					// set appropriate setup fee item
					NL_LoanFees distributedFeeItem = null;
					var fee = nlCalculatorModel.Fees.FirstOrDefault(f => f.AssignDate == s.Date && f.FType == FeeTypes.SetupFee);
					if (fee != null) {
						distributedFeeItem = new NL_LoanFees() {
							Amount = fee.Amount,
							AssignTime = fee.AssignDate,
							Notes = "distributed setup fee",
							LoanFeeTypeID = (int)FeeTypes.SetupFee
						};
					}

					model.Schedule.Add(new NLScheduleItem() { ScheduleItem = iItem, Fee = distributedFeeItem });
				}

				// set APR
				model.APR = nlCalculator.APRCalculate(setupFeeAmount, model.Loan.IssuedTime);

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