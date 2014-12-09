namespace Ezbob.Backend.Strategies.Misc {
	using EzBob.Models;

	public class FirstOfMonthStatusNotifier : AStrategy {
		public override string Name { get { return "First Of Month Status Notifier"; } } // Name

		public override void Execute() {
			new FirstOfMonthStatusStrategyHelper().SendFirstOfMonthStatusMail();
		} // Execute
	} // class FirstOfMonthStatusNotifier
} // namespace
