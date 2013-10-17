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

		#endregion public
	} // class ScheduleTransaction

	#endregion class ScheduleTransaction
} // namespace LoanScheduleTransactionBackFill
