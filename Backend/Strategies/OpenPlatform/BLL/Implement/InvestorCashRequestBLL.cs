namespace Ezbob.Backend.Strategies.OpenPlatform.BLL.Implement
{
	using Ezbob.Backend.Models.Investor;
	using Ezbob.Backend.Strategies.OpenPlatform.BLL.Contracts;
    using Ezbob.Backend.Strategies.OpenPlatform.DAL.Contract;
    using StructureMap.Attributes;

    public class InvestorCashRequestBLL : IInvestorCashRequestBLL
    {
        [SetterProperty]
        public IInvestorCashRequestDAL InvestorCashRequestDAL { get; set; }

        public InvestorLoanCashRequest GetInvestorLoanCashRequest(long cashRequestID) {
            return InvestorCashRequestDAL.GetInvestorLoanCashRequest(cashRequestID);
        }
    }
}
