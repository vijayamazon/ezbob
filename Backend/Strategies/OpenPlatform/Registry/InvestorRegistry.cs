namespace Ezbob.Backend.Strategies.OpenPlatform.Registry
{
    using Ezbob.Backend.Models.Investor;
    using Ezbob.Backend.Strategies.OpenPlatform.BLL.Contracts;
    using Ezbob.Backend.Strategies.OpenPlatform.BLL.Implement;
    using Ezbob.Backend.Strategies.OpenPlatform.DAL.Contract;
    using Ezbob.Backend.Strategies.OpenPlatform.DAL.Implement;
    using StructureMap.Configuration.DSL;

    public class InvestorRegistry : Registry
    {
        public InvestorRegistry() {
            For <IProvider<IMatch<InvestorCashRequest, InvestorParameters>>>().Use(ctx => new Provider<Match<InvestorCashRequest, InvestorParameters>>(ctx));
            ForSingletonOf<IRulesEngineDAL>().Use<RulesEngineDAL>();
            ForSingletonOf<IInvestorParametersBLL>().Use<InvestorParametersBLL>();
            ForSingletonOf<IInvestorService>().Use<InvestorService>();
            ForSingletonOf<IInvestorParametersDAL>().Use<InvestorParametersDAL>();
            ForSingletonOf<IExressionBuilder>().Use<ExressionBuilder>();
            ForSingletonOf<IGenericRules>().Use<GenericRules>();
        }
    }
}
