namespace Ezbob.Backend.Strategies.NewLoan.Migration {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using DbConstants;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Database;

	public class ConvertPaypointTransaction : AStrategy {

		//public MigrateLoanTransaction() {}

		public override string Name { get { return "MigrateLoanTransaction"; } }

		public string Error { get; private set; }

	


		public override void Execute() {

			NL_AddLog(LogType.Info, "Started", null, null, null, null);

			CopyPaypointTransactions();
			//UpdateLoanStates();

			NL_AddLog(LogType.Info, "Ended", null, null, null, null);

		} //Execute


		public void CopyPaypointTransactions() {

			const string query = "select t.PostDate,t.Amount,t.[Description], t.IP, t.PaypointId, t.LoanTransactionMethodId, c.Id as CardID, nl.[LoanID], l.CustomerId " +
				"from NL_Loans nl join LoanTransaction t on t.LoanId=nl.OldLoanID " +
				"join Loan l on l.Id=t.LoanId " +
				"left join PayPointCard c on c.TransactionId = t.PaypointId " +
				"left join [dbo].[NL_Payments] p on p.LoanID=nl.LoanID " +
				"where t.[Status]='Done' and t.[Type]='PaypointTransaction' and p.PaymentID is null order by t.LoanId, t.PostDate";

			List<MigrationModels.LoanTransactionModel> transactionsList = DB.Fill<MigrationModels.LoanTransactionModel>(query, CommandSpecies.Text);

			foreach (MigrationModels.LoanTransactionModel transaction in transactionsList) {

				bool savePayment = false;

				var args = new object[] {transaction, query};

				if (transaction == null || transaction.Amount == 0) {
					//Error = "transaction not found/or amount=0";
					//Log.Debug(Error);
					NL_AddLog(LogType.Info, "transaction not found or amount=0", args, null, null, null);
					continue;
				}
				
				//check the payment exists
				List<NL_Payments> nlPayments = DB.Fill<NL_Payments>("NL_PaymentsGet", CommandSpecies.StoredProcedure, new QueryParameter("LoanID", transaction.LoanID));

				NL_Payments payment = nlPayments.FirstOrDefault(p => p.Amount == transaction.Amount && transaction.PostDate.Date.Equals(p.PaymentTime.Date) && transaction.LoanTransactionMethodId==p.PaymentMethodID && p.CreationTime.Date.Equals(transaction.PostDate.Date));

				if (payment == null) {
					payment = new NL_Payments() {
						Amount = transaction.Amount,
						CreatedByUserID = 1,
						LoanID = transaction.LoanID,
						PaymentStatusID = (int)NLPaymentStatuses.Active,
						PaymentMethodID = transaction.LoanTransactionMethodId,
						CreationTime = transaction.PostDate,
						PaymentTime = transaction.PostDate,
						Notes = (transaction.Description.Equals("system-repay") && transaction.Amount == 5m) ? "rebate" : transaction.Description
					};
					savePayment = true;
				} else {
					NL_AddLog(LogType.Info, "Payment exists", args, payment, null, null);
				}

				if (!string.IsNullOrEmpty(transaction.CardID.ToString())) {

					var paypointTrans = payment.PaypointTransactions.FirstOrDefault(ppt => ppt.PaypointUniqueID == transaction.PaypointId && ppt.PaypointCardID == transaction.CardID && ppt.TransactionTime.Date.Equals(transaction.PostDate.Date));

					if (paypointTrans == null) {
						payment.PaypointTransactions.Add(new NL_PaypointTransactions() {
							Amount = transaction.Amount,
							IP = transaction.IP,
							Notes = transaction.Description,
							PaypointTransactionStatusID = (int)NLPaypointTransactionStatuses.Done,
							PaypointUniqueID = transaction.PaypointId,
							PaypointCardID = transaction.CardID,
							TransactionTime = transaction.PostDate
						});
						savePayment = true;
					} else {
						NL_AddLog(LogType.Info, "PPT exists", args, paypointTrans, null, null);
					}
				}

				if (savePayment) {

					NL_AddLog(LogType.Info, "Copying transaction", args, payment, null, null);

					try {
						AddPayment p = new AddPayment(transaction.CustomerId, payment, 1);
						p.Context.UserID = 1;
						p.Context.CustomerID = transaction.CustomerId;
						p.Execute();

						// ReSharper disable once CatchAllClause
					} catch (Exception ex) {
						Error = String.Format("failed to copy paypoint transaction {0}, err: {1}", transaction, ex.Message);
						Log.Debug(Error);
						NL_AddLog(LogType.Error, "Copying failed", new object[] {
							transaction, payment
						}, Error, ex.Message, ex.StackTrace);
					}
				}
			}
		}


		public void UpdateLoanStates() {
			List<MigrationModels.LoanTransactionModel> loansList = DB.Fill<MigrationModels.LoanTransactionModel>("select nl.[LoanID], l.CustomerId from NL_Loans nl join Loan l on l.Id=nl.OldLoanID", CommandSpecies.Text);

			foreach (MigrationModels.LoanTransactionModel transaction in loansList) {
				// recalculate state by calculator + save new state to DB
				UpdateLoanDBState reloadLoanDBState = new UpdateLoanDBState(transaction.CustomerId, transaction.LoanID, 1);
				try {
					reloadLoanDBState.Execute();

					// ReSharper disable once CatchAllClause
				} catch (Exception ex) {
					Error = ex.Message;
					Log.Error("Failed on UpdateLoanDBState {0}", Error);
					NL_AddLog(LogType.Error, "Failed", transaction.LoanID, reloadLoanDBState.Error + "\n" + Error, ex.ToString(), ex.StackTrace);
				}
			}
		}
	}
}//ns