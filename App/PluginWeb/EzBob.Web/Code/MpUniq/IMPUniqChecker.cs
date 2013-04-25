using System;
using EZBob.DatabaseLib.DatabaseWrapper;
using EZBob.DatabaseLib.Model.Database;

namespace EzBob.Web.Code.MpUniq {
	public interface IMPUniqChecker {
		void Check(Guid marketplaceType, Customer customer, string token);
		void Check(Guid marketplaceType, Customer customer, string login, string url);
	} // IMPUniqChecker

	public abstract class AMPUniqChecker : IMPUniqChecker {
		public virtual void Check(Guid marketplaceType, Customer customer, string login, string url) {
			this.Check(marketplaceType, customer, login);
		} // Check

		public abstract void Check(Guid marketplaceType, Customer customer, string token);
	} // AMPUniqChecker
}