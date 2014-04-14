namespace Ezbob.Backend.Models {
	using System.Runtime.Serialization;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Converters;

	[DataContract]
	public class FinishWizardArgs {
		public FinishWizardArgs() {
			DoSendEmail = true;
			DoMain = true;
			DoFraud = true;
			NewCreditLineOption = NewCreditLineOption.UpdateEverythingAndApplyAutoRules;
			FraudMode = FraudMode.FullCheck;
			AvoidAutoDecision = 0;
			IsUnderwriterForced = false;
		} // constructor

		[DataMember]
		public int CustomerID { get; set; }

		[DataMember]
		public bool DoSendEmail { get; set; }

		[DataMember]
		public bool DoMain { get; set; }

		[DataMember]
		public bool DoFraud { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
		[DataMember]
		public NewCreditLineOption NewCreditLineOption { get; set; }

		[DataMember]
		public int AvoidAutoDecision { get; set; }

		[DataMember]
		public bool IsUnderwriterForced { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
		[DataMember]
		public FraudMode FraudMode { get; set; }
	} // class FinishWizardArgs
} // namespace Ezbob.Backend.Models
