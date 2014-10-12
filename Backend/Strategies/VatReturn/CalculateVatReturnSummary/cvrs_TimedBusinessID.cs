namespace EzBob.Backend.Strategies.VatReturn {
	using System;

	public partial class CalculateVatReturnSummary : AStrategy {
		private class TimedBusinessID {
			public int BusinessID;
			public DateTime Since;

			public void Update(int nBusinessID, DateTime oSince) {
				if (Since < oSince) {
					BusinessID = nBusinessID;
					Since = oSince;
				} //if
			} // Update
		} // TimedBusinessID
	} // class CalculateVatReturnSummary
} // namespace
