using System;
using EZBob.DatabaseLib.Model.Database.Repository;
using EZBob.DatabaseLib.Repository;
using EzBob.Web.Code.MpUniq;

namespace EzBob.Web.Areas.Customer.Controllers {
	public class PlayMPUniqChecker : MPUniqChecker {
		#region constructor

		public PlayMPUniqChecker(ICustomerMarketPlaceRepository customerMarketPlaceRepository, IMP_WhiteListRepository whiteList)
			: base(customerMarketPlaceRepository, whiteList)
		{} // constructor

		#endregion constructor

		#region method Check

		public override void Check(Guid marketplaceType, EZBob.DatabaseLib.Model.Database.Customer customer, string token) {
			throw new NotImplementedException("PlayMPUniqChecker does not support Check(market place, customer, token");
		} // Check

		#endregion method Check

		#region method Check

		public override void Check(Guid marketplaceType, EZBob.DatabaseLib.Model.Database.Customer customer, string login, string name) {
			base.Check(marketplaceType, customer, login + ":|:" + name);
		} // Check

		#endregion method Check
	} // PlayMPUniqChecker
} // namespace
