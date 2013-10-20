using ApplicationMng.Repository;
using EZBob.DatabaseLib.Model;
using EZBob.DatabaseLib.Model.Database.Repository;
using EZBob.DatabaseLib.PayPal;
using StructureMap.Configuration.DSL;

namespace EZBob.DatabaseLib
{
	using Model.Loans;

	public class RepositoryRegistry : Registry
    {
        public RepositoryRegistry()
        {
            For<ICustomerAddressRepository>().Use<CustomerAddressRepository>();
            For<IConfigurationVariablesRepository>().Use<ConfigurationVariablesRepository>();
            For<IMailTemplateRelationRepository>().Use<MailTemplateRelationRepository>();
            For<IMandrillTemplateRepository>().Use<MandrillTemplateRepository>();
			For<ILoanAgreementRepository>().Use<LoanAgreementRepository>();
        }
    }
}