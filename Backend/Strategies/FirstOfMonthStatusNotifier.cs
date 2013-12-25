namespace EzBob.Backend.Strategies {
	using Models;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class FirstOfMonthStatusNotifier : AStrategy {
		public FirstOfMonthStatusNotifier(AConnection oDb, ASafeLog oLog) : base(oDb, oLog) {}

		public override string Name { get { return "First Of Month Status Notifier"; } } // Name

		public override void Execute() {
			new FirstOfMonthStatusStrategyHelper().SendFirstOfMonthStatusMail();
		} // Execute
	} // class FirstOfMonthStatusNotifier
} // namespace
