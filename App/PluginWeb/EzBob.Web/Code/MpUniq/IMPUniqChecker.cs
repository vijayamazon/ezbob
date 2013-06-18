using System;
using EZBob.DatabaseLib.Model.Database;

namespace EzBob.Web.Code.MpUniq {
	public interface IMPUniqChecker {
		void Check(Guid marketplaceType, Customer customer, string token);
	} // IMPUniqChecker
} // namespace
