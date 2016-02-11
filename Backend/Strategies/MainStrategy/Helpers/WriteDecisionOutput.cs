namespace Ezbob.Backend.Strategies.MainStrategy.Helpers {
	using EZBob.DatabaseLib.Model.Database;

	internal class WriteDecisionOutput {
		public CreditResultStatus? CreditResult { get; set; }
		public Status? UserStatus { get; set; }
		public SystemDecision? SystemDecision { get; set; }
	} // class WriteDecisionOutput
} // namespace
