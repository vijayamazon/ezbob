using System.Collections.Generic;
using System.Linq;
using EZBob.DatabaseLib.PyaPalDetails;
using EzBob.Web.Areas.Customer.Models;
using StructureMap;

namespace EzBob.Web.Areas.Underwriter.Models
{
    public class PayPalAccountModel
    {
        public PayPalModel GeneralInfo { get; set; }
        public PayPalAccountInfoModel PersonalInfo { get; set; }
        public PayPalAccountGeneralPaymentsInfoModel Payments { get; set; }
        public PayPalAccountDetailPaymentsInfoModel DetailPayments { get; set; }

        public static PayPalAccountModel Create(int id)
        {
            IPayPalDetailsRepository _payPalDetails = ObjectFactory.GetInstance<IPayPalDetailsRepository>();
            var details = _payPalDetails.GetDetails(id);

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

            var generalInfo = details.Marketplace.Customer.GetPayPalAccounts().FirstOrDefault(x => x.id == id);
            var model = new PayPalAccountModel { GeneralInfo = generalInfo, PersonalInfo = new PayPalAccountInfoModel(details.Marketplace.PersonalInfo), DetailPayments = detailPayments, Payments = payments };
            return model;
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