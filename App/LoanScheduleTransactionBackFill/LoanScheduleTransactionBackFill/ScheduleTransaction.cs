using System.Text;
using Ezbob.Database;

namespace LoanScheduleTransactionBackFill {
	#region class ScheduleTransaction

	class ScheduleTransaction {
		#region public

		#region constructor

		public ScheduleTransaction() {
		} // constructor

		#endregion constructor

		public Schedule Schedule { get; set; }
		public Transaction Transaction { get; set; }
		public LoanScheduleStatus Status { get; set; }

		public decimal PrincipalDelta { get; set; }
		public decimal InterestDelta { get; set; }
		public decimal FeesDelta { get; set; }

		#region method Save

		public void Save(int nLoanID, AConnection oDB) {
			var os = new StringBuilder();

			os.AppendFormat(
				"INSERT INTO LoanScheduleTransaction" +
					"(LoanID, ScheduleID, TransactionID, Date, PrincipalDelta, FeesDelta, InterestDelta, StatusBefore, StatusAfter)" +
				"VALUES ({0}, {1}, {2}, '{3}', {4}, {5}, {6},'{7}','{7}')\n" +
				"INSERT INTO LoanScheduleTransactionBackfilled (LoanScheduleTransactionID) VALUES (@@IDENTITY)", 
				nLoanID, Schedule.ID, Transaction.ID,
				Transaction.Date.ToString("MMMM dd yyyy HH:mm:ss", Schedule.Culture), 
				PrincipalDelta, InterestDelta, FeesDelta, Status
			);

			string sQuery = os.ToString();

			oDB.ExecuteNonQuery(sQuery);
		} // Save

		#endregion method Save

		#region method ToString

		public override string ToString() {
			var os = new StringBuilder();

			os.AppendFormat("Status: {0,10} Schedule: {1,7} Transaction: {2,7}", Status, Schedule.ID, Transaction.ID);

			os.AppendFormat(
				" p: {0,10} i: {1,10} f: {2,10}",
				PrincipalDelta.ToString("C2", Schedule.Culture),
				InterestDelta.ToString("C2", Schedule.Culture),
				FeesDelta.ToString("C2", Schedule.Culture)
			);

			return os.ToString();
		} // ToString

		#endregion method ToString

		#endregion public
	} // class ScheduleTransaction

	#endregion class ScheduleTransaction
} // namespace LoanScheduleTransactionBackFill
