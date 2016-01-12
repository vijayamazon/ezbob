namespace Ezbob.Backend.Strategies.OpenPlatform.DAL.Contract
{
    using Ezbob.Backend.ModelsWithDB.Investor;

	public interface IInvestorCashRequestDAL {
       InvestorLoanCashRequest GetInvestorLoanCashRequest(long cashRequestID);
    }
}
