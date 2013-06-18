namespace EZBob.DatabaseLib.PayPal
{
	using System.Data;
	using System.Collections.Generic;
	using System.Linq;
	using ApplicationMng.Repository;
	using Model.Database;
	using NHibernate;

	public class PayPalDetailsRepository : NHibernateRepositoryBase<PayPalGeneralDetailDataRow>, IPayPalDetailsRepository
    {
        public PayPalDetailsRepository(ISession session)
            : base(session)
		{
        }

		private double? GetDouble(object fieldValue)
		{
			double? res = null;
			if (fieldValue != null)
			{
				double tmp;
				if (double.TryParse(fieldValue.ToString(), out tmp))
				{
					res = tmp;
				}
			}

			return res;
		}

		private IEnumerable<PayPalGeneralDetailDataRow> GetResults(DataTable dt)
		{
			var results = new List<PayPalGeneralDetailDataRow>();
			foreach (DataRow row in dt.Rows)
			{
				var g = new PayPalGeneralDetailDataRow
				{
					Type = row["Payer"] != null ? row["Payer"].ToString() : null,
					M1 = GetDouble(row["M1"]),
					M3 = GetDouble(row["M3"]),
					M6 = GetDouble(row["M6"]),
					M12 = GetDouble(row["M12"]),
					M15 = GetDouble(row["M15"]),
					M18 = GetDouble(row["M18"]),
					M24 = GetDouble(row["M24"]),
					M24Plus = GetDouble(row["M24Plus"])
				};
				results.Add(g);
			}
			return results;
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
                .List<PayPalGeneralDetailDataRow>();
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