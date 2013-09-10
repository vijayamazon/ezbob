namespace EzBob.Models
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using EZBob.DatabaseLib.Model.Database;
	using Web.Areas.Customer.Models;
	using Web.Areas.Underwriter.Models;
	using CommonLib.TimePeriodLogic;
	using EZBob.DatabaseLib;
	
	public class PayPalModelBuilder
	{
		public static PayPalAccountModel Create(MP_CustomerMarketPlace mp)
		{
			var generalInfo = CreatePayPalAccountModelModel(mp);

			var model = new PayPalAccountModel
			{
				GeneralInfo = generalInfo,
				PersonalInfo = new PayPalAccountInfoModel(mp.PersonalInfo),
			};

			return model;
		}

		public static PaymentAccountsModel CreatePayPalAccountModelModel(MP_CustomerMarketPlace m)
		{
			var values = RetrieveDataHelper.GetAnalysisValuesByCustomerMarketPlace(m.Id);
			var analisysFunction = values;
			var av = values.Data.FirstOrDefault(x => x.Key == analisysFunction.Data.Max(y => y.Key)).Value;

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

		private static IAnalysisDataParameterInfo GetMonth(IEnumerable<IAnalysisDataParameterInfo> firstOrDefault)
		{
			foreach (var x in firstOrDefault)
			{
				switch (x.TimePeriod.TimePeriodType)
				{
					case TimePeriodEnum.Month:
						return x;
				}
			}
			return null;
		}



		private static IAnalysisDataParameterInfo GetClosestToYear(IEnumerable<IAnalysisDataParameterInfo> firstOrDefault)
		{
			int closestTime = 0;
			IAnalysisDataParameterInfo closestSoFar = null;
			foreach (var x in firstOrDefault)
			{
				switch (x.TimePeriod.TimePeriodType)
				{
					case TimePeriodEnum.Year:
						return x;
					case TimePeriodEnum.Month6:
						closestSoFar = x;
						closestTime = 6;
						break;
					case TimePeriodEnum.Month3:
						if (closestTime < 6)
						{
							closestSoFar = x;
							closestTime = 3;
						}
						break;
					case TimePeriodEnum.Month:
						if (closestTime < 3)
						{
							closestSoFar = x;
							closestTime = 1;
						}
						break;
				}
			}

			return closestSoFar;
		}
	}
}