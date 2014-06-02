namespace EzService.EzServiceImplementation {
	using EzBob.Backend.Strategies.Misc;

	partial class EzServiceImplementation {
		public AvailableFundsActionResult GetAvailableFunds(int underwriterId)
		{
			GetAvailableFunds instance;
			ActionMetaData result = ExecuteSync(out instance, 0, underwriterId);

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
	} // class EzServiceImplementation
} // namespace EzService
