namespace EzBob.Web.Code {
	using Ezbob.Backend.Models;
	using ServiceClientProxy;
	using ServiceClientProxy.EzServiceReference;

	public class MainStrategyClient {
		public MainStrategyClient(
			int underwriterID,
			int customerID,
			bool isAvoid,
			NewCreditLineOption newCreditLineOption,
			long? cashRequestID,
			EZBob.DatabaseLib.Model.Database.CashRequestOriginator? cashRequestOriginator
		) {
			this.underwriterID = underwriterID;
			this.customerID = customerID;
			this.avoidAutoDecision = isAvoid ? 1 : 0;
			this.newCreditLineOption = newCreditLineOption;
			this.cashRequestID = cashRequestID;
			this.cashRequestOriginator = cashRequestOriginator == null
				? (ServiceClientProxy.EzServiceReference.CashRequestOriginator?)null
				: (ServiceClientProxy.EzServiceReference.CashRequestOriginator)cashRequestOriginator.Value;
			this.serviceClient = new ServiceClient();
		} // constructor

		public ActionMetaData ExecuteSync() {
			return this.serviceClient.Instance.MainStrategySync(
				this.underwriterID,
				this.customerID,
				this.newCreditLineOption,
				this.avoidAutoDecision,
				this.cashRequestID,
				this.cashRequestOriginator
			);
		} // ExecuteSync

		public ActionMetaData ExecuteAsync() {
			return this.serviceClient.Instance.MainStrategyAsync(
				this.underwriterID,
				this.customerID,
				this.newCreditLineOption,
				this.avoidAutoDecision,
				this.cashRequestID,
				this.cashRequestOriginator
			);
		} // ExecuteAsync

		private readonly int underwriterID;
		private readonly int customerID;
		private readonly int avoidAutoDecision;
		private readonly NewCreditLineOption newCreditLineOption;
		private readonly long? cashRequestID;
		private readonly ServiceClientProxy.EzServiceReference.CashRequestOriginator? cashRequestOriginator;

		private readonly ServiceClient serviceClient;
	} // class MainStrategyClient
} // namespace