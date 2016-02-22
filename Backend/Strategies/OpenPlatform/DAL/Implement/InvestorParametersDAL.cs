namespace Ezbob.Backend.Strategies.OpenPlatform.DAL.Implement {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Ezbob.Backend.ModelsWithDB.OpenPlatform;
    using Ezbob.Backend.ModelsWithDB.Wrappers;
    using Ezbob.Backend.Strategies.OpenPlatform.DAL.Contract;
    using Ezbob.Database;
    using log4net;

	public class InvestorParametersDAL : IInvestorParametersDAL {

        private Dictionary<int, decimal> investorsBalance;
        private Dictionary<int, I_Parameter> investorsParameters;
        public Dictionary<int, decimal> InvestorsBalance {
            get {
	            try {
		            return this.investorsBalance ?? (this.investorsBalance = Library.Instance.DB.Fill<I_InvestorBalance>("I_GetInvestorsBalance",
			            CommandSpecies.StoredProcedure)
			            .ToDictionary(x => x.InvestorID, x => x.Balance));
				} catch (Exception ex) {
					Log.ErrorFormat("Failed to retrieve I_GetInvestorsBalance \n{0} ", ex);
					return new Dictionary<int, decimal>();
				}
            }
        }
        public Dictionary<int, I_Parameter> InvestorsParameters {
            get {
	            try {
		            return this.investorsParameters ?? (this.investorsParameters = Library.Instance.DB.Fill<I_Parameter>("I_GetInvestorParameters",
			            CommandSpecies.StoredProcedure)
			            .ToDictionary(x => x.ParameterID, x => x));
				} catch (Exception ex) {
					Log.ErrorFormat("Failed to retrieve I_GetInvestorParameters \n{0} ", ex);
					return new Dictionary<int, I_Parameter>();
				}
            }
        }

        public List<int> GetInvestorsIds() {
            var temp = Library.Instance.DB.Fill<IntWrapper>("I_GetInvestorsIds", CommandSpecies.StoredProcedure);
                return temp.Select(x=> x.Value).ToList();
        }

        public List<I_InvestorParams> GetInvestorParametersDB(int investorId, RuleType ruleType) {
            var queryParameters = ruleType == RuleType.System ? new[] { new QueryParameter("TypeID", (int)ruleType) } : new[] { 
				new QueryParameter("TypeID", (int)ruleType), new QueryParameter("InvestorId", investorId)
			};

            return Library.Instance.DB.Fill<I_InvestorParams>("I_GetInvestorParametersDB", 
				CommandSpecies.StoredProcedure, 
				queryParameters);
        }

        public int GetInvestorWithLatestLoanDate(List<int> investorsList) {
            return Library.Instance.DB.ExecuteScalar<int>("I_GetInvestorWithLatestLoanDate", 
				CommandSpecies.StoredProcedure,  
				Library.Instance.DB.CreateTableParameter("InvestorIDs", (IEnumerable<int>)investorsList));
        }

        public decimal GetGradeMonthlyInvestedAmount(int investorId, Grade grade) {
            var myDate = DateTime.Now;
	        int? gradeID = grade == null ? (int?)grade : (int)grade;
			var firstOfMonth = new DateTime(myDate.Year, myDate.Month, 1);
            var result = Library.Instance.DB.ExecuteScalar<decimal>("I_GetGradeMonthlyInvestedAmount", 
				CommandSpecies.StoredProcedure, 
				new QueryParameter("InvestorID", investorId), 
				new QueryParameter("GradeID", gradeID), 
				new QueryParameter("FirstOfMonth", 
					TimeZoneInfo.ConvertTimeToUtc(firstOfMonth)));
            return (decimal)result;
        }

        public decimal GetGradePercent(int investorId, int grade, int ruleType) {
            var queryParameters = ruleType == 1 ? new QueryParameter[0] : new[] {new QueryParameter("InvestorId", investorId) };
            var index = Library.Instance.DB.FillFirst<I_Index>("I_IndexLoad", CommandSpecies.StoredProcedure, queryParameters);
            switch (grade) {
                case (int)Grade.A: return index.GradeAPercent;
				case (int)Grade.B: return index.GradeBPercent;
				case (int)Grade.C: return index.GradeCPercent;
				case (int)Grade.D: return index.GradeDPercent;
				case (int)Grade.E: return index.GradeEPercent;
				case (int)Grade.F: return index.GradeFPercent;
            }
            return 0;
        }

        public decimal GetInvestorTotalMonthlyDeposits(int investorId) {
            var myDate = DateTime.Now;
            var firstOfMonth = new DateTime(myDate.Year, myDate.Month, 1);
            return Library.Instance.DB.ExecuteScalar<decimal>("I_GetInvestorTotalMonthlyDeposits", 
				CommandSpecies.StoredProcedure, 
				new QueryParameter("InvestorID", investorId),
				new QueryParameter("FirstOfMonth", firstOfMonth));
        }

        public decimal GetInvestorMonthlyFundingCapital(int investorId) {
	        try {
		        return Library.Instance.DB.ExecuteScalar<decimal>("I_GetInvestorMonthlyFundingCapital",
			        CommandSpecies.StoredProcedure,
			        new QueryParameter("InvestorID", investorId));
			} catch (Exception ex) {
				Log.WarnFormat("Failed to retrieve GetInvestorMonthlyFundingCapital for investor {0} \n {1}", investorId, ex);
				return 0;
			}
        }

        public decimal GetFundedAmountPeriod(int investorId, InvesmentPeriod invesmentPeriod) {
	        DateTime now = DateTime.UtcNow;
            DateTime periodAgo = new DateTime();
            switch (invesmentPeriod) {
                case InvesmentPeriod.Day:
                    periodAgo = now.AddDays(-1);
                    break;
                case InvesmentPeriod.Week:
					periodAgo = now.AddDays(-7);
                    break;
                case InvesmentPeriod.Month:
					periodAgo = now.AddMonths(-1);
                    break;
            }

            var fundedAmount = Library.Instance.DB.ExecuteScalar<decimal>("I_GetFundedAmountPeriod", 
				CommandSpecies.StoredProcedure, 
				new QueryParameter("InvestorID", investorId), 
				new QueryParameter("PeriodAgo",periodAgo)
			);
            return fundedAmount;
        }

		protected static ILog Log = LogManager.GetLogger(typeof(InvestorParametersDAL));
	}
}
