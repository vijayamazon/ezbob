namespace Ezbob.Backend.Strategies.OpenPlatform.BLL
{
    using Ezbob.Backend.Models.Investor;
    using Ezbob.Backend.ModelsWithDB.OpenPlatform;
    using Ezbob.Backend.Strategies.OpenPlatform.BLL.Contracts;
    using Ezbob.Backend.Strategies.OpenPlatform.DAL;
    using Ezbob.Backend.Strategies.OpenPlatform.DAL.Contract;
    using Ezbob.Backend.Strategies.OpenPlatform.Provider;
    using Ezbob.Backend.Strategies.OpenPlatform.Provider.Contracts;
    using Ezbob.Backend.Strategies.OpenPlatform.RulesEngine.BLL;
    using Ezbob.Backend.Strategies.OpenPlatform.RulesEngine.Contracts;
    using Ezbob.Backend.Strategies.OpenPlatform.RulesEngine.DAL;
    using EZBob.DatabaseLib.Model.Database;
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
        }
    }
}
