namespace EzService.EzServiceImplementation {
	using Ezbob.Backend.Strategies.Misc;

	partial class EzServiceImplementation {
		public AvailableFundsActionResult GetAvailableFunds(int underwriterId)
		{
			GetAvailableFunds instance;
			ActionMetaData result = ExecuteSync(out instance, null, underwriterId > 0 ? underwriterId : (int?)null);

			return new AvailableFundsActionResult
			{
				MetaData = result,
				AvailableFunds = instance.AvailableFunds,
				ReservedAmount = instance.ReservedAmount
			};
		}

		public ActionMetaData RecordManualPacnetDeposit(int underwriterId, string underwriterName, int amount)
		{
			return ExecuteSync<RecordManualPacnetDeposit>(0, underwriterId, underwriterName, amount);
		}

		public ActionMetaData DisableCurrentManualPacnetDeposits(int underwriterId)
		{
			return ExecuteSync<DisableCurrentManualPacnetDeposits>(0, underwriterId);
		}

		public ActionMetaData VerifyEnoughAvailableFunds(int underwriterId, decimal deductAmount)
		{
			return ExecuteSync<VerifyEnoughAvailableFunds>(0, underwriterId, deductAmount);
		}

		public ActionMetaData TopUpDelivery(int underwriterId, decimal amount, int contentCase) {
			return Execute<TopUpDelivery>(null, underwriterId, underwriterId, amount, contentCase);
		}

		public ActionMetaData PacnetDelivery(int underwriterId, decimal amount) {
			return Execute<PacnetDelivery>(null, underwriterId, underwriterId, amount);
		}
	} // class EzServiceImplementation
} // namespace EzService
