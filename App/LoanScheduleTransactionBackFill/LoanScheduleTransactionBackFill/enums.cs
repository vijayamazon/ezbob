namespace LoanScheduleTransactionBackFill {
	enum LoanType { Standard, Halfway }

	enum LoanScheduleStatus { StillToPay, PaidOnTime, Late, PaidEarly, Paid, AlmostPaid }

	enum ScheduleState {
		Unknown,
		Match,
		SameCountDiffDates,
		DiffCount
	} // enum ScheduleState

} // namespace LoanScheduleTransactionBackFill
