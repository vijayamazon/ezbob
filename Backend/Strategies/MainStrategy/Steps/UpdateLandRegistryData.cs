namespace Ezbob.Backend.Strategies.MainStrategy.Steps {
	using Ezbob.Backend.Models;
	using Ezbob.Backend.Strategies.MainStrategy.Helpers;

	internal class UpdateLandRegistryData : AOneExitStep {
		public UpdateLandRegistryData(
			string outerContextDescription,
			int customerID,
			string customerFullName,
			bool customerIsAutoRejected,
			string customerPropertyStatusDescription,
			bool isOwnerOfMainAddress,
			bool isOwnerOfOtherProperties,
			NewCreditLineOption newCreditLineOption
		) : base(outerContextDescription) {
			this.customerID = customerID;
			this.customerFullName = customerFullName;
			this.customerIsAutoRejected = customerIsAutoRejected;
			this.customerPropertyStatusDescription = customerPropertyStatusDescription;
			this.isOwnerOfMainAddress = isOwnerOfMainAddress;
			this.isOwnerOfOtherProperties = isOwnerOfOtherProperties;
			this.newCreditLineOption = newCreditLineOption;
		} // constructor

		protected override void ExecuteStep() {
			new MainStrategyUpdateLandRegistryData(
				this.customerID,
				this.customerFullName,
				this.customerIsAutoRejected,
				this.customerPropertyStatusDescription,
				this.isOwnerOfMainAddress,
				this.isOwnerOfOtherProperties,
				this.newCreditLineOption
			).Execute();
		} // ExecuteStep

		private readonly int customerID;
		private readonly string customerFullName;
		private readonly bool customerIsAutoRejected;
		private readonly string customerPropertyStatusDescription;
		private readonly bool isOwnerOfMainAddress;
		private readonly bool isOwnerOfOtherProperties;
		private readonly NewCreditLineOption newCreditLineOption;
	} // class UpdateLandRegistryData
} // namespace
