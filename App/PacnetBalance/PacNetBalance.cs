using System;
using Ezbob.Logger;
using System.Collections.Generic;
using Ezbob.Database;
using System.Linq;

namespace PacnetBalance
{
	public static class PacNetBalance {
		private static List<PacNetBalanceRow> pacNetBalanceList;

		static PacNetBalance()
		{
			Logger = new SafeLog();
		} // static constructor

		public static ASafeLog Logger { get; set; }

		/// <summary>
		/// Populate pacNetBalanceList with calcuted data from pacnet report pdf
		/// </summary>
		/// <param name="date">Date</param>
		/// <param name="openingBalance">OpeningBalance</param>
		/// <param name="closingBalance">ClosingBalance</param>
		/// <param name="credits">Credits</param>
		/// <param name="debits">Debits</param>
		/// <param name="rows"></param>
		/// <param name="loginAddress"></param>
		/// <param name="loginPassword"></param>
		static public void PopulateList(DateTime date, decimal openingBalance, decimal closingBalance, decimal credits, decimal debits, List<PacNetBalanceRow> rows, string loginAddress, string loginPassword)
		{
			pacNetBalanceList = new List<PacNetBalanceRow>();
			decimal currenBalance = openingBalance;
			
			foreach (var row in rows) {
				if (row.IsCredit) {
					currenBalance = currenBalance + (Math.Abs(row.Amount) + Math.Abs(row.Fees));
				} else {
					currenBalance = currenBalance - (Math.Abs(row.Amount) + Math.Abs(row.Fees));
				}
				AddRowToList(currenBalance, date, row.Fees, row.Amount, row.IsCredit);
			} // foreach

			VerifyCalculatedValues(openingBalance, closingBalance, credits, debits, pacNetBalanceList, loginAddress, loginPassword);
		} // PopulateList

		private static void VerifyCalculatedValues(decimal openingBalance, decimal closingBalance, decimal credits, decimal debits, List<PacNetBalanceRow> rows, string loginAddress, string loginPassword)
		{
			var sb = new System.Text.StringBuilder();

			decimal calculatedDebits = rows.Where(x => x.IsCredit == false)
				.Sum(x => x.Amount + x.Fees);
			decimal calculatedCredits = rows.Where(x => x.IsCredit)
				.Sum(x => x.Amount + x.Fees);


			if (calculatedDebits == debits) {
				Logger.Info("Debits is equal to calculated debits and is:{0}", debits);
			} else {
				var error = string.Format("Debits is not equal to calculated debits. Debits:{0} CalculatedDebits:{1}", debits, calculatedDebits);
				sb.AppendLine(error);
			}

			if (calculatedCredits == credits) {
				Logger.Info("Credits is equal to calculated credits and is:{0}", credits);
			} else {
				var error = string.Format("Credits is not equal to calculated credits. Credits:{0} CalculatedCredits:{1}", credits, calculatedCredits);
				sb.AppendLine(error);
			}

			decimal calculatedClosingBalance = openingBalance 
				+ rows.Where(x => x.IsCredit).Sum(x => x.Amount + x.Fees) 
				- rows.Where(x => x.IsCredit == false).Sum(x => x.Amount + x.Fees);

			if (calculatedClosingBalance == closingBalance)
			{
				Logger.Info("Closing balance is equal to calculated balance and is:{0}", closingBalance);
			}
			else {
				var error = string.Format("Closing balance is not equal to calculated balance. ClosingBalance:{0} CalculatedClosingBalance:{1}", closingBalance, calculatedClosingBalance);
				sb.AppendLine(error);
			}

			if (rows.Any()) {
				var lastRow = rows.Last();
				
				if (lastRow.CurrentBalance == closingBalance) {
					Logger.Info("Closing balance is equal to last row balance and is:{0}", closingBalance);
				} else {
					var error = string.Format("Closing balance is not equal to last row balance. ClosingBalance:{0} LastRowBalance:{1}", closingBalance, lastRow.CurrentBalance);
					sb.AppendLine(error);
					lastRow.CurrentBalance = closingBalance;
				}
			}

			if (sb.Length > 0) {
				Logger.Error(sb.ToString());
				if (!string.IsNullOrEmpty(loginAddress) && !string.IsNullOrEmpty(loginPassword)) {
					Mailer.Mailer.SendMail(loginAddress, loginPassword, "PacNet Balance Report Error", sb.ToString(),
						"dev@ezbob.com");
				}
				//throw new PacNetBalanceException(sb.ToString());
			}
		} // VerifyCalculatedValues

		private static void AddRowToList(decimal currenBalance, DateTime date, decimal fees, decimal amount, bool isCredit = false)
		{
			pacNetBalanceList.Add(new PacNetBalanceRow
			{
				CurrentBalance = currenBalance,
				Date = date,
				Fees = Math.Abs(fees),
				Amount = Math.Abs(amount),
				IsCredit = isCredit
			});
		} // AddRowToList

		/// <summary>
		/// Save the pacNetBalanceList to the DB
		/// </summary>
		public static void SavePacNetBalanceToDb()
		{
			try
			{
				var oDB = new SqlConnection();

				foreach (PacNetBalanceRow row in pacNetBalanceList)
				{
					oDB.ExecuteNonQuery(
						Consts.InsertPacNetBalanceSpName,
						new QueryParameter(Consts.InsertPacNetBalanceSpParam1, row.Date.ToString(Consts.InsertPacNetBalanceSpDateFormat)),
						new QueryParameter(Consts.InsertPacNetBalanceSpParam2, row.Amount),
						new QueryParameter(Consts.InsertPacNetBalanceSpParam3, row.Fees),
						new QueryParameter(Consts.InsertPacNetBalanceSpParam4, row.CurrentBalance),
						new QueryParameter(Consts.InsertPacNetBalanceSpParam5, row.IsCredit)
					);
				}
				Logger.Info("All rows updated");
			}
			catch (Exception e)
			{
				Logger.Error("Error inserting row. Error was:{0}", e);
				throw new PacNetBalanceException(string.Format("Pacnet Error inserting row. Error was:{0}", e));
			} // try
		} // SavePacNetBalanceToDb
	} // class PacNetBalance
} // namespace
