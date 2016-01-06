namespace Ezbob.Backend.Strategies.OpenPlatform.BLL.Implement
{
    using System.Collections.Generic;
    using Ezbob.Backend.Models.Investor;
    using Ezbob.Backend.ModelsWithDB.OpenPlatform;
    using Ezbob.Backend.Strategies.OpenPlatform.BLL.Contracts;
    using Ezbob.Backend.Strategies.OpenPlatform.DAL.Contract;
    using StructureMap.Attributes;

    public class InvestorParametersBLL : IInvestorParametersBLL
    {
        [SetterProperty]
        public IInvestorParametersDAL InvestorParametersDAL { get; set; }

        public  List<InvestorParameters> GetInvestorParametersList() {
            return InvestorParametersDAL.GetInvestorParametersList();
        }

        public Dictionary<Grade, double> GetInvestorBudgetSokets(int InvestorId) {
            Dictionary<Grade, double> dict = new Dictionary<Grade, double>();
            dict.Add(Grade.A, 100);
            dict.Add(Grade.B, 100);
            dict.Add(Grade.C, 100);
            dict.Add(Grade.D, 100);
            dict.Add(Grade.E, 100);
            dict.Add(Grade.F, 100);
            return dict;
        }
    }
}
