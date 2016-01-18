namespace Ezbob.Backend.Strategies.OpenPlatform.BLL.Contracts
{
	using Ezbob.Backend.Models.Investor;
	
    public interface IInvestorCashRequestBLL {
        InvestorLoanCashRequest GetInvestorLoanCashRequest(long cashRequestID);
    }
}


