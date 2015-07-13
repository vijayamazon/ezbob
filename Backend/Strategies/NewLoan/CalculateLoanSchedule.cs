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


	public class CalculateLoanSchedule : AStrategy {

		public CalculateLoanSchedule(NL_Model nlModel) {
			model = nlModel;
		}//constructor

		public override string Name { get { return "CalculateLoanSchedule"; } }
		public NL_Model Result;

		/// <exception cref="NL_ExceptionCustomerNotFound">Condition. </exception>
		/// <exception cref="NL_ExceptionInputDataInvalid">Condition. </exception>
		/// <exception cref="NL_ExceptionOfferNotValid">Condition. </exception>
		public override void Execute() {

			Log.Debug("------------------ customer {0}------------------------", model.CustomerID);

			string message;

			if (model.CustomerID == 0) {
				message = string.Format("No valid Customer ID {0} ", model.CustomerID);
				//Log.Alert(message);
				model.Error = message;
				throw new NL_ExceptionCustomerNotFound(message);
			}

			// input validation
			if (model.Loan == null) {
				message = string.Format("Expected input data not found (NL_Model initialized by: Loan.InitialLoanAmount, Loan.IssuedTime). Customer {0}", model.CustomerID);
				//Log.Alert(message);
				model.Error = message;
				throw new NL_ExceptionInputDataInvalid(message);
			}

			if (model.CalculatorImplementation == null) {
				message = string.Format("Expected input data not found (NL_Model initialized by: CalculatorImplementation (example: model. ).");
				//Log.Alert(message);
				model.Error = message;
				throw new NL_ExceptionInputDataInvalid(message);
			}

			OfferForLoan dataForLoan = DB.FillFirst<OfferForLoan>("NL_OfferForLoan", CommandSpecies.StoredProcedure, new QueryParameter("CustomerID", model.CustomerID), new QueryParameter("@Now", DateTime.UtcNow));

			if (dataForLoan == null) {
				message = string.Format("No valid offer found. Customer {0} ", model.CustomerID);
				//Log.Alert(message);
				model.Error = message;
				throw new NL_ExceptionOfferNotValid(message);
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

				// setup fee
				var feeCalculator = new SetupFeeCalculator(dataForLoan.SetupFeePercent, dataForLoan.BrokerSetupFeePercent);
				decimal setupFeeAmount = feeCalculator.Calculate(model.Loan.InitialLoanAmount);

				Log.Debug(setupFeeAmount);

				// 5. schedules
				LoanCalculatorModel calcModel = new LoanCalculatorModel {
					LoanIssueTime = model.Loan.IssuedTime, // input
					RepaymentIntervalType = (RepaymentIntervalTypes)Enum.ToObject(typeof(RepaymentIntervalTypesId), model.Loan.RepaymentIntervalTypeID), // offer|LoanLegal
					LoanAmount = model.Loan.InitialLoanAmount, // offer|LoanLegal
					RepaymentCount = model.Loan.RepaymentCount,  //  offer|LoanLegal
					MonthlyInterestRate = model.Loan.InterestRate,  //  offer|LoanLegal
					InterestOnlyRepayments = model.Loan.InterestOnlyRepaymentCount ?? 0  //  offer|LoanLegal
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
				
				if (model.CalculatorImplementation.GetType() == typeof(BankLikeLoanCalculator)) {
					nlCalculator = new BankLikeLoanCalculator(calcModel);
				}

				List<ScheduledItemWithAmountDue> shedules = nlCalculator.CreateScheduleAndPlan();

				Log.Debug(shedules.ToString());

				model.Schedule = new List<NL_LoanSchedules>();

		var scheduleStatuses	=	Enum.GetValues(typeof(NLScheduleStatuses));

				foreach (var s in shedules) {
					NL_LoanSchedules sch = new NL_LoanSchedules();
					sch.InterestRate = s.InterestRate;
					sch.PlannedDate = s.Date;
					sch.Position = s.Position;
					sch.Principal = s.Principal;
					//sch.LoanScheduleStatusID = scheduleStatuses

					model.Schedule.Add(sch);
					Log.Debug(sch.ToString());
				}

				model.APR = nlCalculator.APRCalculate(setupFeeAmount, model.Loan.IssuedTime);

			} catch (Exception ex) {
				message = string.Format("Failed to write NL_Loan for customer {0}, oldLoanID {1}, err: {2}", model.CustomerID, model.Loan.OldLoanID, ex);
			}

		}//Execute

		public int LoanID;
		public NL_Model model { get; set; }


	}//class CalculateLoanSchedule
}//ns