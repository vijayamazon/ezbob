namespace EZBob.DatabaseLib.PayPal
{
	using System.Collections.Generic;
	using System.Linq;
	using ApplicationMng.Repository;
	using Model.Marketplaces.PayPal;
	using NHibernate;

	public interface IPayPalAggregationFormulaRepository : IRepository<MP_PayPalAggregationFormula>
	{
		List<MP_PayPalAggregationFormula> GetByFormulaName(string name);
		List<MP_PayPalAggregationFormula> GetByFormulaNum(int num);
	}

	public class PayPalAggregationFormulaRepository : NHibernateRepositoryBase<MP_PayPalAggregationFormula>, IPayPalAggregationFormulaRepository
	{
		public PayPalAggregationFormulaRepository(ISession session)
			: base(session)
		{
		}

		public List<MP_PayPalAggregationFormula> GetByFormulaName(string name)
		{
			return GetAll().Where(a => a.FormulaName == name).ToList();
		}

		public List<MP_PayPalAggregationFormula> GetByFormulaNum(int num)
		{
			return GetAll().Where(a => a.FormulaNum == num).ToList();
		}
	}
}