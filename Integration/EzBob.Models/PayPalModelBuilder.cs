namespace EzBob.Models
{
	using System;
	using System.Globalization;
	using System.Linq;
	using EZBob.DatabaseLib.Model.Database;
	using Marketplaces.Builders;
	using NHibernate;
	using Web.Areas.Customer.Models;
	using Web.Areas.Underwriter.Models;

	public class PayPalModelBuilder : MarketplaceModelBuilder
	{
		public PayPalModelBuilder(ISession session) : base(session)
		{
		}

		public static PayPalAccountModel CreatePayPal(MP_CustomerMarketPlace mp, DateTime? history)
		{
			var generalInfo = CreatePayPalAccountModel(mp, history);

			var model = new PayPalAccountModel
			{
				GeneralInfo = generalInfo,
				PersonalInfo = new PayPalAccountInfoModel(mp.PersonalInfo),
			};

			return model;
		}

		public static PaymentAccountsModel CreatePayPalAccountModel(MP_CustomerMarketPlace m, DateTime? history = null)
		{
			var av = GetAnalysisFunctionValues(m, history);

			var tnop = 0.0;
			var tnip = 0.0;
			var tc = 0;
			var mip = 0.0;
			if (av != null)
			{
				var tnipN = GetClosestToYear(av.Where(x => x.ParameterName == "Total Net In Payments"));
				var tnopN = GetClosestToYear(av.Where(x => x.ParameterName == "Total Net Out Payments"));
				var tcN = GetClosestToYear(av.Where(x => x.ParameterName == "Transactions Number"));
				var mipN = GetMonth(av.Where(x => x.ParameterName == "Total Net In Payments"));

				if (mipN != null) mip = Math.Abs(Convert.ToDouble(mipN.Value, CultureInfo.InvariantCulture));
				if (tnipN != null) tnip = Math.Abs(Convert.ToDouble(tnipN.Value, CultureInfo.InvariantCulture));
				if (tnopN != null) tnop = Math.Abs(Convert.ToDouble(tnopN.Value, CultureInfo.InvariantCulture));
				if (tcN != null) tc = Convert.ToInt32(tcN.Value, CultureInfo.InvariantCulture);
			}

			var status = m.GetUpdatingStatus();

			var payPalModel = new PaymentAccountsModel
			{
				displayName = m.DisplayName,
				TotalNetInPayments = tnip,
				MonthInPayments = mip,
				TotalNetOutPayments = tnop,
				TransactionsNumber = tc,
				id = m.Id,
				Status = status,
                IsNew = m.IsNew
			};
			return payPalModel;
		}

		
	}
}