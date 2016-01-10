namespace Ezbob.Backend.Strategies.OpenPlatform.DAL.Implement
{
    using System;
    using Ezbob.Backend.Models.Investor;
    using Ezbob.Backend.ModelsWithDB.OpenPlatform;
    using Ezbob.Backend.Strategies.OpenPlatform.DAL.Contract;
    using Ezbob.Database;

    public class InvestorCashRequestDAL : IInvestorCashRequestDAL
    {
        public InvestorLoanCashRequest GetInvestorLoanCashRequest(long cashRequestID) {
            
            var crData = Library.Instance.DB.ExecuteScalar<Tuple<int,double>>(
                string.Format("select ipt.FundingTypeID, cr.ManagerApprovedSum from CashRequests cr  " +
                              "join I_ProductSubType ipt on cr.ProductSubTypeID = ipt.ProductTypeID " +
                              "where cr.Id = {0}", cashRequestID), CommandSpecies.Text);

            var grade = Library.Instance.DB.ExecuteScalar<int>(
                string.Format("select top 1 lgrsp.GradeID from CashRequests cr " +
                              "join customer c on cr.Id = c.Id  " +
                              "join MP_ServiceLog mpsl on mpsl.CompanyID = c.CompanyId and mpsl.CustomerId = c.Id and mpsl.ServiceType = 'logicalglue'  " +
                              "join LogicalGlueRequests lgr on IsTryOut = 0  " +
                              "join LogicalGlueResponses lgrsp on lgr.ServiceLogID = lgrsp.ServiceLogID " +
                               "where cr.Id = {0} order by mpsl.InsertDate desc", cashRequestID), CommandSpecies.Text);



            var investorPrecentage = GetPrecentage((I_FundingTypeEnum)crData.Item1);
            if (investorPrecentage < 0)
                 throw new Exception("Couldn't calc investor precentage");


            return  new InvestorLoanCashRequest() {
                CashRequestID = cashRequestID,
                ManagerApprovedSum = crData.Item1 * investorPrecentage,
                Grade = (Grade)grade
            };
        }

        private float GetPrecentage(I_FundingTypeEnum fundingType) {
            switch (fundingType) {
                case I_FundingTypeEnum.FullInvestment:
                    return 1;
                case I_FundingTypeEnum.CoInvestment:
                    throw new Exception("Unsupported CoInvestment loans at this time");
                case I_FundingTypeEnum.PooledInvestment:
                    throw new Exception("Unsupported PooledInvestment loans at this time");
            }
            return -1;
        }
    }
}
