using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.PyaPalDetails;
using EzBob.Web.Areas.Customer.Models;
using EzBob.Web.Areas.Underwriter.Models;
using StructureMap;

namespace EzBob.Models
{
    public class PayPalModelBuilder
    {
        public static PayPalAccountModel Create(MP_CustomerMarketPlace mp)
        {
            var _payPalDetails = ObjectFactory.GetInstance<IPayPalDetailsRepository>();
            var details = _payPalDetails.GetDetails(mp.Id);

            var payments = new PayPalAccountGeneralPaymentsInfoModel();
            var total = new List<PayPalGeneralDataRowModel>();


            var detailIncome = details.DetailIncome.Select(row => new PayPalGeneralDataRowModel(row)).ToList();
            var detailExpenses = details.DetailExpenses.Select(row => new PayPalGeneralDataRowModel(row)).ToList();

            total.Add(new PayPalGeneralDataRowModel(details.TotalIncome));
            total.Add(new PayPalGeneralDataRowModel(details.TotalExpenses));
            total.Add(new PayPalGeneralDataRowModel(details.TotalTransactions) { Pounds = false });

            payments.Data = total;

            var detailPayments = new PayPalAccountDetailPaymentsInfoModel
            {
                Income = ProcessPayments(detailIncome, details),
                Expenses = ProcessPayments(detailExpenses, details)
            };

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
            if (av != null)
            {
                var tnipN =
                    av.FirstOrDefault(
                        x => x.ParameterName == "Total Net In Payments" && x.CountMonths == av.Max(i => i.CountMonths));
                var tnopN =
                    av.FirstOrDefault(
                        x => x.ParameterName == "Total Net Out Payments" && x.CountMonths == av.Max(i => i.CountMonths));
                var tcN =
                    av.FirstOrDefault(
                        x => x.ParameterName == "Transactions Number" && x.CountMonths == av.Max(i => i.CountMonths));

                if (tnipN != null) tnip = Math.Abs(Convert.ToDouble(tnipN.Value, CultureInfo.InvariantCulture));
                if (tnopN != null) tnop = Math.Abs(Convert.ToDouble(tnopN.Value, CultureInfo.InvariantCulture));
                if (tcN != null) tc = Convert.ToInt32(tcN.Value, CultureInfo.InvariantCulture);
            }

            var transactionsMinDate = m.PayPalTransactions.Any()
                                          ? m.PayPalTransactions.Min(
                                              x =>
                                              x.TransactionItems.Any() ? x.TransactionItems.Min(y => y.Created) : DateTime.Now)
                                          : DateTime.Now;

            var seniority = DateTime.Now - transactionsMinDate;

            var status = m.GetUpdatingStatus();

            var payPalModel = new PaymentAccountsModel
            {
                displayName = m.DisplayName,
                TotalNetInPayments = tnip,
                TotalNetOutPayments = tnop,
                TransactionsNumber = tc,
                id = m.Id,
                Seniority = (seniority.Days * 1.0 / 30).ToString(CultureInfo.InvariantCulture),
                Status = status
            };
            return payPalModel;
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