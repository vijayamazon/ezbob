namespace Ezbob.Backend.Strategies.NewLoan {
	using System;
	using System.Collections.Generic;
	using System.Web;
	using Ezbob.Backend.Models.NewLoan;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Database;
	using MailApi;

	public class AddPayment : AStrategy {

		public AddPayment(NL_Model nlModel) {

			NLModel = nlModel;
			Loader = new NL_Loader(NLModel);

			this.emailToAddress = "elinar@ezbob.com";
			this.emailFromAddress = "elinar@ezbob.com";
			this.emailFromName = "ezbob-system";
		} // constructor

		public override string Name { get { return "AddPayment"; } }

		/// <exception cref="NL_ExceptionInputDataInvalid">Condition. </exception>
		/// <exception cref="NL_ExceptionInputDataInvalid">Condition. </exception>
		/// <exception cref="NL_ExceptionPaymentSave">Condition. </exception>
		/// <exception cref="NL_ExceptionRequiredDataNotFound">Condition. </exception>
		public override void Execute() {

			Log.Debug("--------+++++++++++---------- customer {0}----------++++++++++++++--------------", NLModel.CustomerID);

			string message;

			// input validation
			if (NLModel.Loan == null) {
				message = string.Format("Expected input data not found (NL_Model initialized by: Loan.LoanID). Customer {0}", NLModel.CustomerID);
				Log.Alert(message);
				throw new NL_ExceptionInputDataInvalid(message);
			}

			if (NLModel.Loan.LoanID == 0) {
				message = string.Format("Expected input data not found (NL_Model initialized by: Loan.LoanID). Customer {0}", NLModel.CustomerID);
				Log.Alert(message);
				throw new NL_ExceptionInputDataInvalid(message);
			}

			if (NLModel.Payment == null) {
				message = string.Format("Expected input data not found (NL_Model initialized by: Payment). Customer {0}", NLModel.CustomerID);
				Log.Alert(message);
				throw new NL_ExceptionInputDataInvalid(message);
			}

			if (NLModel.PaypointTransaction != null) {

				if (NLModel.PaypointTransactionStatus == null) {
					message = string.Format("Saving PaypointTransaction: Expected input data not found (NLModel initialized by: PaypointTransactionStatus). Customer {0}", NLModel.CustomerID);
					Log.Alert(message);
					throw new NL_ExceptionInputDataInvalid(message);
				}

				if (NLModel.PaypointTransaction.PaypointUniqID == null) {
					message = string.Format("Saving PaypointTransaction: Expected input data not found (NLModel initialized by: PaypointTransactionStatus.PaypointUniqID). Customer {0}", NLModel.CustomerID);
					Log.Alert(message);
					throw new NL_ExceptionInputDataInvalid(message);
				}

				//	select ppc.Id from PayPointCard ppc where ppc.TransactionId = '6fc34f9a-422f-4643-b151-06a472bed9d7'
				SafeReader sr = DB.GetFirst(string.Format("SELECT Id as PaypointCardID FROM PayPointCard WHERE TransactionId='{0}' and CustomerId={1}", NLModel.PaypointTransaction.PaypointUniqID, NLModel.CustomerID));

				//Console.WriteLine(string.Format("SELECT Id as PaypointCardID FROM PayPointCard WHERE TransactionId='{0}' and CustomerId={1}", NLModel.PaypointTransaction.PaypointUniqID, NLModel.CustomerID));
				//Console.WriteLine("========================" + sr.IsEmpty);
				//Console.WriteLine("========================" + sr.Count);
				if (sr.IsEmpty) {
					message = string.Format("Paypoint card for customer {0}, loanID {1}, PaypointUniqID {2} not found", NLModel.CustomerID, NLModel.Loan.LoanID, NLModel.PaypointTransaction.PaypointUniqID);
					Log.Alert(message);
					throw new NL_ExceptionRequiredDataNotFound(message);
				}

				NLModel.PaypointTransaction.PaypointCardID = sr["PaypointCardID"];

				Console.WriteLine(NLModel.PaypointTransaction.ToString());
			}

			ConnectionWrapper pconn = DB.GetPersistent();

			try {

				NLModel.Payment.CreationTime = DateTime.UtcNow;

				Log.Debug(NLModel.Payment.ToString());

				pconn.BeginTransaction();

				// 1. save NL_Payments
				NLModel.Payment.PaymentID = DB.ExecuteScalar<int>(pconn, "NL_PaymentsSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter("Tbl", NLModel.Payment));

				Log.Debug("NL_PaymentsSave: LoanID: {0}, paymentID: {1}", NLModel.Loan.LoanID, NLModel.Payment.PaymentID);

				// 2. save NL_PaypointTransactions
				if (NLModel.PaypointTransaction != null) {

					NLModel.PaypointTransaction.PaymentID = NLModel.Payment.PaymentID;

					List<NL_PaypointTransactionStatuses> ppstatsues = NL_Loader.PaypointTransactionStatuses();

					//ppstatsues.ForEach(s=> 		Console.WriteLine(s.TransactionStatus));

					var ppTransactionStatus = ppstatsues.Find(s => s.TransactionStatus.StartsWith(NLModel.PaypointTransactionStatus)) ?? ppstatsues.Find(s => s.TransactionStatus.Equals("Unknown"));

					//Console.WriteLine("==================>" + ppTransactionStatus.TransactionStatus + "=" + ppTransactionStatus.PaypointTransactionStatusID);

					NLModel.PaypointTransaction.PaypointTransactionStatusID = ppTransactionStatus.PaypointTransactionStatusID;

					Log.Debug(NLModel.PaypointTransaction.ToString());

					NLModel.PaypointTransaction.PaypointTransactionID = DB.ExecuteScalar<int>(pconn, "NL_PaypointTransactionsSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter("Tbl", NLModel.PaypointTransaction));

					Log.Debug("NL_PaypointTransactionsSave: LoanID: {0}, paymentID: {1}, PaypointTransactionID {2}", NLModel.Loan.LoanID, NLModel.Payment.PaymentID, NLModel.PaypointTransaction.PaypointTransactionID);
				}

				pconn.Commit();

				// ReSharper disable once CatchAllClause
			} catch (Exception ex) {

				message = string.Format("Failed to write NL_Payment / NL_PaypointTransaction for customer {0}, LoanID {1}, err: {2}", NLModel.CustomerID, NLModel.Loan.LoanID, ex);
				Log.Error(message);

				pconn.Rollback();
				SendErrorMail(message);

				// ReSharper disable once ThrowFromCatchWithNoInnerException
				throw new NL_ExceptionPaymentSave(message);
			}

			/* 
			* 3. strategy AssignPaymentToLoan: argument: NL_Model with Loan.LoanID; decimal amount; output: NL_Model containing list of fees/schedules that covered by the amount/payment
			* closest unpaid loan fees and schedules (1.fee if exists; 2.interest; 3.principal) 
			* 4. strategy AssignPaymentToLoan: saving into NL_LoanSchedulePayments with PaymentID sent + NL_LoanSchedules
			*/
			// UNCOMMENT AFTER AssignPaymentToLoan be ready
			// AssignPaymentToLoan payLoanStrategy = new AssignPaymentToLoan(NLModel);
			// payLoanStrategy.Execute();

		} // Execute


		private void SendErrorMail(string sMsg, NL_LoanFees setupFee = null, List<NL_LoanSchedules> scheduleItems = null) {
			var message = string.Format(
				"<h3>CustomerID: {0};</h3>"
				 + "<h3>UserID: {1}</h3><p>"
				 + "<h3>Input data</h3>: {2} <br/>"
				 + "<h3>Loan</h3>: {3} <br/>"
				 + "<h3>NL_Payment</h3>: {4} <br/>"
				 + "<h3>NL_PaypointTransaction</h3>: {5} <br/>"
				 + "</p>",

				NLModel.CustomerID
				, NLModel.UserID
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


		public NL_Loader Loader { get; private set; }
		public NL_Model NLModel { get; private set; }

		private readonly string emailToAddress;
		private readonly string	emailFromAddress;
		private readonly string	emailFromName;

	} // class AddPayment
} // ns