namespace Ezbob.Backend.Strategies.OpenPlatform.BLL
{
	using Ezbob.Backend.ModelsWithDB.Investor;
	using Ezbob.Backend.Strategies.OpenPlatform.BLL.Contracts;
	using Ezbob.Backend.Strategies.OpenPlatform.DAL;
	using Ezbob.Backend.Strategies.OpenPlatform.DAL.Contract;
	using Ezbob.Backend.Strategies.OpenPlatform.Provider;
	using Ezbob.Backend.Strategies.OpenPlatform.Provider.Contracts;
	using RulesEngine.BLL;
	using RulesEngine.Contracts;
	using RulesEngine.DAL;
	using StructureMap.Configuration.DSL;

	public class InvestorRegistry : Registry
    {
        public InvestorRegistry() {
            For<IProvider<IMatch<OfferParameters, InvestorParameters>>>().Use(ctx => new Provider<Match<OfferParameters, InvestorParameters>>(ctx));
            ForSingletonOf<IRulesEngineDAL>().Use<RulesEngineDAL>();
            ForSingletonOf<IInvestorParametersBLL>().Use<InvestorParametersBLL>();
            ForSingletonOf<IInvestorService>().Use<InvestorService>();
            ForSingletonOf<IInvestorParametersDAL>().Use<InvestorParametersDAL>();
            ForSingletonOf<IExressionBuilder>().Use<ExressionBuilder>();
        }
    }
}
