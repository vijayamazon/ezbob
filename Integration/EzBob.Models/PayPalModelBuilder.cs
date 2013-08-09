namespace EzBob.Models
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.PayPal;
	using Web.Areas.Customer.Models;
	using Web.Areas.Underwriter.Models;
	using StructureMap;
	using CommonLib.TimePeriodLogic;
	using EZBob.DatabaseLib;
	
	public class PayPalModelBuilder
	{
		public static PayPalAccountModel Create(MP_CustomerMarketPlace mp)
		{
			var payPalDetails = ObjectFactory.GetInstance<IPayPalDetailsRepository>();

			var details = payPalDetails.GetDetails(mp);

			var payments = new PayPalAccountGeneralPaymentsInfoModel();
			var total = new List<PayPalGeneralDataRowModel>();


			var detailIncome = details.DetailIncome.Select(row => new PayPalGeneralDataRowModel(row)).ToList();
			var detailExpenses = details.DetailExpenses.Select(row => new PayPalGeneralDataRowModel(row)).ToList();

			total.Add(new PayPalGeneralDataRowModel(details.TotalIncome));
			total.Add(new PayPalGeneralDataRowModel(details.TotalExpenses));
			total.Add(new PayPalGeneralDataRowModel(details.TotalTransactions) { Pounds = false });

			payments.Data = total;

			var detailPayments = new PayPalAccountDetailPaymentsInfoModel();

			if (details.EnableCategories)
			{
				detailPayments = new PayPalAccountDetailPaymentsInfoModel
					{
						Income = ProcessPayments(detailIncome, details),
						Expenses = ProcessPayments(detailExpenses, details),
					};
			}

			var generalInfo = CreatePayPalAccountModelModel(mp);

			var model = new PayPalAccountModel
			{
				GeneralInfo = generalInfo,
				PersonalInfo = new PayPalAccountInfoModel(details.Marketplace.PersonalInfo),
				DetailPayments = detailPayments,
				Payments = payments
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
				Status = status
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

		private static IEnumerable<PayPalGeneralDataRowModel> ProcessPayments(ICollection<PayPalGeneralDataRowModel> payments, PayPalDetailsModel details)
		{
			var percents = new List<double?>();
			foreach (var payPalGeneralDataRowModel in payments)
			{
				var v1 = payPalGeneralDataRowModel.FirstNotNull();
				var v2 = details.TotalIncome.FirstNotNull();
				var percent = (v1 / v2) * 100;
				payPalGeneralDataRowModel.Type = string.Format("{0} ({1:0.00}%)", payPalGeneralDataRowModel.Type, percent ?? 0);
				payPalGeneralDataRowModel.Percent = percent;
				percents.Add(percent);
			}

			var otherIncomePercent = 100 - percents.Sum();
			var otherIncomeRow = new PayPalGeneralDataRowModel
			{
				Type = string.Format("Other ({0:0.00}%)", otherIncomePercent),
				M1 = details.TotalIncome.M1 - payments.Sum(x => x.M1),
				M3 = details.TotalIncome.M3 - payments.Sum(x => x.M3),
				M6 = details.TotalIncome.M6 - payments.Sum(x => x.M6),
				M12 = details.TotalIncome.M12 - payments.Sum(x => x.M12),
				M15 = details.TotalIncome.M15 - payments.Sum(x => x.M15),
				M18 = details.TotalIncome.M18 - payments.Sum(x => x.M18),
				M24 = details.TotalIncome.M24 - payments.Sum(x => x.M24),
				M24Plus = details.TotalIncome.M24Plus - payments.Sum(x => x.M24Plus)
			};
			payments.Add(otherIncomeRow);
			return payments.OrderByDescending(x => x.Percent).ToList();
		}
	}
}