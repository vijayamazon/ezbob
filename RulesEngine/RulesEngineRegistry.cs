namespace RulesEngine
{
    using Ezbob.Backend.ModelsWithDB.Investor;
    using RulesEngine.DAL;
    using StructureMap.Configuration.DSL;

    public class RulesEngineRegistry : Registry
    {
        public RulesEngineRegistry() {
            ForSingletonOf<IMatchInvestor>().Use<MatchInvestor<LoanParameters,InvestorParameters>>();
            ForSingletonOf<IRulesEngineDAL>().Use<RulesEngineDAL>();
        }

    }
}
