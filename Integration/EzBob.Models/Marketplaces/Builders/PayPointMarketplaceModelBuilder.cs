using System;
using System.Linq;
using EZBob.DatabaseLib.Model.Database;
using EzBob.Web.Areas.Customer.Models;
using NHibernate;
using NHibernate.Linq;
using System.Globalization;

namespace EzBob.Models.Marketplaces.Builders
{
	class PayPointMarketplaceModelBuilder : MarketplaceModelBuilder
	{
		public PayPointMarketplaceModelBuilder(ISession session)
			: base(session)
		{
		}

		public override PaymentAccountsModel GetPaymentAccountModel(MP_CustomerMarketPlace mp, MarketPlaceModel model, DateTime? history)
		{
			return CreatePayPointAccountModelModel(mp, history);
		}

		public PaymentAccountsModel CreatePayPointAccountModelModel(MP_CustomerMarketPlace m, DateTime? history)
		{
			var av = GetAnalysisFunctionValues(m, history);

			var tnop = 0.0;
			var tnip = 0.0;
			var tc = 0;
			var mip = 0.0;
			if (av != null)
			{
				var tnipN = GetClosestToYear(av.Where(x => x.ParameterName == "Total Sum of Orders"));
				var tcN = GetClosestToYear(av.Where(x => x.ParameterName == "Num of Orders"));
				var mipN = GetMonth(av.Where(x => x.ParameterName == "Total Sum of Orders"));

				if (mipN != null) mip = Math.Abs(Convert.ToDouble(mipN.Value, CultureInfo.InvariantCulture));
				if (tnipN != null) tnip = Math.Abs(Convert.ToDouble(tnipN.Value, CultureInfo.InvariantCulture));
				if (tcN != null) tc = Convert.ToInt32(tcN.Value, CultureInfo.InvariantCulture);
			}

			var status = m.GetUpdatingStatus(history);

			var payPointModel = new PaymentAccountsModel
			{
				displayName = m.DisplayName,
				TotalNetInPayments = tnip,
				MonthInPayments = mip,
				TotalNetOutPayments = tnop,
				TransactionsNumber = tc,
				id = m.Id,
				Status = status,
			};
			return payPointModel;
		}

		public override DateTime? GetSeniority(MP_CustomerMarketPlace mp)
		{
			var s = _session.Query<MP_PayPointOrderItem>()
				.Where(oi => oi.Order.CustomerMarketPlace.Id == mp.Id)
				.Where(oi => oi.date != null)
				.Select(oi => oi.date);
			return !s.Any() ? (DateTime?)null : s.Min();
		}

		public override DateTime? GetLastTransaction(MP_CustomerMarketPlace mp)
		{
			var s = _session.Query<MP_PayPointOrderItem>()
				.Where(oi => oi.Order.CustomerMarketPlace.Id == mp.Id)
				.Where(oi => oi.date != null);

			if (s.Count() != 0)
			{
				return s.Max(oi => oi.date);
			}

			return null;
		}
	}
}