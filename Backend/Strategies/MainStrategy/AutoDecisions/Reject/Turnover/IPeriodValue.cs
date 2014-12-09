namespace Ezbob.Backend.Strategies.MainStrategy.AutoDecisions.Reject.Turnover {
	internal interface IPeriodValue {
		void Add(Row r);
		decimal Value { get; }
	} // IPeriodValue
} // namespace
