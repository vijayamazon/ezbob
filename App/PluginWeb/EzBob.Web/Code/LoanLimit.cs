namespace EzBob.Web.Code
{
	using System;
	using System.Linq;
	using ConfigManager;
	using NHibernateWrapper.Web;

    public class LoanLimit
    {
        private readonly IWorkplaceContext _context;

        public LoanLimit(IWorkplaceContext context)
        {
            _context = context;
        }

        public void Check(double amount)
        {
            Check((decimal)amount);
        }
        public void Check(decimal amount)
        {
	        int xMinLoan = CurrentValues.Instance.XMinLoan;
			if (amount < 0 || amount < xMinLoan || amount > GetMaxLimit())
            {
				throw new ArgumentException(string.Format("Amount is more then {0} or less then {1}", GetMaxLimit(), xMinLoan));
            }
        }

        public int GetMaxLimit()
        {
            if (_context.User.Roles.Any(r => r.Name == "manager"))
            {
                return CurrentValues.Instance.ManagerMaxLoan;
            }
			return CurrentValues.Instance.MaxLoan;
        }
    }
}