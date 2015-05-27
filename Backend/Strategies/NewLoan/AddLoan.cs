namespace Ezbob.Backend.Strategies.NewLoan {
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Data.SqlClient;
	using System.Linq;
	using DbConstants;
	using Ezbob.Backend.CalculateLoan.LoanCalculator;
	using Ezbob.Backend.CalculateLoan.Models;
	using Ezbob.Backend.CalculateLoan.Models.Helpers;
	using Ezbob.Backend.Models.NewLoan;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Database;
	using PaymentServices.Calculators;

	public class AddLoan : AStrategy {

		public AddLoan(int userID, int customerID, NL_Model model) {
			this.userID = userID;
			this.customerID = customerID;
			this.model = model;
			this.loader = new NL_Loader(this.model);
		}//constructor

		public override string Name { get { return "AddLoan"; } }

		public override void Execute() {
			/**
			 - loan
			 - history
			 - schedules
			 - fees
			 - broker comissions ? - update NL
			 - fund transfer
			 - pacnet transaction
			 */
			int fundTransferID = 0;
			try {
				Log.Debug("------------------Adding NL for customer {0}------------------------", this.customerID);

				this.model.Loan.IssuedTime = DateTime.UtcNow;
				this.model.Loan.CreationTime = DateTime.UtcNow;

				NL_Model dataForLoan = this.loader.OfferForLoan();

				// 1.method argument 2. from LoanLegals 3.from the offer
				if (this.model.Loan.InitialLoanAmount != dataForLoan.LoanLegalAmount) {
					Log.Alert("Mismatch InitialLoanAmount param amount {0}, LoanLegalAmount {1}, loan: {2}", this.model.Loan.InitialLoanAmount, dataForLoan.LoanLegalAmount, this.model.Loan.Refnum);
				} // by default, use method argument amount value

				this.model.Loan.RepaymentCount = dataForLoan.LoanLegalRepaymentPeriod;
				this.model.Loan.LoanStatusID = NL_Loader.LoanStatuses().First(s => s.LoanStatus.Equals("Live")).LoanStatusID;

				// from offer 
				this.model.Loan.OfferID = dataForLoan.Offer.OfferID;
				this.model.Loan.LoanTypeID = dataForLoan.Offer.LoanTypeID;
				this.model.Loan.RepaymentIntervalTypeID = dataForLoan.Offer.RepaymentIntervalTypeID;
				this.model.Loan.LoanSourceID = dataForLoan.Offer.LoanSourceID;
				this.model.Loan.InterestRate = dataForLoan.Offer.MonthlyInterestRate;
				this.model.Loan.InterestOnlyRepaymentCount = dataForLoan.Offer.InterestOnlyRepaymentCount;

				Log.Debug(this.model.Loan.ToString());
	
				DB.GetPersistent().BeginTransaction();

				this.LoanID = DB.ExecuteScalar<int>("NL_LoansSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter("Tbl", this.model.Loan));

				Log.Debug("NL_LoansSave: LoanID: {0}", this.LoanID);

				// history
				this.model.LoanHistory.LoanID = this.LoanID;
				this.model.LoanHistory.EventTime = DateTime.UtcNow;
				this.model.LoanHistory.Description = "add loan ID " + this.LoanID;
				this.model.LoanHistory.LoanID = this.LoanID;
				this.model.LoanHistory.AgreementModel = this.model.LoanHistory.AgreementModel;
				this.model.LoanHistory.UserID = this.userID;
				this.model.LoanHistory.LoanLegalID = dataForLoan.LoanLegal.LoanLegalID;

				Log.Debug(this.model.LoanHistory.ToString());

				int historyID = DB.ExecuteScalar<int>("NL_LoanHistorySave", CommandSpecies.StoredProcedure, DB.CreateTableParameter("Tbl", this.model.LoanHistory));

				Log.Debug("NL_LoanHistorySave: LoanID: {0}, LoanHistoryID: {1}", this.LoanID, historyID);

				// agreements
				foreach (NL_LoanAgreements agreement in this.model.LoanHistory.LoanAgreements) {
					agreement.LoanHistoryID = historyID;
					Log.Debug(agreement.ToString());
				}
				DB.ExecuteNonQuery("NL_LoanAgreementsSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanAgreements>("Tbl", this.model.LoanHistory.LoanAgreements));

				// setup fee
				var feeCalc = new SetupFeeCalculator(dataForLoan.Offer.SetupFeePercent, dataForLoan.Offer.BrokerSetupFeePercent);

				NL_LoanFees setupFee = new NL_LoanFees();
				setupFee.LoanID = this.LoanID;
				setupFee.LoanFeeTypeID = NL_Loader.LoanFeeTypes().First(s => s.LoanFeeType.Equals("SetupFee")).LoanFeeTypeID;
				setupFee.Amount = feeCalc.Calculate(this.model.Loan.InitialLoanAmount);
				setupFee.AssignTime = DateTime.UtcNow;
				setupFee.CreatedTime = setupFee.AssignTime;

				Log.Debug(setupFee.ToString());

				int setupfeeID = DB.ExecuteScalar<int>("NL_LoanFeesSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter("Tbl", setupFee));

				Log.Debug("NL_LoanFeesSave: LoanID: {0}, LoanFeeID: {1}", this.LoanID, setupfeeID);

				// done in controller, check if this is the broker's customer
				var brokerComissions = feeCalc.CalculateBrokerFee(this.model.Loan.InitialLoanAmount);
				if (brokerComissions > 0) {
					DB.ExecuteNonQuery("UPDATE dbo.LoanBrokerCommission SET NLLoanID = @nlloanid WHERE LoanID = @loanID;",
						new QueryParameter("@nlloanid", this.LoanID),
						new QueryParameter("@loanID", this.model.Loan.OldLoanID)
					);
				}

				string reptype = NL_Loader.RepaymentIntervalTypes().First(s => s.RepaymentIntervalTypeID == this.model.Loan.RepaymentIntervalTypeID).RepaymentIntervalType;

				// use for schedules offer.DiscountPlanID + plan entries
				LoanCalculatorModel calcModel = new LoanCalculatorModel {
					LoanAmount = this.model.Loan.InitialLoanAmount,
					LoanIssueTime = this.model.Loan.IssuedTime, //new DateTime(2015, 1, 31, 14, 15, 16, DateTimeKind.Utc),
					RepaymentIntervalType = RepaymentIntervalTypes.Month, // TODO change to real value
					RepaymentCount = this.model.Loan.RepaymentCount,   //6
					MonthlyInterestRate = this.model.Loan.InterestRate, // 0.1m,
					InterestOnlyRepayments = this.model.Loan.InterestOnlyRepaymentCount ?? 0
				};

				//calcModel.SetDiscountPlan(0, 0, -0.5m, 0.6m); 
				if (dataForLoan.DiscountPlan != null) {
					string[] stringSeparator = {","};
					string[] result = dataForLoan.DiscountPlan.Split(stringSeparator, StringSplitOptions.None);
					decimal[] dpe = {};
					var i = 0;
					foreach (string s in result) {
						dpe.SetValue(Decimal.Parse(s), i++);
					}
					calcModel.SetDiscountPlan(dpe);

					Log.Debug(dataForLoan.DiscountPlan);
				}
					

				BankLikeLoanCalculator nlCalculator = new BankLikeLoanCalculator(calcModel);

				List<ScheduledItemWithAmountDue> shedules = nlCalculator.CreateScheduleAndPlan();
				List<NL_LoanSchedules> scheduleItems = new List<NL_LoanSchedules>();
				foreach (var s in shedules) {
					NL_LoanSchedules sch = new NL_LoanSchedules();
					sch.InterestRate = s.InterestRate;
					sch.PlannedDate = s.Date;
					sch.Position = s.Position;
					sch.Principal = s.Principal;
					scheduleItems.Add(sch);
				}
				Log.Debug(shedules.ToString());

				DB.ExecuteNonQuery("NL_LoanSchedulesSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanSchedules>("Tbl", scheduleItems));

				this.model.FundTransfer.LoanID = this.LoanID;
				fundTransferID = DB.ExecuteScalar<int>("NL_FundTransfersSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter("Tbl", this.model.FundTransfer));

				Log.Debug("NL_FundTransfersSave: LoanID: {0}, fundTransferID: {1}", this.LoanID, fundTransferID);

				DB.GetPersistent().Commit();

			} catch (Exception e) {
				
				this.LoanID = 0;
				
				DB.GetPersistent().Rollback();

				Log.Error("Add loan failed: {0}", e);

				throw;
			} 

			if (this.model.PacnetTransaction != null) {
				this.model.PacnetTransaction.FundTransferID = fundTransferID;
				int pacnetTransactionID = DB.ExecuteScalar<int>("NL_PacnetTransactionsSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter("Tbl", this.model.PacnetTransaction));

				Log.Debug("pacnetTransactionID: LoanID: {0}, pacnetTransactionID: {1}", this.LoanID, pacnetTransactionID);
			}

		}//Execute


		private int userID;
		private int customerID;
		public int LoanID;
		private NL_Model model;
		private NL_Loader loader;
	}//class AddLoan
}//ns
