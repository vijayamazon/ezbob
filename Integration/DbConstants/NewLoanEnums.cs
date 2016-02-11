namespace DbConstants {

	using System.ComponentModel;

	// Numeric value: number of days in the interval, except Month (where it can vary).
	public enum RepaymentIntervalTypes {
		[Description("0")] // DB table id
		Month = 1,
		[Description("1")]
		Day = 2,
		[Description("7")]
		Week = 3,
		[Description("10")]
		TenDays = 4
	} // enum RepaymentIntervalTypes


	public enum NLLoanStatuses {
		Pending = 1,// DB table id
		Live = 2, 
		Late = 3,
		PaidOff = 4,
		WriteOff = 6,
		//Default = 5,
		//DebtManagement = 7
	} // enum NLLoanStatuses
	

	public enum NLFeeTypes {
		None = 0,
		SetupFee = 1,		// DB table id
		ServicingFee = 2,	// distributed - requires different consideration in loan calculator
		ArrangementFee = 3, // Arrangement - requires different consideration in loan calculator (other types should be paif in the nearby installment)
		RolloverFee = 4,
		AdminFee = 5,
		LatePaymentFee = 6,
		PartialPaymentFee = 7,
		OtherCharge = 8
	} // enum FeeTypes

	
	public enum NLScheduleStatuses {
		[Description("Open")]
		StillToPay = 1, // db ID
		[Description("Late")]
		Late = 2,
		[Description("Paid")]
		Paid = 3,
		[Description("Deleted on reschedule (nothing was repaid before reschedule)")]
		DeletedOnReschedule = 4,
		[Description("Closed on reschedule (was partially repaid before reschedule)")]
		ClosedOnReschedule = 5,
		[Description("Was late on reschedule")]
		LateDeletedOnReschedule = 6
	} // enum NLScheduleStatuses

	public enum NLLoanTypes {
		[Description("Standard Loan")]
		StandardLoanType = 1, // DB table id
		[Description("HalfWay Loan")]
		HalfWayLoanType = 2,
		[Description("Alibaba Loan")]
		AlibabaLoanType = 3
	} // enum NLLoanTypes

	public enum NLLoanSources {
		Standard = 1, // DB table id
		EU = 2,
		COSME = 3
	}

	public enum NLPacnetTransactionStatuses {
		Submited = 1, // DB table id
		ConfigErrorMultipleCandidateChannels = 2,
		Error = 3,
		InProgress = 4,
		PaymentByCustomer = 5,
		Done = 6
	} //enum NLPacnetTransactionStatuses

	public enum NLPaypointTransactionStatuses {
		InProgress = 1, // DB table id
		Done = 2, 
		Error = 3,
		Unknown = 4
	} // enum NLPaypointTransactionStatuses
	
	public enum NLPaymentStatuses {
		Error = 1,				// DB table id => Error, Unknown
		Active = 2,				// => InProgress, Done
		ChargeBack = 3,			// cancel payment
		WrongPayment = 4		// cancel payment
	} // enum NLPaymentStatuses

	public enum NLPaymentDestinations {
		Rollover
	} // enum NLPaymentDestinations

	public enum NLLoanTransactionMethods {
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
		WriteOff = 10,
		SetupFeeOffset = 11, // used to offsetting setup fee,
		SystemRepay = 12,
	} //enum NLLoanTransactionMethods (table LoanTransactionMethods)

	/// <summary>
	/// used in "pay loan" flows to indicate throuhg which payment system current transaction done
	/// </summary>
	public enum NLPaymentSystemTypes {
		None,
		Paypoint,
		Pacnet
	} // NLMoneyTransactionTypes

	// duplicate of enum LoanAgreementTemplateType
	public enum NLLoanAgreementTemplateTypes {
		GuarantyAgreement = 1, // DB table id
		PreContractAgreement = 2,
		CreditActAgreement = 3,
		PrivateCompanyLoanAgreement = 4,

		AlibabaGuarantyAgreement = 5,
		AlibabaPreContractAgreement = 6,
		AlibabaCreditActAgreement = 7,
		AlibabaPrivateCompanyLoanAgreement = 8,
		AlibabaCreditFacility = 9,
	}

	// logic transaction statuses
	public enum NLFundTransferStatuses {
		Pending = 1, // DB table id
		Active = 2,
		Deleted = 3
	}

	public enum NLLoanFormulas {
		[Description("1")] // IsActive
		EqualPrincipal = 1,	// DB table id
		[Description("0")] // 
		FixedPayment = 2 // used for "out of loan agreement" rescheduling, not supported yet (15 October 2015)
	}




} // namespace