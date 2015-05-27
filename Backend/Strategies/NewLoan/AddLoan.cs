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

	public class AddLoan : AStrategy {

		public AddLoan(int userID, int customerID, NL_Model model) {
			this.userID = userID;
			this.customerID = customerID;
			Model = model;
			Loader = new NL_Loader(Model);
		}//constructor

		public override string Name { get { return "AddLoan"; } }

		/// <exception cref="NL_ExceptionInputDataInvalid">Condition. </exception>
		/// /// <exception cref="NL_ExceptionLoanExists">Condition. </exception>
		/// <exception cref="NL_ExceptionOfferNotValid">Condition. </exception>
		/// <exception cref="Exception">Add loan failed: {0}</exception>
		public override void Execute() {

			Log.Debug("------------------ customer {0}------------------------", this.customerID);

			Console.WriteLine(Model.PacnetTransaction);

			string message;

			OfferForLoan dataForLoan = DB.FillFirst<OfferForLoan>("NL_OfferForLoan", CommandSpecies.StoredProcedure, new QueryParameter("CustomerID", this.customerID));

			if (dataForLoan == null) {
				message = string.Format("No valid offer found. Customer {0} ", this.customerID);
				Log.Alert(message);
				throw new NL_ExceptionOfferNotValid(message);
			}

			Log.Debug(dataForLoan.ToString());

			SafeReader sr = DB.GetFirst(string.Format("SELECT LoanID FROM NL_Loans WHERE OfferID={0}", dataForLoan.OfferID));

			if (sr["LoanID"] > 0) {
				message = string.Format("Loan for customer {0} and offer {1} exists", this.customerID, dataForLoan.OfferID);
				Log.Alert(message);
				throw new NL_ExceptionLoanExists(message);
			}

			// input validation
			if (Model.Loan == null) {
				message = string.Format("Expected input data not found (NL_Model initialized by: Loan.OldLoanID, Loan.InitialLoanAmount, Loan.Refnum). Customer {0}", this.customerID);
				Log.Alert(message);
				throw new NL_ExceptionInputDataInvalid(message);
			}

			if (Model.FundTransfer == null) {
				message = string.Format("Expected input data not found (NL_Model initialized by: FundTransfer). Customer {0}", this.customerID);
				Log.Alert(message);
				throw new NL_ExceptionInputDataInvalid(message);
			}

			if (Model.LoanHistory == null) {
				message = string.Format("Expected input data not found (NL_Model initialized by: LoanHistory.AgreementModel). Customer {0}", this.customerID);
				Log.Alert(message);
				throw new NL_ExceptionInputDataInvalid(message);
			}

			if (Model.LoanAgreements == null) {
				message = string.Format("Expected input data not found (NL_Model initialized by: LoanAgreements list). Customer {0}", this.customerID);
				Log.Alert(message);
				throw new NL_ExceptionInputDataInvalid(message);
			}

			// complete other validations here

			/*** 
			//CHECK "Enough Funds" (uncomment WHEN old BE REMOVED from \App\PluginWeb\EzBob.Web\Code\LoanCreator.cs, method CreateLoan method)
			  
			VerifyEnoughAvailableFunds enoughAvailableFunds = new VerifyEnoughAvailableFunds(Model.Loan.InitialLoanAmount);
			enoughAvailableFunds.Execute();
			if (!enoughAvailableFunds.HasEnoughFunds) {
				Log.Alert("No enough funds for loan: customer {0}; offer {1}", this.customerID, dataForLoan.Offer.OfferID);
			}
			****/

			// TODO check usage of DefaultRepaymentPeriod from LoanSource

			/**
			 - loan
			 - fees
			 - history
			 - schedules
			 - broker comissions ? - update NL
			 - fund transfer
			 - pacnet transaction
			 */
			var pconn= DB.GetPersistent();

			int fundTransferID = 0;
			int setupFeeID = NL_Loader.LoanFeeTypes()
				.First(s => s.LoanFeeType.StartsWith("SetupFee"))
				.LoanFeeTypeID;
			int liveLoanStatusID = NL_Loader.LoanStatuses()
				.First(s => s.LoanStatus.StartsWith("Live"))
				.LoanStatusID;

			try {
				Model.Loan.IssuedTime = DateTime.UtcNow;
				Model.Loan.CreationTime = DateTime.UtcNow;

				// 1.method argument 2. from LoanLegals 3.from the offer
				if (Model.Loan.InitialLoanAmount != dataForLoan.LoanLegalAmount) {
					Log.Alert("Mismatch InitialLoanAmount param amount {0}, LoanLegalAmount {1}, loan: {2}", Model.Loan.InitialLoanAmount, dataForLoan.LoanLegalAmount, Model.Loan.Refnum);
				} // by default, use method argument amount value

				Model.Loan.RepaymentCount = dataForLoan.LoanLegalRepaymentPeriod;
				Model.Loan.LoanStatusID = liveLoanStatusID;

				// from offer 
				Model.Loan.OfferID = dataForLoan.OfferID;
				Model.Loan.LoanTypeID = dataForLoan.LoanTypeID;
				Model.Loan.RepaymentIntervalTypeID = dataForLoan.RepaymentIntervalTypeID;
				Model.Loan.LoanSourceID = dataForLoan.LoanSourceID;
				Model.Loan.InterestRate = dataForLoan.MonthlyInterestRate;
				Model.Loan.InterestOnlyRepaymentCount = dataForLoan.InterestOnlyRepaymentCount;
				Model.Loan.Position = 1; //TODO set a real value

				Log.Debug(Model.Loan.ToString());

				pconn.BeginTransaction();

				// 1. loan
				this.LoanID = DB.ExecuteScalar<int>(pconn, "NL_LoansSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter("Tbl", Model.Loan));

				Log.Debug("NL_LoansSave: LoanID: {0}", this.LoanID);

				// setup fee
				var feeCalc = new SetupFeeCalculator(dataForLoan.SetupFeePercent, dataForLoan.BrokerSetupFeePercent);

				// 2. fees
				NL_LoanFees setupFee = new NL_LoanFees();
				setupFee.LoanID = this.LoanID;
				setupFee.LoanFeeTypeID = setupFeeID;
				setupFee.Amount = feeCalc.Calculate(Model.Loan.InitialLoanAmount);
				setupFee.AssignTime = DateTime.UtcNow;
				setupFee.CreatedTime = setupFee.AssignTime;
				setupFee.DisabledTime = null;

				Log.Debug(setupFee.ToString());

				int setupfeeID = DB.ExecuteScalar<int>(pconn, "NL_LoanFeesSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter("Tbl", setupFee));

				Log.Debug("NL_LoanFeesSave: LoanID: {0}, setupfeeID: {1}", this.LoanID, setupfeeID);

				// done in controller. When old loan removed: check if this is the broker's customer, calc broker fees, insert into LoanBrokerCommission
				var brokerComissions = feeCalc.CalculateBrokerFee(Model.Loan.InitialLoanAmount);
				if (brokerComissions > 0) {
					DB.ExecuteNonQuery(string.Format("UPDATE dbo.LoanBrokerCommission SET NLLoanID = {0} WHERE LoanID = {1}", this.LoanID, Model.Loan.OldLoanID));
				}

				// 3. history
				Model.LoanHistory.LoanID = this.LoanID;
				Model.LoanHistory.UserID = this.userID;
				Model.LoanHistory.LoanLegalID = dataForLoan.LoanLegalID;
				Model.LoanHistory.Amount = Model.Loan.InitialLoanAmount;
				Model.LoanHistory.RepaymentCount = Model.Loan.RepaymentCount;
				Model.LoanHistory.InterestRate = Model.Loan.InterestRate;
				Model.LoanHistory.EventTime = DateTime.UtcNow;
				Model.LoanHistory.Description = "add loan ID " + this.LoanID;
				Model.LoanHistory.AgreementModel = Model.LoanHistory.AgreementModel;

				Log.Debug(Model.LoanHistory.ToString());

				int historyID = DB.ExecuteScalar<int>(pconn, "NL_LoanHistorySave", CommandSpecies.StoredProcedure, DB.CreateTableParameter("Tbl", Model.LoanHistory));

				Log.Debug("NL_LoanHistorySave: LoanID: {0}, LoanHistoryID: {1}", this.LoanID, historyID);

				// 4. loan agreements
				foreach (NL_LoanAgreements agreement in Model.LoanAgreements) {
					agreement.LoanHistoryID = historyID;
					Log.Debug(agreement.ToString());
				}
				DB.ExecuteNonQuery(pconn, "NL_LoanAgreementsSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanAgreements>("Tbl", Model.LoanAgreements));

				//	string reptype = NL_Loader.RepaymentIntervalTypes().First(s => s.RepaymentIntervalTypeID == this.model.Loan.RepaymentIntervalTypeID).RepaymentIntervalType;

				// 5. schedules
				LoanCalculatorModel calcModel = new LoanCalculatorModel {
					LoanAmount = Model.Loan.InitialLoanAmount,
					LoanIssueTime = Model.Loan.IssuedTime,
					RepaymentIntervalType = RepaymentIntervalTypes.Month, // TODO change to real value
					RepaymentCount = Model.Loan.RepaymentCount,   //6
					MonthlyInterestRate = Model.Loan.InterestRate, // 0.1m,
					InterestOnlyRepayments = Model.Loan.InterestOnlyRepaymentCount ?? 0
				};

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

				Log.Debug("calcModel: " + calcModel.ToString());

				BankLikeLoanCalculator nlCalculator = new BankLikeLoanCalculator(calcModel);

				List<ScheduledItemWithAmountDue> shedules = nlCalculator.CreateScheduleAndPlan();
				Log.Debug(shedules.ToString());

				List<NL_LoanSchedules> scheduleItems = new List<NL_LoanSchedules>();
				foreach (var s in shedules) {
					NL_LoanSchedules sch = new NL_LoanSchedules();
					sch.InterestRate = s.InterestRate;
					sch.PlannedDate = s.Date;
					sch.Position = s.Position;
					sch.Principal = s.Principal;
					sch.LoanHistoryID = historyID;
					scheduleItems.Add(sch);
					Log.Debug(sch.ToString());
				}

				DB.ExecuteNonQuery(pconn, "NL_LoanSchedulesSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanSchedules>("Tbl", scheduleItems));

				// 5. Fund Transfer 
				Model.FundTransfer.LoanID = this.LoanID;
				fundTransferID = DB.ExecuteScalar<int>(pconn, "NL_FundTransfersSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter("Tbl", Model.FundTransfer));

				Log.Debug("NL_FundTransfersSave: LoanID: {0}, fundTransferID: {1}", this.LoanID, fundTransferID);

				pconn.Commit();

				// ReSharper disable once CatchAllClause
			} catch (Exception ex) {

				this.LoanID = 0;
				fundTransferID = 0;

				pconn.Rollback();
				pconn = null;

				Log.Error("Add loan failed: {0}", ex);

				// ReSharper disable once ThrowingSystemException
				throw new Exception("Add loan failed: {0}", ex);
			}

			// 7. Pacnet transaction
			if (Model.PacnetTransaction != null && fundTransferID > 0) {
				List<NL_PacnetTransactionStatuses> pacnetStatuses = NL_Loader.PacnetTransactionStatuses();
				int pacnetTransactionStatusID = (Model.PacnetTransactionStatus != "") ? pacnetStatuses.First(s => s.TransactionStatus.Equals(Model.PacnetTransactionStatus))
					.PacnetTransactionStatusID : pacnetStatuses.First(s => s.TransactionStatus.StartsWith("Unknown")).PacnetTransactionStatusID;
				Model.PacnetTransaction.FundTransferID = fundTransferID;
				Model.PacnetTransaction.PacnetTransactionStatusID = pacnetTransactionStatusID;
				int pacnetTransactionID = DB.ExecuteScalar<int>("NL_PacnetTransactionsSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter("Tbl", Model.PacnetTransaction));
				Log.Debug("NL_PacnetTransactionsSave: LoanID: {0}, pacnetTransactionID: {1}", this.LoanID, pacnetTransactionID);
			}

		}//Execute

		public int LoanID;

		private readonly int userID;
		private readonly int customerID;
		public NL_Loader Loader { get; private set; }
		public NL_Model Model { get; private set; }
	}//class AddLoan
}//ns
