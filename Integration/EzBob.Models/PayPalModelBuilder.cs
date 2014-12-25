namespace EzBob.Models {
	using System;
	using System.Globalization;
	using System.Linq;
	using Ezbob.Models;
	using EZBob.DatabaseLib.Model.Database;
	using Marketplaces;
	using Marketplaces.Builders;
	using NHibernate;
	using Web.Areas.Underwriter.Models;

	public class PayPalModelBuilder : MarketplaceModelBuilder {
		public PayPalModelBuilder(ISession session)
			: base(session) {
		}

		public static PayPalAccountModel CreatePayPal(MP_CustomerMarketPlace mp, DateTime? history) {
			var generalInfo = CreatePayPalAccountModel(mp, history);

			var model = new PayPalAccountModel {
				GeneralInfo = generalInfo,
				PersonalInfo = new PayPalAccountInfoModel(mp.PersonalInfo),
			};

			return model;
		}

		public static PaymentAccountsModel CreatePayPalAccountModel(MP_CustomerMarketPlace m, DateTime? history = null) {
			var status = m.GetUpdatingStatus(history);

			var payPalModel = new PayPalPaymentAccountsModel {
				displayName = m.DisplayName,
				id = m.Id,
				Status = status,
				IsNew = m.IsNew
			};

			payPalModel.Load(m.Id, history, Library.Instance.DB);

			return payPalModel;
		}
	} // class PayPalModelBuilder
} // namespace
