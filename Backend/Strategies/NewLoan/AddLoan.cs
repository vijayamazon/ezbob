namespace Ezbob.Backend.Strategies.NewLoan {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using DbConstants;
	using Ezbob.Backend.CalculateLoan.LoanCalculator;
	using Ezbob.Backend.CalculateLoan.Models;
	using Ezbob.Backend.CalculateLoan.Models.Helpers;
	using Ezbob.Backend.Models.NewLoan;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Database;
	using MailApi;
	using PaymentServices.Calculators;

	public class AddLoan : AStrategy {

		public AddLoan(NL_Model nlModel) {
			this.userID = nlModel.UserID;
			this.customerID = nlModel.CustomerID;
			NLModel = nlModel;
			Loader = new NL_Loader(NLModel);

			this.emailToAddress = "elinar@ezbob.com";
			this.emailFromAddress = "elinar@ezbob.com";
			this.emailFromName = "ezbob-system";
		}//constructor

		public override string Name { get { return "AddLoan"; } }

		//AddLoanOptions ??????????????
		//var addLoanOptions = new AddLoanOptions(new NL_LoanOptions)

		/// <exception cref="NL_ExceptionInputDataInvalid">Condition. </exception>
		/// <exception cref="NL_ExceptionLoanExists">Condition. </exception>
		/// <exception cref="NL_ExceptionOfferNotValid">Condition. </exception>
		/// <exception cref="Exception">Add loan failed: {0}</exception>
		/// <exception cref="NL_ExceptionCustomerNotFound">Condition. </exception>
		public override void Execute() {

			Log.Debug("------------------ customer {0}------------------------", this.customerID);

			string message;

			if (this.customerID == 0) {
				message = string.Format("No valid Customer ID {0} ", this.customerID);
				Log.Alert(message);
				throw new NL_ExceptionCustomerNotFound(message);
			}

			OfferForLoan dataForLoan = DB.FillFirst<OfferForLoan>("NL_OfferForLoan", CommandSpecies.StoredProcedure, 
				new QueryParameter("CustomerID", this.customerID),
				new QueryParameter("@Now", DateTime.UtcNow));

			if (dataForLoan == null) {
				message = string.Format("No valid offer found. Customer {0} ", this.customerID);
				Log.Alert(message);
				throw new NL_ExceptionOfferNotValid(message);
			}

			Log.Debug(dataForLoan.ToString());

			// loan for the offer exists
			SafeReader sr = DB.GetFirst(string.Format("SELECT LoanID FROM NL_Loans WHERE OfferID={0}", dataForLoan.OfferID));
			if (sr["LoanID"] > 0) {
				message = string.Format("Loan for customer {0} and offer {1} exists", this.customerID, dataForLoan.OfferID);
				Log.Alert(message);
				throw new NL_ExceptionLoanExists(message);
			}

			// input validation
			if (NLModel.Loan == null) {
				message = string.Format("Expected input data not found (NL_Model initialized by: Loan.OldLoanID, Loan.InitialLoanAmount, Loan.Refnum). Customer {0}", this.customerID);
				Log.Alert(message);
				throw new NL_ExceptionInputDataInvalid(message);
			}

			if (NLModel.FundTransfer == null) {
				message = string.Format("Expected input data not found (NL_Model initialized by: FundTransfer). Customer {0}", this.customerID);
				Log.Alert(message);
				throw new NL_ExceptionInputDataInvalid(message);
			}

			if (NLModel.LoanHistory == null) {
				message = string.Format("Expected input data not found (NL_Model initialized by: LoanHistory.AgreementModel). Customer {0}", this.customerID);
				Log.Alert(message);
				throw new NL_ExceptionInputDataInvalid(message);
			}

			if (NLModel.LoanAgreements == null) {
				message = string.Format("Expected input data not found (NL_Model initialized by: LoanAgreements list). Customer {0}", this.customerID);
				Log.Alert(message);
				throw new NL_ExceptionInputDataInvalid(message);
			}
			
			// complete other validations here

			/*** 
			//CHECK "Enough Funds" (uncomment WHEN old BE REMOVED from \App\PluginWeb\EzBob.Web\Code\LoanCreator.cs, method CreateLoan method)			  
			VerifyEnoughAvailableFunds enoughAvailableFunds = new VerifyEnoughAvailableFunds(NLModel.Loan.InitialLoanAmount);
			enoughAvailableFunds.Execute();
			if (!enoughAvailableFunds.HasEnoughFunds) {
				Log.Alert("No enough funds for loan: customer {0}; offer {1}", this.customerID, dataForLoan.Offer.OfferID);
			}
			****/

			// TODO check usage of DefaultRepaymentPeriod from LoanSource
			// TODO additional check to be replicated here in master, commits 
			// adding safe check before creating loan that the interest rate is correct 4b6d139658203863dc57cc13748ea49da498ff27
			// add server side validation of repayment period when CreateLoan  b6926a4428919dd0343dd4f8c7ceb0d5d6b1b03d

			/**
			 - loan
			 - fees
			 - history
			 - schedules
			 - broker comissions ? - update NLLoanID
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
			List<NL_LoanSchedules> scheduleItems = new List<NL_LoanSchedules>();
			NL_LoanFees setupFee = new NL_LoanFees();

			try {
				NLModel.Loan.IssuedTime = DateTime.UtcNow;
				NLModel.Loan.CreationTime = DateTime.UtcNow;

				// 1.method argument 2. from LoanLegals 3.from the offer
				if (NLModel.Loan.InitialLoanAmount != dataForLoan.LoanLegalAmount) {
					Log.Alert("Mismatch InitialLoanAmount param amount {0}, LoanLegalAmount {1}, loan: {2}", NLModel.Loan.InitialLoanAmount, dataForLoan.LoanLegalAmount, NLModel.Loan.Refnum);
				} // by default, use method argument amount value

				NLModel.Loan.RepaymentCount = dataForLoan.LoanLegalRepaymentPeriod;
				NLModel.Loan.LoanStatusID = liveLoanStatusID;

				// from offer 
				NLModel.Loan.OfferID = dataForLoan.OfferID;
				NLModel.Loan.LoanTypeID = dataForLoan.LoanTypeID;
				NLModel.Loan.RepaymentIntervalTypeID = dataForLoan.RepaymentIntervalTypeID;
				NLModel.Loan.LoanSourceID = dataForLoan.LoanSourceID;
				NLModel.Loan.InterestRate = dataForLoan.MonthlyInterestRate;
				NLModel.Loan.InterestOnlyRepaymentCount = dataForLoan.InterestOnlyRepaymentCount;
				NLModel.Loan.Position = dataForLoan.LoansCount;

				Log.Debug(NLModel.Loan.ToString());

				pconn.BeginTransaction();

				// 1. loan
				this.LoanID = DB.ExecuteScalar<int>(pconn, "NL_LoansSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter("Tbl", NLModel.Loan));

				Log.Debug("NL_LoansSave: LoanID: {0}", this.LoanID);

				// setup fee
				var feeCalc = new SetupFeeCalculator(dataForLoan.SetupFeePercent, dataForLoan.BrokerSetupFeePercent);

				// 2. fees
				setupFee.LoanID = this.LoanID;
				setupFee.LoanFeeTypeID = setupFeeID;
				setupFee.Amount = feeCalc.Calculate(NLModel.Loan.InitialLoanAmount);
				setupFee.AssignTime = DateTime.UtcNow;
				setupFee.CreatedTime = setupFee.AssignTime;
				setupFee.DisabledTime = null;

				Log.Debug(setupFee.ToString());

				int setupfeeID = DB.ExecuteScalar<int>(pconn, "NL_LoanFeesSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter("Tbl", setupFee));

				Log.Debug("NL_LoanFeesSave: LoanID: {0}, setupfeeID: {1}", this.LoanID, setupfeeID);

				// done in controller. When old loan removed: check if this is the broker's customer, calc broker fees, insert into LoanBrokerCommission
				var brokerComissions = feeCalc.CalculateBrokerFee(NLModel.Loan.InitialLoanAmount);
				if (brokerComissions > 0) {
					DB.ExecuteNonQuery(string.Format("UPDATE dbo.LoanBrokerCommission SET NLLoanID = {0} WHERE LoanID = {1}", this.LoanID, NLModel.Loan.OldLoanID));
				}

				// 3. history
				NLModel.LoanHistory.LoanID = this.LoanID;
				NLModel.LoanHistory.UserID = this.userID;
				NLModel.LoanHistory.LoanLegalID = dataForLoan.LoanLegalID;
				NLModel.LoanHistory.Amount = NLModel.Loan.InitialLoanAmount;
				NLModel.LoanHistory.RepaymentCount = NLModel.Loan.RepaymentCount;
				NLModel.LoanHistory.InterestRate = NLModel.Loan.InterestRate;
				NLModel.LoanHistory.EventTime = DateTime.UtcNow;
				NLModel.LoanHistory.Description = "add loan ID " + this.LoanID;
				NLModel.LoanHistory.AgreementModel = NLModel.LoanHistory.AgreementModel;

				Log.Debug(NLModel.LoanHistory.ToString());

				int historyID = DB.ExecuteScalar<int>(pconn, "NL_LoanHistorySave", CommandSpecies.StoredProcedure, DB.CreateTableParameter("Tbl", NLModel.LoanHistory));

				Log.Debug("NL_LoanHistorySave: LoanID: {0}, LoanHistoryID: {1}", this.LoanID, historyID);

				// 4. loan agreements
				foreach (NL_LoanAgreements agreement in NLModel.LoanAgreements) {
					agreement.LoanHistoryID = historyID;
					Log.Debug(agreement.ToString());
				}
				DB.ExecuteNonQuery(pconn, "NL_LoanAgreementsSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanAgreements>("Tbl", NLModel.LoanAgreements));

				// 5. schedules
				LoanCalculatorModel calcModel = new LoanCalculatorModel {
					LoanAmount = NLModel.Loan.InitialLoanAmount,
					LoanIssueTime = NLModel.Loan.IssuedTime,
					RepaymentIntervalType = (RepaymentIntervalTypes)Enum.ToObject(typeof(RepaymentIntervalTypesId), NLModel.Loan.RepaymentIntervalTypeID),
					RepaymentCount = NLModel.Loan.RepaymentCount,
					MonthlyInterestRate = NLModel.Loan.InterestRate,
					InterestOnlyRepayments = NLModel.Loan.InterestOnlyRepaymentCount ?? 0
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
				NLModel.FundTransfer.LoanID = this.LoanID;
				fundTransferID = DB.ExecuteScalar<int>(pconn, "NL_FundTransfersSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter("Tbl", NLModel.FundTransfer));

				Log.Debug("NL_FundTransfersSave: LoanID: {0}, fundTransferID: {1}", this.LoanID, fundTransferID);

				pconn.Commit();

				// ReSharper disable once CatchAllClause
			} catch (Exception ex) {

				message = string.Format("Failed to write NL_Loan for customer {0}, oldLoanID {1}, err: {2}", this.customerID, NLModel.Loan.OldLoanID, ex);
				this.LoanID = 0;
				pconn.Rollback();

				Log.Error(message);
				SendErrorMail(message, setupFee, scheduleItems);

				// ReSharper disable once ThrowingSystemException
				throw new Exception("Add NL loan failed: {0}", ex);
			}

			// 7. Pacnet transaction
			try {
				if (NLModel.PacnetTransaction != null && fundTransferID > 0) {
					NLModel.PacnetTransaction.FundTransferID = fundTransferID;

					List<NL_PacnetTransactionStatuses> pacnetStatuses = NL_Loader.PacnetTransactionStatuses();
					var pacnetTransactionStatus = pacnetStatuses.Find(s => s.TransactionStatus.Equals(NLModel.PacnetTransactionStatus)) ?? pacnetStatuses.Find(s => s.TransactionStatus.Equals("Unknown"));
					NLModel.PacnetTransaction.PacnetTransactionStatusID = pacnetTransactionStatus.PacnetTransactionStatusID;

					NLModel.PacnetTransaction.PacnetTransactionStatusID = DB.ExecuteScalar<int>("NL_PacnetTransactionsSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter("Tbl", NLModel.PacnetTransaction));

					Log.Debug("NL_PacnetTransactionsSave: LoanID: {0}, pacnetTransactionID: {1}", this.LoanID, NLModel.PacnetTransaction.PacnetTransactionStatusID);
				}
			} catch (Exception e1) {
				message = string.Format("Failed to write NL PacnetTransaction: Customer {0}, oldLoanID {1}, err: {2}", this.customerID, NLModel.Loan.OldLoanID, e1);
				SendErrorMail(message, setupFee, scheduleItems);
				Log.Error(message);
			}
		}//Execute


		private void SendErrorMail(string sMsg, NL_LoanFees setupFee = null, List<NL_LoanSchedules> scheduleItems = null) {
			var message = string.Format(
				"<h3>CustomerID: {0}</h3><p>"
				 + "<h3>Input data</h3>: {1} <br/>"
				 + "<h3>NL_Loan</h3>: {2} <br/>"
				 + "<h3>NL_LoanHistory</h3>: {3} <br/>"
				 + "<h3>NL_LoanFees</h3>: {4} <br/>"
				 + "<h3>NL_LoanSchedules</h3>: {5} <br/>"
				 + "<h3>NL_LoanAgreements</h3>: {6} <br/>"
				 + "<h3>NL_FundTransfer</h3>: {7} <br/>"
				 + "<h3>NL_PacnetTransactionr</h3>: {8} <br/></p>",

				this.customerID
				, HttpUtility.HtmlEncode(NLModel.ToString())
				, HttpUtility.HtmlEncode(NLModel.Loan == null ? "no Loan specified" : NLModel.Loan.ToString())
				, HttpUtility.HtmlEncode(NLModel.LoanHistory == null ? "no LoanHistory specified" : NLModel.LoanHistory.ToString())
				, HttpUtility.HtmlEncode(setupFee == null ? "no LoanFees specified" : setupFee.ToString())
				, HttpUtility.HtmlEncode(scheduleItems == null ? "no scheduleItems specified" : scheduleItems.ToString()) // NL_LoanSchedules
				, HttpUtility.HtmlEncode(NLModel.LoanAgreements == null ? "no LoanAgreements specified" : NLModel.LoanAgreements.ToString()) // NL_LoanAgreements
				, HttpUtility.HtmlEncode(NLModel.FundTransfer == null ? "no FundTransfer specified" : NLModel.FundTransfer.ToString())
				, HttpUtility.HtmlEncode(NLModel.PacnetTransaction == null ? "no PacnetTransaction specified" : NLModel.PacnetTransaction.ToString())
			);

			new Mail().Send(
				this.emailToAddress,
				null, // message text
				message, //html
				this.emailFromAddress, // fromEmail
				this.emailFromName, // fromName
				sMsg //"#NL_Loan failed oldLoanID: " + (int)NLModel.Loan.OldLoanID + " for customer " + this.customerID // subject
			);

		} // SendErrorMail

		public int LoanID;

		private readonly int? userID;
		private readonly int customerID;

		public NL_Loader Loader { get; private set; }
		public NL_Model NLModel { get; private set; }
		
		private readonly string emailToAddress;
		private readonly string	emailFromAddress;
		private readonly string	emailFromName;

	}//class AddLoan
}//ns