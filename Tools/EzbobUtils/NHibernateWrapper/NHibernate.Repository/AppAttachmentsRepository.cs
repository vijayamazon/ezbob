using ApplicationMng.Model;
using ApplicationMng.Repository;
using NHibernate;
using System;
using System.Linq;
namespace NHibernateWrapper.NHibernate.Repository
{
	public class AppAttachmentsRepository : NHibernateRepositoryBase<AppAttachment>, IAppAttachmentsRepository, IRepository<AppAttachment>, System.IDisposable
	{
		public AppAttachmentsRepository(ISession session) : base(session)
		{
		}
		public AppAttachment GetByDetailId(long detailId)
		{
			return this.GetAll().First((AppAttachment a) => a.DetailId == detailId);
		}
		public AppAttachment GetByDetailId(long detailId, System.Func<AppAttachment> defaultFunc)
		{
			return this.GetAll().FirstOrDefault((AppAttachment a) => a.DetailId == detailId) ?? defaultFunc();
		}
	}
}
