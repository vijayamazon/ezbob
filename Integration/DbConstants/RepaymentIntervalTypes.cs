namespace DbConstants {
	// Numeric value: number of days in the interval, except Month (where it can vary).
	public enum RepaymentIntervalTypes {
		Month   = 0,
		Day     = 1,
		Week    = 7,
		TenDays = 10,
	} // enum RepaymentIntervalTypes

    public enum RepaymentIntervalTypesId {
        Month = 1,
        Day = 2,
        Week = 3,
        TenDays = 4,
    } // enum RepaymentIntervalTypesId
} // namespace
