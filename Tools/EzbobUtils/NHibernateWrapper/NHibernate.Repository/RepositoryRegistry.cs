using ApplicationMng.Repository;
using NHibernate;
using StructureMap.Configuration.DSL;
using System;
namespace NHibernateWrapper.NHibernate.Repository
{
	public class RepositoryRegistry : Registry
	{
		public RepositoryRegistry()
		{
			base.For(typeof(IRepository<>)).Use(typeof(NHibernateRepositoryBase<>));
			base.For<IApplicationRepository>().Use<ApplicationRepository>();
			base.For<IPublicNameRepository>().Use<PublicNameRepository>();
			base.For<IAppDetailRepository>().Use<AppDetailRepository>();
			base.For<IAttachDocTypeRepository>().Use<AttachDocTypeRepository>();
			base.For<IStrategyRepository>().Use<StrategyRepository>();
			base.For<IControlHistoryRepository>().Use<ControlHistoryRepository>();
			base.For<ISessionFactory>().Use(() => NHibernateManager.SessionFactory);
			base.For<IStrategyScheduleItemRepository>().Use<StrategyScheduleItemRepository>();
			base.For<IUsersRepository>().Use<UsersRepository>();
			base.For<IRolesRepository>().Use<RolesRepository>();
			base.For<IAppAttachmentsRepository>().Use<AppAttachmentsRepository>();
			base.For<ISecurityApplicationsRepository>().Use<SecurityApplicationsRepository>();
			base.For<IMenuRepository>().Use<MenuRepository>();
			base.For<IAppStatusRepository>().Use<AppStatusRepository>();
			base.For<IBusinessEntityRepository>().Use<BusinessEntityRepository>();
			base.For<ApplicationSuspendedRepository>().Use<ApplicationSuspendedRepository>();
			base.For<ISecurityQuestionRepository>().Use<SecurityQuestionRepository>();
		}
	}
}
