namespace DbConstants {
	// Numeric value: number of days in the interval, except Month (where it can vary).
	public enum RepaymentIntervalTypes {
		Month = 0,
		Day = 1,
		Week = 7,
		TenDays = 10,
	} // enum RepaymentIntervalTypes

	public enum RepaymentIntervalTypesId {
		Month = 1,
		Day = 2,
		Week = 3,
		TenDays = 4,
	} // enum RepaymentIntervalTypesId


	public enum NLLoanStatuses {
		Live = 1,
		Late = 2,
		PaidOff = 3,
		Pending = 4,
		Default = 5,
		WriteOff = 6,
		DebtManagement = 7,
		//1-14DaysMissed                                                                  	=	8,
		//15-30DaysMissed                                                                 	=	9,
		//31-45DaysMissed                                                                 	=	10,
		//46-90DaysMissed                                                                 	=	11,
		//60-90DaysMissed                                                                 	=	12,
		//90DaysMissed                                                                    	=	13,
		//Legal ??? claim process                                                         	=	14,
		//Legal - apply for judgment                                                      	=	15,
		//Legal: CCJ                                                                      	=	16,
		//Legal: bailiff                                                                  	=	17,
		//Legal: charging order                                                           	=	18,
		//Collection: Tracing                                                             	=	19,
		//Collection: Site Visit                                                          	=	20,
		//Legal ??? claim process                                                         	=	21,
		//Legal ??? claim process                                                         	=	22,
		//Legal ??? claim process                                                         	=	23     
	} // enum NLLoanStatuses


} // namespace
