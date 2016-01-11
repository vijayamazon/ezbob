namespace Ezbob.Backend.Strategies.OpenPlatform.BLL.Implement {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Ezbob.Backend.Models.Investor;
    using Ezbob.Backend.ModelsWithDB.OpenPlatform;
    using Ezbob.Backend.Strategies.OpenPlatform.BLL.Contracts;
    using Ezbob.Backend.Strategies.OpenPlatform.DAL.Contract;
    using Ezbob.Backend.Strategies.OpenPlatform.Models;
    using StructureMap.Attributes;

    public class InvestorParametersBLL : IInvestorParametersBLL {
        [SetterProperty]
        public IInvestorParametersDAL InvestorParametersDAL { get; set; }

        public List<int> GetInvestorsIds() {
            return InvestorParametersDAL.GetInvestorsIds();
        }

        public InvestorParameters GetInvestorParameters(int investorId, RuleType ruleType) {
            var iInvestorParameters = InvestorParametersDAL.GetInvestorParametersDB(investorId, ruleType);
            var investorParameters = new InvestorParameters();

            investorParameters.InvestorID = investorId;
            investorParameters.Balance = InvestorParametersDAL.InvestorsBalance[investorId];
            
            if (iInvestorParameters == null) {
                return investorParameters;
            }

            var firstOrDefault = iInvestorParameters.FirstOrDefault(x => x.InvestorParamsID == 1);
            if (firstOrDefault != null) {
                investorParameters.DailyAvailableAmount = (double)firstOrDefault.Value - InvestorParametersDAL.GetFundedAmountPeriod(investorId, InvesmentPeriod.Day);
            }
            var investorParams = iInvestorParameters.FirstOrDefault(x => x.InvestorParamsID == 2);
            if (investorParams != null) {
                investorParameters.WeeklyAvailableAmount = (double)investorParams.Value - InvestorParametersDAL.GetFundedAmountPeriod(investorId, InvesmentPeriod.Week);
            }
            return investorParameters;
        }

        public double GetGradeAvailableAmount(int investorId, InvestorLoanCashRequest investorLoanCashRequest, int ruleType) {

            //The amount that investor invest on grade this month.                                   
            double investorGradeInvestedAmount = InvestorParametersDAL.GetGradeMonthlyInvestedAmount(investorId, investorLoanCashRequest.Grade);

            //Sum of already funded this month + current
            var totalFunded = investorGradeInvestedAmount + investorLoanCashRequest.ManagerApprovedSum;

            //Calc max score for grade.
            var gradeMaxScore = (double)InvestorParametersDAL.GetGradeMaxScore(investorId, investorLoanCashRequest.Grade);

            //Calc total sum of positive transctions this month
            var totalMonthlyDeposits = InvestorParametersDAL.GetInvestorTotalMonthlyDeposits(investorId);

            var balanceOneMonthAgo = InvestorParametersDAL.GetInvestorBalanceMonthAgo(investorId);

            var investorMonthlyBalance = balanceOneMonthAgo + totalMonthlyDeposits;

            var monthlyFundingCapital = InvestorParametersDAL.GetInvestorMonthlyFundingCapital(investorId);

            var monthlyMax = Math.Max(investorMonthlyBalance, monthlyFundingCapital);

            return gradeMaxScore * monthlyMax - totalFunded;
        }
    }
}
