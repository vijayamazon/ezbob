using System.Text;
using Ezbob.Database;

namespace LoanScheduleTransactionBackFill {

	class ScheduleTransaction {

		public ScheduleTransaction() {
		} // constructor

		public Schedule Schedule { get; set; }
		public Transaction Transaction { get; set; }
		public LoanScheduleStatus Status { get; set; }

		public decimal PrincipalDelta { get; set; }
		public decimal InterestDelta { get; set; }
		public decimal FeesDelta { get; set; }

		public bool IsAlreadyProcessed { get { return (Schedule != null && Schedule.IsAlreadyProcessed) || Transaction.IsAlreadyProcessed; } }

		public void Save(int nLoanID, ScheduleState nStatus, AConnection oDB) {
			if (IsAlreadyProcessed)
				if (!Loan.Step2.Contains(nLoanID))
					return;

			if (!Loan.Step2.Contains(nLoanID))
				return;

			var os = new StringBuilder();

			int nIsBad = 0;

			if (nStatus == ScheduleState.DiffCount)
				nIsBad = 1;

			os.AppendFormat(
				"INSERT INTO LoanScheduleTransaction" +
					"(LoanID, ScheduleID, TransactionID, Date, PrincipalDelta, FeesDelta, InterestDelta, StatusBefore, StatusAfter)" +
				"VALUES ({0}, {1}, {2}, '{3}', {4}, {5}, {6},'{7}','{7}')\n" +
				"INSERT INTO LoanScheduleTransactionBackfilled (LoanScheduleTransactionID, IsBad, Step) VALUES (@@IDENTITY, {8}, {9})", 
				nLoanID, Schedule.ID, Transaction.ID,
				Transaction.Date.ToString("MMMM dd yyyy HH:mm:ss", Schedule.Culture), 
				PrincipalDelta, InterestDelta, FeesDelta, Status, nIsBad, 2
			);

			string sQuery = os.ToString();

			oDB.ExecuteNonQuery(sQuery);
		} // Save

		public override string ToString() {
			var os = new StringBuilder();

			os.AppendFormat(
				"{3} Status: {0,10} Schedule: {1,7} Transaction: {2,7}",
				Status, (Schedule == null ? "null" : Schedule.ID.ToString()), Transaction.ID,
				IsAlreadyProcessed ? "v" : " "
			);

			os.AppendFormat(
				" p: {0,10} i: {1,10} f: {2,10}",
				PrincipalDelta.ToString("C2", Schedule.Culture),
				InterestDelta.ToString("C2", Schedule.Culture),
				FeesDelta.ToString("C2", Schedule.Culture)
			);

			return os.ToString();
		} // ToString

	} // class ScheduleTransaction

} // namespace LoanScheduleTransactionBackFill
