using Ezbob.Database;
using Ezbob.Logger;

namespace EzBob.Backend.Strategies {
	using Models;

	public class FirstOfMonthStatusNotifier : AStrategy {
		public FirstOfMonthStatusNotifier(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {}

		public override string Name { get { return "First Of Month Status Notifier"; } } // Name

		public override void Execute() {
			new FirstOfMonthStatusStrategyHelper().SendFirstOfMonthStatusMail();
		} // Execute
	} // class FirstOfMonthStatusNotifier
} // namespace
