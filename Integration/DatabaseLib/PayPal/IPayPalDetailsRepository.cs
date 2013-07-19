using System.Collections.Generic;
using ApplicationMng.Repository;
using EZBob.DatabaseLib.Model.Database;

namespace EZBob.DatabaseLib.PayPal
{
    public interface IPayPalDetailsRepository : IRepository<PayPalGeneralDetailDataRow>
    {
        IEnumerable<PayPalGeneralDetailDataRow> IncomeDetails(MP_CustomerMarketPlace marketplace);
        IEnumerable<PayPalGeneralDetailDataRow> TotalExpenses(MP_CustomerMarketPlace marketplace);
        IEnumerable<PayPalGeneralDetailDataRow> ExpensesDetails(MP_CustomerMarketPlace marketplace);
        IEnumerable<PayPalGeneralDetailDataRow> TotalIncome(MP_CustomerMarketPlace marketplace);
        IEnumerable<PayPalGeneralDetailDataRow> TotalTransactions(MP_CustomerMarketPlace marketplace);
        PayPalDetailsModel GetDetails(MP_CustomerMarketPlace marketplace);
    }
}
