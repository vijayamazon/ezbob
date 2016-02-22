namespace Ezbob.Backend.Strategies.OpenPlatform.DAL.Implement {
	using System;
	using Ezbob.Backend.Models.Investor;
	using Ezbob.Backend.ModelsWithDB.OpenPlatform;
	using Ezbob.Backend.Strategies.LogicalGlue;
	using Ezbob.Backend.Strategies.OpenPlatform.DAL.Contract;
	using Ezbob.Database;

	public class InvestorCashRequestDAL : IInvestorCashRequestDAL {
		public InvestorLoanCashRequest GetInvestorLoanCashRequest(long cashRequestID) {

			var cashRequestData = Library.Instance.DB.FillFirst<I_CashRequestData>("I_GetInvestorLoanCashRequest", CommandSpecies.StoredProcedure, new QueryParameter("CashRequestsId", cashRequestID));

			if (cashRequestData == null) {
				throw new Exception(string.Format("Couldn't resolve data for cash request ID: {0}", cashRequestID));
			}

			GetLatestKnownInference strategy = new GetLatestKnownInference(cashRequestData.CustomerID, DateTime.Now, false);
			strategy.Execute();

			var grade = 0;
			if (strategy.Inference!=null && strategy.Inference.Bucket.HasValue) {
				grade = (int)strategy.Inference.Bucket;
			}

			var investorPrecentage = GetPrecentage((I_FundingTypeEnum)cashRequestData.FundingTypeID);
			if (investorPrecentage < 0)
				throw new Exception("Couldn't calc investor precentage");


			return new InvestorLoanCashRequest() {
				CashRequestID = cashRequestID,
				ManagerApprovedSum = cashRequestData.ManagerApprovedSum * (decimal)investorPrecentage,
				GradeID = grade,
				FundingType = investorPrecentage
			};

		}

		private decimal GetPrecentage(I_FundingTypeEnum fundingType) {
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
