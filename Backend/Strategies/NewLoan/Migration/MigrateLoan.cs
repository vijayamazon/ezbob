﻿namespace Ezbob.Backend.Strategies.NewLoan.Migration {
	using System;
	using System.Collections.Generic;
	using DbConstants;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using StructureMap;

	public class MigrateLoan : AStrategy {

		public MigrateLoan() {
			nowTime = DateTime.UtcNow;
			//this.strategyArgs = new object[] {
			//	nowTime
			//};
			loanRep = ObjectFactory.GetInstance<LoanRepository>();
		}

		public override string Name {get { return "MigrateLoan"; }}

		public string Error { get; private set; }
		//public long LoanID { get; private set; }
		//public NL_Model model { get; private set; }

		//private readonly object[] strategyArgs;
		public DateTime nowTime { get; private set; }

		public LoanRepository loanRep { get; private set; }

		public class LoanTransactionModel {
			public DateTime PostDate { get; set; }
			public decimal Amount { get; set; }
			public string Description { get; set; }
			public string IP { get; set; }
			public string PaypointId { get; set; }
			public int CardID { get; set; }
			public long LoanID { get; set; }
			public int LoanTransactionMethodId { get; set; }
			public int CustomerId { get; set; }
		}

		public class CashReqModel {
			public long CashRequestID { get; set; }
			public long DecisionID { get; set; }
			public long OfferID { get; set; }
		}

		public class LoanId {
			public int Id { get; set; }
		}


		public override void Execute() {
			CopyPaypointTransactions();
			UpdateLoanStates();
		} //Execute

		
		public void CopyPaypointTransactions() {

			List<LoanTransactionModel> transactionsList = DB.Fill<LoanTransactionModel>(
				"select t.PostDate,t.Amount,t.[Description], t.IP, t.PaypointId, t.LoanTransactionMethodId, c.Id as CardID, nl.[LoanID], l.CustomerId " +
					"from NL_Loans nl join LoanTransaction t on t.LoanId=nl.OldLoanID join PayPointCard c on c.TransactionId = t.PaypointId left join [dbo].[NL_Payments] p on p.LoanID=nl.LoanID join Loan l on l.Id=t.LoanId " +
					"where t.[Status]='Done' and t.[Type]='PaypointTransaction' and p.PaymentID is null order by t.LoanId, t.PostDate", CommandSpecies.Text);

			foreach (LoanTransactionModel transaction in transactionsList) {

				if (transaction == null || transaction.Amount == 0) {
					Error = "transaction not found/or amount=0";
					Log.Debug(Error);
					NL_AddLog(LogType.Info, Error, new object[] {
						transaction
					}, null, Error, null);
					continue;
				}

				// check the payment exists
				//	List<NL_Payments> nlPayments = DB.Fill<NL_Payments>("NL_PaymentsGet", CommandSpecies.StoredProcedure, new QueryParameter("LoanID", LoanID));
				//if (nlPayments.FirstOrDefault(p => p.Amount == transaction.Amount && transaction.PostDate.Date.Equals(p.PaymentTime.Date)) != null) {
				//	Log.Debug("rebate transaction for oldLoanID {0} already recorded", model.Loan.OldLoanID);
				//	NL_AddLog(LogType.Info, string.Format("rebate transaction for oldLoanID {0} already recorded", model.Loan.OldLoanID), args, null, Error, null);
				//	return false;
				//}

				// call AddPayment 
				NL_Payments payment = new NL_Payments() {
					Amount = transaction.Amount,
					CreatedByUserID = 1,
					LoanID = transaction.LoanID,
					PaymentStatusID = (int)NLPaymentStatuses.Active,
					PaymentMethodID = transaction.LoanTransactionMethodId, 
					CreationTime = transaction.PostDate,
					PaymentTime = transaction.PostDate.AddMilliseconds(60),
					Notes = (transaction.Description.Equals("system-repay") && transaction.Amount==5m) ? "rebate"  : transaction.Description
				};
				payment.PaypointTransactions.Add(new NL_PaypointTransactions() {
					Amount = transaction.Amount,
					IP = transaction.IP,
					Notes = transaction.Description,
					PaypointTransactionStatusID = (int)NLPaypointTransactionStatuses.Done,
					PaypointUniqueID = transaction.PaypointId,
					PaypointCardID = transaction.CardID,
					TransactionTime = transaction.PostDate
				});

				NL_AddLog(LogType.Info, "Copying transaction", new object[] {
					transaction, payment
				}, Error, null, null);

				try {
					AddPayment p = new AddPayment(transaction.CustomerId, payment, 1);
					p.Context.UserID = 1;
					p.Context.CustomerID = transaction.CustomerId;
					p.Execute();

					// ReSharper disable once CatchAllClause
				} catch (Exception ex) {
					Error = String.Format("failed to copy paypoint transaction {0}, err: {1}", transaction, ex.Message);
					Log.Debug(Error);
					NL_AddLog(LogType.Error, "Copying failed", new object[] {transaction, payment}, Error, ex.Message, ex.StackTrace);
				}
			}
		}


		public void UpdateLoanStates() {
			List<LoanTransactionModel> loansList = DB.Fill<LoanTransactionModel>("select nl.[LoanID], l.CustomerId from NL_Loans nl join Loan l on l.Id=nl.OldLoanID", CommandSpecies.Text);

			foreach (LoanTransactionModel transaction in loansList) {
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