namespace DbConstants {

	using System.ComponentModel;

	// Numeric value: number of days in the interval, except Month (where it can vary).
	public enum RepaymentIntervalTypes {
		Month = 0,
		Day = 1,
		Week = 7,
		TenDays = 10
	} // enum RepaymentIntervalTypes

	public enum RepaymentIntervalTypesId {
		Month = 1,
		Day = 2,
		Week = 3,
		TenDays = 4
	} // enum RepaymentIntervalTypesId


	public enum NLLoanStatuses {
		Live = 1, // DB table id
		Late = 2,
		PaidOff = 3,
		Pending = 4,
		Default = 5,
		WriteOff = 6,
		DebtManagement = 7
	} // enum NLLoanStatuses

	public enum FeeTypes {
		SetupFee = 1, // DB table id
		RolloverFee = 2,
		AdminFee = 3,
		ServicingFee = 4,
		ArrangementFee = 5,
		LatePeriod1 = 6,
		LatePeriod2 = 7,
		LatePeriod3 = 8,
		LatePaymentFee = 9,
		PartialPaymentFee = 10,
	} // enum FeeTypes


	public enum NLScheduleStatuses {
		[Description("Open")]
		StillToPay = 1, // db ID
		[Description("Paid ontime")]
		PaidOnTime = 2,
		[Description("Late")]
		Late = 3,
		[Description("Paid early")]
		PaidEarly = 4,
		[Description("Paid")]
		Paid = 5,
		[Description("Almost paid")]
		AlmostPaid = 6
	} // enum NLScheduleStatuses

	public enum NLLoanTypes {
		[Description("Standard Loan")]
		StandardLoanType = 1, // DB table id
		[Description("HalfWay Loan")]
		HalfWayLoanType = 2,
		[Description("Alibaba Loan")]
		AlibabaLoanType = 3
	} // enum NLLoanTypes

	public enum NLPacnetTransactionStatuses {
		Submited = 1, // DB table id
		ConfigErrorMultipleCandidateChannels = 2,
		Error = 3,
		InProgress = 4,
		PaymentByCustomer = 5,
		Done = 6
	} //enum NLPacnetTransactionStatuses

	public enum NLPaypointTransactionStatuses {
		Done = 1, // DB table id
		Error = 2,
		Unknown = 3
	} // enum NLPaypointTransactionStatuses

	public enum NLPaymentStatuses {
		InProgress = 1, // DB table id
		Active = 2,
		Deleted = 3,
		Cancelled = 4
	} // enum NLPaymentStatuses

	public enum LoanTransactionMethods {
		Unknown = 0, // DB table id
		Pacnet = 1,
		Auto = 2,
		Manual = 3,
		Cheque = 4,
		Cash = 5,
		NonCash = 6,
		BankTransfer = 7,
		Other = 8,
		CustomerAuto = 9,
		WriteOff = 11,
		ChargeBack = 12,
		WrongPayment = 13,
		SystemRepay = 14
	} //enum LoanTransactionMethods




} // namespace