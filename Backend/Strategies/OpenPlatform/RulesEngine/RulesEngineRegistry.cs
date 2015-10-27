namespace Ezbob.Backend.Strategies.OpenPlatform.RulesEngine
{
	using Ezbob.Backend.Strategies.OpenPlatform.RulesEngine.DAL;
	using StructureMap.Configuration.DSL;

	public class RulesEngineRegistry : Registry
    {
        public RulesEngineRegistry() {
            //ForSingletonOf<IMatchInvestor>().Use<MatchInvestor<LoanParameters,InvestorParameters>>();
            ForSingletonOf<IRulesEngineDAL>().Use<RulesEngineDAL>();
        }

    }
}
