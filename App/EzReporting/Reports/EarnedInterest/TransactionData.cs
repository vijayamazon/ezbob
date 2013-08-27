using System;

namespace Reports {
	#region class TransactionData

	class TransactionData {
		#region public

		public readonly DateTime Date;
		public decimal Repayment;

		#region constructor

		public TransactionData(DateTime oDate, decimal nRepayment) {
			Date = oDate;
			Repayment = nRepayment;
		} // constructor

		#endregion constructor

		#region method ToStirng

		public override string ToString() {
			return string.Format("on {0}: {1}", Date, Repayment);
		} // ToString

		#endregion method ToStirng

		#endregion public
	} // class TransactionData

	#endregion class TransactionData
} // namespace Reports
