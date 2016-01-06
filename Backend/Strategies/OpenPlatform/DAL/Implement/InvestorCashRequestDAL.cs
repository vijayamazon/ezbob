namespace Ezbob.Backend.Strategies.OpenPlatform.DAL.Implement
{
    using Ezbob.Backend.Models.Investor;
    using Ezbob.Backend.ModelsWithDB.OpenPlatform;
    using Ezbob.Backend.Strategies.OpenPlatform.DAL.Contract;
    using Ezbob.Utils.Html.Tags;

    public class InvestorCashRequestDAL : IInvestorCashRequestDAL
    {
        public InvestorLoanCashRequest GetInvestorLoanCashRequest(long cashRequestID) {

            return  new InvestorLoanCashRequest() {
                CashRequestID = 1,
                ManagerApprovedSum = 500,
                Grade = Grade.A
            };
        }
    }
}
