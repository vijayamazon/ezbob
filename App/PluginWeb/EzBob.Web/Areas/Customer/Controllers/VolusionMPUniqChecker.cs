using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EZBob.DatabaseLib.Model.Database.Repository;
using EZBob.DatabaseLib.Repository;
using EzBob.Web.Code.MpUniq;

namespace EzBob.Web.Areas.Customer.Controllers {
	public class VolusionMPUniqChecker : MPUniqChecker {
		#region constructor

		public VolusionMPUniqChecker(ICustomerMarketPlaceRepository customerMarketPlaceRepository, IMP_WhiteListRepository whiteList)
			: base(customerMarketPlaceRepository, whiteList)
		{} // constructor

		#endregion constructor

		#region method Check

		public override void Check(Guid marketplaceType, EZBob.DatabaseLib.Model.Database.Customer customer, string token) {
			throw new NotImplementedException("VolusionMPUniqChecker does not support Check(market place, customer, token");
		} // Check

		#endregion method Check

		#region method Check

		public override void Check(Guid marketplaceType, EZBob.DatabaseLib.Model.Database.Customer customer, string login, string url) {
			base.Check(marketplaceType, customer, login + ":|:" + url);
		} // Check

		#endregion method Check
	} // VolusionMPUniqChecker
} // namespace
