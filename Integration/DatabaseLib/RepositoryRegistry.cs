namespace EZBob.DatabaseLib
{
	using Model.Database.UserManagement;
	using Model.Loans;
	using NHibernate;
	using NHibernateWrapper.NHibernate;
	using ApplicationMng.Repository;
	using Model;
	using Model.Database.Repository;
	using StructureMap.Configuration.DSL;

	public class RepositoryRegistry : Registry
    {
        public RepositoryRegistry()
		{
			For(typeof(IRepository<>)).Use(typeof(NHibernateRepositoryBase<>));
			For<ISessionFactory>().Use(() => NHibernateManager.SessionFactory);
			For<IUsersRepository>().Use<UsersRepository>();
			For<IRolesRepository>().Use<RolesRepository>();
			For<ISecurityQuestionRepository>().Use<SecurityQuestionRepository>();

            For<ICustomerAddressRepository>().Use<CustomerAddressRepository>();
            For<IConfigurationVariablesRepository>().Use<ConfigurationVariablesRepository>();
			For<ILoanAgreementRepository>().Use<LoanAgreementRepository>();
        }
    }
}