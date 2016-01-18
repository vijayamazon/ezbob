namespace Ezbob.Backend.Strategies.OpenPlatform.Registry
{
	using Ezbob.Backend.Models.Investor;
	using Ezbob.Backend.Strategies.OpenPlatform.BLL.Contracts;
    using Ezbob.Backend.Strategies.OpenPlatform.BLL.Implement;
    using Ezbob.Backend.Strategies.OpenPlatform.DAL.Contract;
    using Ezbob.Backend.Strategies.OpenPlatform.DAL.Implement;
    using Ezbob.Backend.Strategies.OpenPlatform.Facade.Contracts;
    using Ezbob.Backend.Strategies.OpenPlatform.Facade.Implement;
    using Ezbob.Backend.Strategies.OpenPlatform.Models;
    using StructureMap.Configuration.DSL;

    public class InvestorRegistry : Registry
    {
        public InvestorRegistry() {
            For <IProvider<IMatchBLL<InvestorLoanCashRequest, InvestorParameters>>>().Use(ctx => new Provider<MatchBLL<InvestorLoanCashRequest, InvestorParameters>>(ctx));
            ForSingletonOf<IInvestorService>().Use<InvestorService>();
            
            ForSingletonOf<IInvestorParametersBLL>().Use<InvestorParametersBLL>();
            ForSingletonOf<IInvestorCashRequestBLL>().Use<InvestorCashRequestBLL>();
            ForSingletonOf<IExressionBuilderBLL>().Use<ExressionBuilderBLL>();
            ForSingletonOf<IGenericRulesBLL>().Use<GenericRulesBLL>();

            ForSingletonOf<IRulesEngineDAL>().Use<RulesEngineDAL>();
            ForSingletonOf<IInvestorParametersDAL>().Use<InvestorParametersDAL>();
            ForSingletonOf<IInvestorCashRequestDAL>().Use<InvestorCashRequestDAL>();
            ForSingletonOf<IRulesEngineDAL>().Use<RulesEngineDAL>();
        }
    }
}
