namespace Ezbob.Backend.Strategies.NewLoan {
	using System;
	using System.Collections.Generic;
	using DbConstants;
	using Ezbob.Backend.CalculateLoan.LoanCalculator;
	using Ezbob.Backend.CalculateLoan.Models;
	using Ezbob.Backend.CalculateLoan.Models.Helpers;
	using Ezbob.Backend.Models.NewLoan;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Database;
	using PaymentServices.Calculators;

	public class CalculateLoanSchedule<T> : AStrategy {

		public CalculateLoanSchedule(T t, NL_Model nlModel) {
			this.t = t;
			NLModel = nlModel;
		}//constructor

		public override string Name { get { return "CalculateLoanSchedule"; } }

		/// <exception cref="NL_ExceptionCustomerNotFound">Condition. </exception>
		/// <exception cref="NL_ExceptionInputDataInvalid">Condition. </exception>
		/// <exception cref="NL_ExceptionOfferNotValid">Condition. </exception>
		public override void Execute() {

			Log.Debug("------------------ customer {0}------------------------", NLModel.CustomerID);

			string message;

			if (NLModel.CustomerID == 0) {
				message = string.Format("No valid Customer ID {0} ", NLModel.CustomerID);
				Log.Alert(message);
				NLModel.Error = message;
				throw new NL_ExceptionCustomerNotFound(message);
			}

			// input validation
			if (NLModel.Loan == null) {
				message = string.Format("Expected input data not found (NL_Model initialized by: Loan.InitialLoanAmount, IssuedTime). Customer {0}", NLModel.CustomerID);
				Log.Alert(message);
				NLModel.Error = message;
				throw new NL_ExceptionInputDataInvalid(message);
			}

			OfferForLoan dataForLoan = DB.FillFirst<OfferForLoan>("NL_OfferForLoan", CommandSpecies.StoredProcedure, new QueryParameter("CustomerID", NLModel.CustomerID), new QueryParameter("@Now", DateTime.UtcNow));

			if (dataForLoan == null) {
				message = string.Format("No valid offer found. Customer {0} ", NLModel.CustomerID);
				Log.Alert(message);
				NLModel.Error = message;
				throw new NL_ExceptionOfferNotValid(message);
			}

			Log.Debug(dataForLoan.ToString());

			try {
				
				// from offer 
				NLModel.Loan.RepaymentCount = dataForLoan.LoanLegalRepaymentPeriod;
				NLModel.Loan.RepaymentIntervalTypeID = dataForLoan.RepaymentIntervalTypeID;
				NLModel.Loan.InterestRate = dataForLoan.MonthlyInterestRate;
				NLModel.Loan.InterestOnlyRepaymentCount = dataForLoan.InterestOnlyRepaymentCount;

				Log.Debug(NLModel.Loan.ToString());

				// setup fee
				var feeCalc = new SetupFeeCalculator(dataForLoan.SetupFeePercent, dataForLoan.BrokerSetupFeePercent);
				// 2. fees
				NL_LoanFees setupFee = new NL_LoanFees();
				setupFee.Amount = feeCalc.Calculate(NLModel.Loan.InitialLoanAmount);

				Log.Debug(setupFee.ToString());

				// 5. schedules
				LoanCalculatorModel calcModel = new LoanCalculatorModel {
					LoanAmount = NLModel.Loan.InitialLoanAmount,
					LoanIssueTime = NLModel.Loan.IssuedTime,
					RepaymentIntervalType = (RepaymentIntervalTypes)Enum.ToObject(typeof(RepaymentIntervalTypesId), NLModel.Loan.RepaymentIntervalTypeID),
					RepaymentCount = NLModel.Loan.RepaymentCount,
					MonthlyInterestRate = NLModel.Loan.InterestRate,
					InterestOnlyRepayments = NLModel.Loan.InterestOnlyRepaymentCount ?? 0
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
					calcModel.SetDiscountPlan(dpe);
				}

				Log.Debug("calcModel: " + calcModel);
				
				ALoanCalculator nlCalculator = new LegacyLoanCalculator(calcModel);
				if (this.t.GetType() == typeof(BankLikeLoanCalculator)) {
					nlCalculator = new BankLikeLoanCalculator(calcModel);
				}

				List<ScheduledItemWithAmountDue> shedules = nlCalculator.CreateScheduleAndPlan();
				Log.Debug(shedules.ToString());

				NLModel.Schedule = new List<NL_LoanSchedules>();

				foreach (var s in shedules) {
					NL_LoanSchedules sch = new NL_LoanSchedules();
					sch.InterestRate = s.InterestRate;
					sch.PlannedDate = s.Date;
					sch.Position = s.Position;
					sch.Principal = s.Principal;
					NLModel.Schedule.Add(sch);

					Log.Debug(sch.ToString());
				}

				NLModel.APR = nlCalculator.APRCalculate(setupFee.Amount, NLModel.Loan.IssuedTime);

			} catch (Exception ex) {
				message = string.Format("Failed to write NL_Loan for customer {0}, oldLoanID {1}, err: {2}", NLModel.CustomerID, NLModel.Loan.OldLoanID, ex);
			}

		}//Execute

		public int LoanID;
		private readonly T t;
		public NL_Model NLModel { get; private set; }


	}//class CalculateLoanSchedule
}//ns