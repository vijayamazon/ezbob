using System;
using System.Data.Common;
using Ezbob.Logger;

namespace LoanScheduleTransactionBackFill {
	#region class Transaction

	class Transaction : Schedule {
		#region public

		#region constructor

		public Transaction(ASafeLog log = null) : base(log) {
		} // constructor

		public Transaction(DbDataReader row, ASafeLog log = null) : base(row, log) {
			Interest = Convert.ToDecimal(row["Interest"]);
			Fees = Convert.ToDecimal(row["Fees"]);
		} // constructor

		public Transaction(Transaction o) : base((Schedule)o) {
			Interest = o.Interest;
			Fees = o.Fees;
		} // constructor

		#endregion constructor

		public decimal Interest { get; set; }
		public decimal Fees { get; set; }

		#region method ToString

		public override string ToString() {
			return string.Format("{0} i: {1,10} f: {2,10}", base.ToString(), Interest.ToString("C2", Culture), Fees.ToString("C2", Culture));
		} // ToString

		#endregion method ToString

		#endregion public
	} // class Transaction

	#endregion class Transaction
} // namespace LoanScheduleTransactionBackFill
