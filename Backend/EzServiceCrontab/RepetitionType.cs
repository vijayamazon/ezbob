namespace EzServiceCrontab {
	/// <summary>
	/// Do not change numeric values - they are used as keys in DB.
	/// </summary>
	enum RepetitionType {
		// This is an illegal value for scheduling.
		Undefined = 0,

		Monthly = 1,
		Daily = 2,
		EveryXMinutes = 3,

		// Keep it last and without an explicit numeric value.
		Max
	} // enum RepetitionType
} // namespace
