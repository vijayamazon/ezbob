using System;
using System.Linq;
using EzBob.Web.Infrastructure;
using Scorto.Web;

namespace EzBob.Web.Code
{
    public class LoanLimit
    {
        private readonly IWorkplaceContext _context;
        private readonly IEzBobConfiguration _config;

        public LoanLimit(IWorkplaceContext context, IEzBobConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public void Check(double amount)
        {
            Check((decimal)amount);
        }
        public void Check(decimal amount)
        {
            if (amount < 0 || amount < _config.XMinLoan || amount > GetMaxLimit())
            {
                throw new ArgumentException(string.Format("Amount is more then {0} or less then {1}", GetMaxLimit(), _config.XMinLoan));
            }
        }

        public int GetMaxLimit()
        {
            if (_context.User.Roles.Any(r => r.Name == "manager"))
            {
                return _config.ManagerMaxLoan;
            }
            return _config.MaxLoan;
        }
    }
}