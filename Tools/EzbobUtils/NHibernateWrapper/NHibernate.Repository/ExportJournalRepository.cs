using ApplicationMng.Repository;
using NHibernate;
using NHibernateWrapper.NHibernate.Model;
using System;
namespace NHibernateWrapper.NHibernate.Repository
{
	public class ExportJournalRepository : NHibernateRepositoryBase<ExportJournal>
	{
		public ExportJournalRepository(ISession session) : base(session)
		{
		}
	}
}
