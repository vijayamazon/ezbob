namespace Ezbob.Backend.Strategies.OpenPlatform.DAL.Contract
{
	using Ezbob.Backend.Models.Investor;

	public interface IInvestorCashRequestDAL {
       InvestorLoanCashRequest GetInvestorLoanCashRequest(long cashRequestID);
    }
}
