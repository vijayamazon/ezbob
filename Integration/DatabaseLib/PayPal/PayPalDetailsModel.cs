using System.Collections.Generic;
using EZBob.DatabaseLib.Model.Database;

namespace EZBob.DatabaseLib.PayPal
{
    public class PayPalDetailsModel
    {
        public MP_CustomerMarketPlace Marketplace { get; set; }

        public PayPalGeneralDetailDataRow TotalIncome { get; set; }

        public PayPalGeneralDetailDataRow TotalExpenses { get; set; }

        public PayPalGeneralDetailDataRow TotalTransactions { get; set; }

        public IEnumerable<PayPalGeneralDetailDataRow> DetailIncome { get; set; }

        public IEnumerable<PayPalGeneralDetailDataRow> DetailExpenses { get; set; }
    }
}