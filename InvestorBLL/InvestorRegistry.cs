namespace InvestorBLL
{
    using Ezbob.Backend.ModelsWithDB.Investor;
    using EzBobCommon;
    using EzBobCommon.Contracts;
    using InvestorBLL.Contracts;
    using InvestorDAL;
    using InvestorDAL.Contract;
    using RulesEngine.BLL;
    using RulesEngine.Contracts;
    using RulesEngine.DAL;
    using StructureMap.Configuration.DSL;

    public class InvestorRegistry : Registry
    {
        public InvestorRegistry() {
            For<IProvider<IMatch<LoanParameters, InvestorParameters>>>().Use(ctx => new Provider<Match<LoanParameters, InvestorParameters>>(ctx));
            ForSingletonOf<IRulesEngineDAL>().Use<RulesEngineDAL>();
            ForSingletonOf<IInvestorParametersBLL>().Use<InvestorParametersBLL>();
            ForSingletonOf<IInvestorService>().Use<InvestorService>();
            ForSingletonOf<IInvestorParametersDAL>().Use<InvestorParametersDAL>();
            ForSingletonOf<IExressionBuilder>().Use<ExressionBuilder>();
        }
    }
}
