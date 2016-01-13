namespace Ezbob.Backend.Strategies.OpenPlatform.DAL.Implement {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Ezbob.Backend.ModelsWithDB.OpenPlatform;
    using Ezbob.Backend.ModelsWithDB.Wrappers;
    using Ezbob.Backend.Strategies.OpenPlatform.DAL.Contract;
    using Ezbob.Database;

    public class InvestorParametersDAL : IInvestorParametersDAL {

        private Dictionary<int, decimal> investorsBalance;
        private Dictionary<int, I_Parameter> investorsParameters;
        public Dictionary<int, decimal> InvestorsBalance {
            get {
                return this.investorsBalance ?? (this.investorsBalance = Library.Instance.DB.Fill<I_InvestorBalance>("I_GetInvestorsBalance", CommandSpecies.StoredProcedure).ToDictionary(x => x.InvestorID, x => x.Balance));
            }
        }
        public Dictionary<int, I_Parameter> InvestorsParameters {
            get {
                return this.investorsParameters ?? (this.investorsParameters = Library.Instance.DB.Fill<I_Parameter>("I_GetInvestorParameters", CommandSpecies.StoredProcedure).ToDictionary(x => x.ParameterID, x => x));
            }
        }

        public List<int> GetInvestorsIds() {
            var temp = Library.Instance.DB.Fill<IntWrapper>("I_GetInvestorsIds", CommandSpecies.StoredProcedure);
                return temp.Select(x=> x.Value).ToList();
        }

        public List<I_InvestorParams> GetInvestorParametersDB(int investorId, RuleType ruleType) {
            var queryParameters = ruleType == RuleType.System ? new[] { new QueryParameter("TypeID", (int)ruleType) } : new[] { new QueryParameter("TypeID", (int)ruleType), new QueryParameter("InvestorId", investorId) };
            return Library.Instance.DB.Fill<I_InvestorParams>("I_GetInvestorParametersDB", CommandSpecies.StoredProcedure, queryParameters);
        }

        public int GetInvestorWithLatestLoanDate(List<int> investorsList) {
            return Library.Instance.DB.ExecuteScalar<int>("I_GetInvestorWithLatestLoanDate", CommandSpecies.StoredProcedure,  Library.Instance.DB.CreateTableParameter("InvestorIDs", (IEnumerable<int>)investorsList));
        }

        public decimal GetGradeMonthlyInvestedAmount(int investorId, Grade grade) {
            var myDate = DateTime.Now;
            var firstOfMonth = new DateTime(myDate.Year, myDate.Month, 1);
            var result = Library.Instance.DB.ExecuteScalar<decimal>("I_GetGradeMonthlyInvestedAmount", CommandSpecies.StoredProcedure, new QueryParameter("InvestorID", investorId), new QueryParameter("GradeID", (int)grade), new QueryParameter("FirstOfMonth", TimeZoneInfo.ConvertTimeToUtc(firstOfMonth)));
            return (decimal)result;
        }

        public decimal GetGradeMaxScore(int investorId, int grade, int ruleType) {
            var queryParameters = ruleType == 1 ? new QueryParameter[0] : new[] {new QueryParameter("InvestorId", investorId) };
            var index = Library.Instance.DB.FillFirst<I_Index>("I_GetGradeMaxScore", CommandSpecies.StoredProcedure, queryParameters);
            switch (grade) {
                case (int)Grade.A: return index.GradeAMaxScore;
				case (int)Grade.B: return index.GradeBMaxScore;
				case (int)Grade.C: return index.GradeCMaxScore;
				case (int)Grade.D: return index.GradeDMaxScore;
				case (int)Grade.E: return index.GradeEMaxScore;
				case (int)Grade.F: return index.GradeFMaxScore;
            }
            return 0;
        }

        public decimal GetInvestorTotalMonthlyDeposits(int investorId) {
            var myDate = DateTime.Now;
            var firstOfMonth = new DateTime(myDate.Year, myDate.Month, 1);
            return Library.Instance.DB.ExecuteScalar<decimal>("I_GetInvestorTotalMonthlyDeposits", CommandSpecies.StoredProcedure, new QueryParameter("InvestorID", investorId), new QueryParameter("FirstOfMonth", firstOfMonth));
        }

        public decimal GetInvestorMonthlyFundingCapital(int investorId) {
            return Library.Instance.DB.ExecuteScalar<decimal>("I_GetInvestorMonthlyFundingCapital", CommandSpecies.StoredProcedure, new QueryParameter("InvestorID", investorId));
        }

        public decimal GetFundedAmountPeriod(int investorId, InvesmentPeriod invesmentPeriod) {
            DateTime periodAgo = new DateTime();
            switch (invesmentPeriod) {
                case InvesmentPeriod.Day:
                    periodAgo = DateTime.Today.AddDays(-1);
                    break;
                case InvesmentPeriod.Week:
                    periodAgo = DateTime.Today.AddDays(-7);
                    break;
                case InvesmentPeriod.Month:
                    periodAgo = DateTime.Today.AddDays(-30);
                    break;
            }

            var fundedAmount = Library.Instance.DB.ExecuteScalar<decimal>("I_GetFundedAmountPeriod", CommandSpecies.StoredProcedure, new QueryParameter("InvestorID", investorId), new QueryParameter("PeriodAgo", TimeZoneInfo.ConvertTimeToUtc(periodAgo)));
            return fundedAmount;
        }

    }
}
