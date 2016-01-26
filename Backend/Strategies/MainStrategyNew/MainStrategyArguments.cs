namespace Ezbob.Backend.Strategies.MainStrategyNew {
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
	} // class MainStrategyArguments
} // namespace
