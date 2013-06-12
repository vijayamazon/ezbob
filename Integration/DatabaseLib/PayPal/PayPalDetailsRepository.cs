using System.Collections.Generic;
using System.Linq;
using ApplicationMng.Repository;
using EZBob.DatabaseLib.Model.Database;
using NHibernate;

namespace EZBob.DatabaseLib.PayPal
{
    public class PayPalDetailsRepository : NHibernateRepositoryBase<PayPalGeneralDetailDataRow>, IPayPalDetailsRepository
    {
        public PayPalDetailsRepository(ISession session)
            : base(session)
        {
        }

        public IEnumerable<PayPalGeneralDetailDataRow> IncomeDetails(int marketPlaceId)
        {
            return _session.GetNamedQuery("GetPayPalIncomeDetails")
                .SetParameter("marketplaceId", marketPlaceId)
                .Future<PayPalGeneralDetailDataRow>();
        }
        public IEnumerable<PayPalGeneralDetailDataRow> TotalExpenses(int marketPlaceId)
        {
            return _session.GetNamedQuery("GetPayPalTotalExpensesByMarketplace")
                .SetParameter("marketplaceId", marketPlaceId)
                .Future<PayPalGeneralDetailDataRow>();
        }
        public IEnumerable<PayPalGeneralDetailDataRow> ExpensesDetails(int marketPlaceId)
        {
            return _session.GetNamedQuery("GetPayPalExpensesDetails")
                .SetParameter("marketplaceId", marketPlaceId)
                .Future<PayPalGeneralDetailDataRow>();
        }

        public IEnumerable<PayPalGeneralDetailDataRow> TotalIncome(int marketPlaceId)
        {
            return _session.GetNamedQuery("GetPayPalTotalIncomeByMarketplace")
                .SetParameter("marketplaceId", marketPlaceId)
                .Future<PayPalGeneralDetailDataRow>();
        }

        public IEnumerable<PayPalGeneralDetailDataRow> TotalTransactions(int marketPlaceId)
        {
            return _session.GetNamedQuery("GetPayPalTotalTransactionsByMarketplace")
                .SetParameter("marketplaceId", marketPlaceId)
                .Future<PayPalGeneralDetailDataRow>();
        }

        public PayPalDetailsModel GetDetails(int id)
        {
            var details = new PayPalDetailsModel();

            details.Marketplace = _session.Get<MP_CustomerMarketPlace>(id);
            var totalIncome = TotalIncome(id);
            var totalExpenses = TotalExpenses(id);
            var totalTransactions = TotalTransactions(id);

            details.DetailIncome = IncomeDetails(id);
            details.DetailExpenses = ExpensesDetails(id);

            details.TotalIncome = totalIncome.FirstOrDefault();            
            details.TotalExpenses = totalExpenses.FirstOrDefault();            
            details.TotalTransactions = totalTransactions.FirstOrDefault();

            return details;
        }
    }
}