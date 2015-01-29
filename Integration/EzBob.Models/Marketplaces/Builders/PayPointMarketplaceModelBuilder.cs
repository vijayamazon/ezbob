namespace EzBob.Models.Marketplaces.Builders {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Common;
	using EZBob.DatabaseLib.Model.Database;
	using NHibernate.Linq;

	class PayPointMarketplaceModelBuilder : MarketplaceModelBuilder {
		public PayPointMarketplaceModelBuilder() : base(null) { }

		public override string GetUrl(MP_CustomerMarketPlace mp, IMarketPlaceSecurityInfo securityInfo) {
			return "#";
		}

		public PaymentAccountsModel CreatePayPointAccountModelModel(MP_CustomerMarketPlace m, DateTime? history, List<IAnalysisDataParameterInfo> av) {
			var payPointModel = new PayPointAccountsModel(m, history);
			payPointModel.Load(av);
			return payPointModel;
		}

		public override DateTime? GetLastTransaction(MP_CustomerMarketPlace mp) {
			var s = _session.Query<MP_PayPointOrderItem>()
				.Where(oi => oi.Order.CustomerMarketPlace.Id == mp.Id)
				.Where(oi => oi.date != null);

			if (s.Count() != 0)
				return s.Max(oi => oi.date);

			return null;
		}

		protected override PaymentAccountsModel GetPaymentAccountModel(MP_CustomerMarketPlace mp, MarketPlaceModel model, DateTime? history, List<IAnalysisDataParameterInfo> av) {
			return CreatePayPointAccountModelModel(mp, history, av);
		}
		public override DateTime? GetSeniority(MP_CustomerMarketPlace mp) {
			var s = _session.Query<MP_PayPointOrderItem>()
				.Where(oi => oi.Order.CustomerMarketPlace.Id == mp.Id)
				.Where(oi => oi.date != null)
				.Select(oi => oi.date);
			return !s.Any() ? null : s.Min();
		}
	}
}