namespace Ezbob.Backend.Models {
	using System.Runtime.Serialization;
	using EZBob.DatabaseLib.Model.Database;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Converters;

	[DataContract(IsReference = true)]
	public class FinishWizardArgs {
		public FinishWizardArgs() {
			DoSendEmail = true;
			DoMain = true;
			DoFraud = true;
			NewCreditLineOption = NewCreditLineOption.UpdateEverythingAndApplyAutoRules;
			FraudMode = FraudMode.FullCheck;
			AvoidAutoDecision = 0;
			CashRequestOriginator = CashRequestOriginator.Other;
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

		[JsonConverter(typeof(StringEnumConverter))]
		[DataMember]
		public FraudMode FraudMode { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
		[DataMember]
		public CashRequestOriginator CashRequestOriginator { get; set; }

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>
		/// A string that represents the current object.
		/// </returns>
		public override string ToString() {
			return string.Format(
				"customer: {0}, send email: '{1}', do main: '{2}', do fraud: '{3}', '{4}', " +
				"avoid auto decision: '{5}', fraud mode: '{6}', originator: '{7}'",
				CustomerID,
				DoSendEmail ? "yes" : "no",
				DoMain ? "yes" : "no",
				DoFraud ? "yes" : "no",
				NewCreditLineOption,
				AvoidAutoDecision == 1 ? "yes" : "no",
				FraudMode,
				CashRequestOriginator
			);
		} // ToString
	} // class FinishWizardArgs
} // namespace Ezbob.Backend.Models
