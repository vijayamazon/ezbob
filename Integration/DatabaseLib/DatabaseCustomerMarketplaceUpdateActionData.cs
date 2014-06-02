using System;
using EzBob.CommonLib;

namespace EZBob.DatabaseLib
{
	using Ezbob.Utils;

	internal class DatabaseCustomerMarketplaceUpdateActionData
	{
		public CustomerMarketplaceUpdateActionType ActionName { get; set; }
		public DateTime UpdatingStart { get; set; }
		public UpdateActionResultType? ControlValueName { get; set; }
		public object ControlValue { get; set; }
		public DateTime UpdatingEnd { get; set; }
		public string Error { get; set; }
		public RequestsCounterData RequestsCounter { get; set; }

		public ElapsedTimeInfo ElapsedTime { get; set; }
	}
}