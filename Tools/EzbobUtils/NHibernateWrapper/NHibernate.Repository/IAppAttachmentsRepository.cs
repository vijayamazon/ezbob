using ApplicationMng.Model;
using ApplicationMng.Repository;
using System;
namespace NHibernateWrapper.NHibernate.Repository
{
	public interface IAppAttachmentsRepository : IRepository<AppAttachment>, System.IDisposable
	{
		AppAttachment GetByDetailId(long detailId);
		AppAttachment GetByDetailId(long detailId, System.Func<AppAttachment> defaultFunc);
	}
}
