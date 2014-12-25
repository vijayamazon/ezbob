namespace EzBob.Models.Marketplaces.Builders {
	using System;
	using System.Linq;
	using EZBob.DatabaseLib.Model.Database;
	using NHibernate.Linq;

	class PayPointMarketplaceModelBuilder : MarketplaceModelBuilder {
		public PayPointMarketplaceModelBuilder() : base(null) { }

		public PaymentAccountsModel CreatePayPointAccountModelModel(MP_CustomerMarketPlace m, DateTime? history) {
			var status = m.GetUpdatingStatus(history);

			var payPointModel = new PaymentAccountsModel {
				displayName = m.DisplayName,
				MonthInPayments = 0, // TODO: if this connector is used fill this number with real data
				TotalNetInPayments = 0, // TODO: if this connector is used fill this number with real data
				TotalNetOutPayments = 0, // TODO: if this connector is used fill this number with real data
				TransactionsNumber = 0, // TODO: if this connector is used fill this number with real data
				id = m.Id,
				Status = status,
			};

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

		public override PaymentAccountsModel GetPaymentAccountModel(MP_CustomerMarketPlace mp, MarketPlaceModel model, DateTime? history) {
			return CreatePayPointAccountModelModel(mp, history);
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