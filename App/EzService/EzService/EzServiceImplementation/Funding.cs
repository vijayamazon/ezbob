namespace EzService.EzServiceImplementation {
	using EzBob.Backend.Strategies.Misc;

	partial class EzServiceImplementation {
		public DecimalActionResult GetAvailableFunds(int underwriterId)
		{
			GetAvailableFunds instance;
			ActionMetaData result = ExecuteSync(out instance, 0, underwriterId);

			return new DecimalActionResult
			{
				MetaData = result,
				Value = instance.AvailableFunds
			};
		}
	} // class EzServiceImplementation
} // namespace EzService
