namespace Ezbob.Backend.Strategies.MainStrategy {
	using System.Runtime.Serialization;
	using Ezbob.Backend.Models;
	using EZBob.DatabaseLib.Model.Database;

	[DataContract(IsReference = true)]
	public class MainStrategyArguments {
		[DataMember]
		public int UnderwriterID { get; set; }

		[DataMember]
		public int CustomerID { get; set; }

		[DataMember]
		public NewCreditLineOption NewCreditLine { get; set; }

		[DataMember]
		public int AvoidAutoDecision { get; set; }

		[DataMember]
		public FinishWizardArgs FinishWizardArgs { get; set; }

		[DataMember]
		public long? CashRequestID { get; set; } // When old cash request is removed replace this with NLcashRequestID

		[DataMember]
		public CashRequestOriginator? CashRequestOriginator { get; set; }

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>
		/// A string that represents the current object.
		/// </returns>
		public override string ToString() {
			return string.Format(
				"UW: {0}, customer: {1}, '{2}', avoid auto decision = '{3}', " +
				" cash request = {4}, originator: {5}, finish wizard: '{6}'",
				UnderwriterID,
				CustomerID,
				NewCreditLine,
				AvoidAutoDecision == 1 ? "yes" : "no",
				CashRequestID == null ? "NULL" : CashRequestID.Value.ToString(),
				CashRequestOriginator == null ? "N/A" : CashRequestOriginator.ToString(),
				FinishWizardArgs == null ? "no" : FinishWizardArgs.ToString()
			);
		} // ToString
	} // class MainStrategyArguments
} // namespace
