namespace EzBob.Web.Code.MpUniq {
	using System;
	using EZBob.DatabaseLib.Model.Database;

	public interface IMPUniqChecker {
		void Check(Guid marketplaceType, Customer customer, string token);
	} // IMPUniqChecker
} // namespace
