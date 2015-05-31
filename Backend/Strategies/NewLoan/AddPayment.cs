namespace Ezbob.Backend.Strategies.NewLoan {
	using System;
	using System.Collections.Generic;
	using System.Web;
	using Ezbob.Backend.Models.NewLoan;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Database;
	using MailApi;

	public class AddPayment : AStrategy {

		public AddPayment( NL_Model nlModel) {
			this.userID = nlModel.UserID;
			this.customerID = NLModel.CustomerID;
			NLModel = nlModel;
			Loader = new NL_Loader(NLModel);

			this.emailToAddress = "elinar@ezbob.com";
			this.emailFromAddress = "elinar@ezbob.com";
			this.emailFromName = "ezbob-system";
		} // constructor

		public override string Name { get { return "AddPayment"; } }

		/// <exception cref="NL_ExceptionInputDataInvalid">Condition. </exception>
		/// <exception cref="NL_ExceptionPaymentSave">Add NL loan failed: {0}</exception>
		/// <exception cref="NL_ExceptionInputDataInvalid">Condition. </exception>
		public override void Execute() {

			Log.Debug("--------+++++++++++---------- customer {0}----------++++++++++++++--------------", this.customerID);

			string message;

			// input validation
			if (NLModel.Loan == null) {
				message = string.Format("Expected input data not found (NL_Model initialized by: Loan.LoanID). Customer {0}", this.customerID);
				Log.Alert(message);
				throw new NL_ExceptionInputDataInvalid(message);
			}

			if (NLModel.Loan.LoanID == 0) {
				message = string.Format("Expected input data not found (NL_Model initialized by: Loan.LoanID). Customer {0}", this.customerID);
				Log.Alert(message);
				throw new NL_ExceptionInputDataInvalid(message);
			}

			if (NLModel.Payment == null) {
				message = string.Format("Expected input data not found (NL_Model initialized by: Payment). Customer {0}", this.customerID);
				Log.Alert(message);
				throw new NL_ExceptionInputDataInvalid(message);
			}

			try {

				Log.Debug(NLModel.Payment.ToString());

				/* 1. save NL_Payments with PaymentStatusID (NL_PaymentStatuses ("rebate"? / "system-repay" ?)), PaymentMethodID ([LoanTransactionMethod] 'Auto' ID 2)
				NLModel.Payment.CreationTime = DateTime.UtcNow; // default always*/

				NLModel.Payment.PaymentID = DB.ExecuteScalar<int>("NL_PaymentsSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter("Tbl", NLModel.Payment));

				Log.Debug("NL_PaymentsSave: LoanID: {0}, paymentID: {1}", NLModel.Loan.LoanID, NLModel.Payment.PaymentID);

				/* 2. save NL_PaypointTransactions with:
				* PaypointCardID - from just created PayPointCard.Id, 
				* PaypointTransactionStatusID =1 (Done) NL_PaypointTransactionStatuses  
				* IP - from LoanTransaction IP
				* Amount - from the method argument amount if not null, otherwise 5 pounds
				* PaymentID - from 1.	
				**/

				if (NLModel.PaypointTransaction != null) {

					//	select ppc.Id from PayPointCard ppc where ppc.TransactionId = '6fc34f9a-422f-4643-b151-06a472bed9d7'

					SafeReader sr = DB.GetFirst(string.Format("SELECT Id as PaypointCardID FROM PayPointCard WHERE TransactionId='{0}'", NLModel.PaypointTransaction.PaypointUniqID));
					if (sr["PaypointCardID"] == null) {
						message = string.Format("Paypoint card for customer {0}, loanID {1}, PaypointUniqID {2} not found", this.customerID, NLModel.Loan.LoanID, NLModel.PaypointTransaction.PaypointUniqID);
						Log.Alert(message);
						throw new NL_ExceptionRequiredDataNotFound(message);
					}

					NLModel.PaypointTransaction.PaymentID = NLModel.Payment.PaymentID;
					NLModel.PaypointTransaction.PaypointCardID = sr["PaypointCardID"];

					List<NL_PaypointTransactionStatuses> ppstatsues = NL_Loader.PaypointTransactionStatuses();

					var ppTransactionStatus = ppstatsues.Find(s => s.TransactionStatus.Equals(NLModel.PaypointTransactionStatus)) ?? ppstatsues.Find(s => s.TransactionStatus.Equals("Unknown"));
					NLModel.PacnetTransaction.PacnetTransactionStatusID = ppTransactionStatus.PacnetTransactionStatusID;

					int ppTransactionID = DB.ExecuteScalar<int>("NL_PaypointTransactionsSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter("Tbl", NLModel.PaypointTransaction));

					Log.Debug(NLModel.PaypointTransaction.ToString());

					Log.Debug("NL_PaypointTransactionsSave: LoanID: {0}, paymentID: {1}, ppTransactionID {2}", NLModel.Loan.LoanID, NLModel.Payment.PaymentID, ppTransactionID);
				}

				// ReSharper disable once CatchAllClause
			} catch (Exception ex) {

				message = string.Format("Failed to write NL_Payment / NL_PaypointTransaction for customer {0}, LoanID {1}, err: {2}", this.customerID, NLModel.Loan.LoanID, ex);

				Log.Error(message);
				SendErrorMail(message);
				
				// ReSharper disable once ThrowFromCatchWithNoInnerException
				throw new NL_ExceptionPaymentSave(message);
			}

			/* 
			* 3. new strategy AssignPaymentToLoan: argument: NL_Model with Loan.LoanID; decimal amount; output: NL_Model containing list of fees/schedules that covered by the amount/payment
			* closest unpaid loan fees and schedules (1.fee if exists; 2.interest; 3.principal) 
			* 4. save into NL_LoanSchedulePayments : PaymentID just created + NL_LoanSchedules from AssignPaymentToLoan strategy
			*/

			AssignPaymentToLoan payLoanStrategy = new AssignPaymentToLoan(this.userID, NLModel);
			payLoanStrategy.Execute();

		} // Execute


		private void SendErrorMail(string sMsg, NL_LoanFees setupFee = null, List<NL_LoanSchedules> scheduleItems = null) {
			var message = string.Format(
				"<h3>CustomerID: {0}</h3><p>"
				 + "<h3>Input data</h3>: {1} <br/>"
				 + "<h3>Loan</h3>: {2} <br/>"
				 + "<h3>NL_Payment</h3>: {3} <br/>"
				 + "<h3>NL_PaypointTransaction</h3>: {4} <br/>"
				 + "</p>",

				this.customerID
				, HttpUtility.HtmlEncode(NLModel.ToString())
				, HttpUtility.HtmlEncode(NLModel.Loan == null ? "no Loan specified" : NLModel.Loan.ToString())
				, HttpUtility.HtmlEncode(NLModel.Payment == null ? "no Payment specified" : NLModel.Payment.ToString())
				, HttpUtility.HtmlEncode(NLModel.PaypointTransaction == null ? "no PaypointTransaction specified" : NLModel.PaypointTransaction.ToString())
			);

			new Mail().Send(
				this.emailToAddress,
				null, // message text
				message, //html
				this.emailFromAddress, // fromEmail
				this.emailFromName, // fromName
				sMsg // subject
			);

		} // SendErrorMail

		private readonly int userID;
		private readonly int customerID;

		public NL_Loader Loader { get; private set; }
		public NL_Model NLModel { get; private set; }


		private readonly string emailToAddress;
		private readonly string	emailFromAddress;
		private readonly string	emailFromName;

	} // class AddPayment
} // ns