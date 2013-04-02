using System.Collections.Generic;
using ApplicationMng.Repository;
using EZBob.DatabaseLib.Model.Database;

namespace EZBob.DatabaseLib.PyaPalDetails
{
    public interface IPayPalDetailsRepository : IRepository<PayPalGeneralDetailDataRow>
    {
        IEnumerable<PayPalGeneralDetailDataRow> IncomeDetails(int marketPlaceId);
        IEnumerable<PayPalGeneralDetailDataRow> TotalExpenses(int marketPlaceId);
        IEnumerable<PayPalGeneralDetailDataRow> ExpensesDetails(int marketPlaceId);
        IEnumerable<PayPalGeneralDetailDataRow> TotalIncome(int marketPlaceId);
        IEnumerable<PayPalGeneralDetailDataRow> TotalTransactions(int marketPlaceId);
        PayPalDetailsModel GetDetails(int id);
    }
}
