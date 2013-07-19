using EzBob.PayPalServiceLib;

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
	    private readonly IPayPalMarketplaceSettings _config;

	    public PayPalDetailsRepository(ISession session, IPayPalMarketplaceSettings config )
            : base(session)
        {
            _config = config;
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

        public IEnumerable<PayPalGeneralDetailDataRow> IncomeDetails(MP_CustomerMarketPlace marketplace)
        {
	        return _session.GetNamedQuery("GetPayPalIncomeDetails")
                .SetParameter("marketplaceId", marketplace.Id)
                .Future<PayPalGeneralDetailDataRow>();
        }
        public IEnumerable<PayPalGeneralDetailDataRow> TotalExpenses(MP_CustomerMarketPlace marketplace)
        {
            return _session.GetNamedQuery("GetPayPalTotalExpensesByMarketplace")
                .SetParameter("marketplaceId", marketplace.Id)
                .List<PayPalGeneralDetailDataRow>();
        }
        public IEnumerable<PayPalGeneralDetailDataRow> ExpensesDetails(MP_CustomerMarketPlace marketplace)
        {
            return _session.GetNamedQuery("GetPayPalExpensesDetails")
                .SetParameter("marketplaceId", marketplace.Id)
                .Future<PayPalGeneralDetailDataRow>();
        }

        public IEnumerable<PayPalGeneralDetailDataRow> TotalIncome(MP_CustomerMarketPlace marketplace)
        {
			return _session.GetNamedQuery("GetPayPalTotalIncomeByMarketplace")
                .SetParameter("marketplaceId", marketplace.Id)
				.Future<PayPalGeneralDetailDataRow>();
        }

        public IEnumerable<PayPalGeneralDetailDataRow> TotalTransactions(MP_CustomerMarketPlace marketplace)
		{
			return _session.GetNamedQuery("GetPayPalTotalTransactionsByMarketplace")
				.SetParameter("marketplaceId", marketplace.Id)
				.Future<PayPalGeneralDetailDataRow>();
        }

        public PayPalDetailsModel GetDetails(MP_CustomerMarketPlace marketplace)
        {
            var details = new PayPalDetailsModel();

            details.Marketplace = marketplace;

            details.TotalIncome = new PayPalGeneralDetailDataRow();
            details.TotalExpenses = new PayPalGeneralDetailDataRow();
            details.TotalTransactions = new PayPalGeneralDetailDataRow();

            details.DetailIncome = new List<PayPalGeneralDetailDataRow>();
            details.DetailExpenses = new List<PayPalGeneralDetailDataRow>();

            details.EnableCategories = _config.EnableCategories;

            if (_config.EnableCategories)
            {
                var totalIncome = TotalIncome(marketplace);
                var totalExpenses = TotalExpenses(marketplace);
                var totalTransactions = TotalTransactions(marketplace);
                
                details.TotalIncome =  totalIncome.FirstOrDefault();
                details.TotalExpenses = totalExpenses.FirstOrDefault();
                details.TotalTransactions = totalTransactions.FirstOrDefault();

                details.DetailIncome = IncomeDetails(marketplace);
                details.DetailExpenses = ExpensesDetails(marketplace);
            }

            return details;
        }
    }
}