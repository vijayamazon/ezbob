namespace Ezbob.Backend.Strategies.OpenPlatform.Registry
{
    using Ezbob.Backend.Strategies.OpenPlatform.DAL.Contract;
    using Ezbob.Backend.Strategies.OpenPlatform.DAL.Implement;
    using StructureMap.Configuration.DSL;

    public class RulesEngineRegistry : Registry
    {
        public RulesEngineRegistry() {
            //ForSingletonOf<IMatchInvestor>().Use<MatchInvestor<LoanParameters,InvestorParameters>>();
            ForSingletonOf<IRulesEngineDAL>().Use<RulesEngineDAL>();
        }

    }
}
