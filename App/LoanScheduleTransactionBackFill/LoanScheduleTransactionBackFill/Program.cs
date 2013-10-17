using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Ezbob.Context;
using Ezbob.Database;
using Ezbob.Logger;
using Ezbob.Utils;

namespace LoanScheduleTransactionBackFill {
	class Program {
		static void Main(string[] args) {
			using (var oLog = new FileLog(oLog: new ConsoleLog())) {
				var oEnv = new Ezbob.Context.Environment(Name.Production, oLog: oLog);

				var oDB = new SqlConnection(oEnv, oLog);

				var oLoans = new SortedDictionary<int, Loan>();

				var oProgress = new ProgressCounter("{0} transaction entries processed.", oLog, 64);

				oDB.ForEachRow(
					(oReader, bRowsetStart) => {
						int nLoanID = Convert.ToInt32(oReader["LoanID"]);

						if (oLoans.ContainsKey(nLoanID))
							oLoans[nLoanID].Transactions.Add(new Transaction(oReader, oLog));
						else
							oLoans[nLoanID] = new Loan(oReader, oLog);

						oProgress++;

						return ActionResult.Continue;
					},
					GetQuery("Transactions")
				);

				oProgress.Log();

				oProgress = new ProgressCounter("{0} schedule entries processed.", oLog, 64);

				oDB.ForEachRow(
					(oReader, bRowsetStart) => {
						int nLoanID = Convert.ToInt32(oReader["LoanID"]);

						if (oLoans.ContainsKey(nLoanID))
							oLoans[nLoanID].Actual.Add(new Schedule(oReader, oLog));

						oProgress++;

						return ActionResult.Continue;
					},
					GetQuery("Schedule")
				);

				oProgress.Log();

				oLog.Msg("Loans status:");

				var oStateStat = new SortedDictionary<ScheduleState, int>();

				foreach (Loan l in oLoans.Values) {
					l.BuildWorkingSet();

					if (l.IsCountable && (l.TotalPrincipalPaid > l.LoanAmount))
						oLog.Warn("Loan {0}: principal ({1}) is greater than loan amount ({2}).", l.ID, l.TotalPrincipalPaid, l.LoanAmount);

					l.Calculate();

					oLog.Msg("{0}", l.ToString());

					if (oStateStat.ContainsKey(l.ScheduleState))
						oStateStat[l.ScheduleState]++;
					else
						oStateStat[l.ScheduleState] = 1;
				} // for each loan

				double nTotal = (double)oStateStat.Values.Sum();

				foreach (KeyValuePair<ScheduleState, int> pair in oStateStat)
					oLog.Msg("Status {0}: {1} loans = {2}", pair.Key, pair.Value, ((double)pair.Value / nTotal).ToString("p2"));
			} // using log file
		} // Main

		private static string GetQuery(string sFileName) {
			Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream(
				"LoanScheduleTransactionBackFill." + sFileName + ".sql"
			);

			var sr = new StreamReader(s);

			return sr.ReadToEnd();
		} // GetQuery
	} // class Program
} // namespace
