namespace Ezbob.Backend.Strategies.OpenPlatform.BLL.Contracts
{
    using Ezbob.Backend.ModelsWithDB.Investor;

    public interface IInvestorCashRequestBLL {
        InvestorLoanCashRequest GetInvestorLoanCashRequest(long cashRequestID);
    }
}


