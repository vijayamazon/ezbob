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
	} // class EzServiceImplementation
} // namespace EzService
