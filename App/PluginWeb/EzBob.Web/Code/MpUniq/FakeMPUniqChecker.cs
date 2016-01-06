namespace EzBob.Web.Code.MpUniq {
	using System;
	using EZBob.DatabaseLib.Model.Database;

	public class FakeMPUniqChecker : IMPUniqChecker {
		public virtual void Check(Guid marketplaceType, Customer customer, string token) {
		} // Check
	} // class FakeMPUniqChecker
} // namespace
