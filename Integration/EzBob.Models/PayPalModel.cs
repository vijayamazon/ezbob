using System;
using System.Globalization;
using System.Linq;
using EZBob.DatabaseLib.Model.Database;

namespace EzBob.Web.Areas.Customer.Models
{
    public class PayPalModel : SimpleMarketPlaceModel
    {
        public int id { get; set; }
        public double TranzactionsNumber { get; set; }
        public double TotalNetInPayments { get; set; }
        public double TotalNetOutPayments { get; set; }
        public string Seniority { get; set; }
        public string Status { get; set; }

        public static PayPalModel CreatePayPalModel(MP_CustomerMarketPlace m)
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

            var status =
                (m.UpdatingStart != null && m.UpdatingEnd == null)
                    ? "In progress"
                    : (!String.IsNullOrEmpty(m.UpdateError))
                          ? "Error"
                          : "Done";

            var payPalModel = new PayPalModel
                                  {
                                      displayName = m.DisplayName,
                                      TotalNetInPayments = tnip,
                                      TotalNetOutPayments = tnop,
                                      TranzactionsNumber = tc,
                                      id = m.Id,
                                      Seniority = (seniority.Days*1.0/30).ToString(CultureInfo.InvariantCulture),
                                      Status = status
                                  };
            return payPalModel;
        }
    }
}