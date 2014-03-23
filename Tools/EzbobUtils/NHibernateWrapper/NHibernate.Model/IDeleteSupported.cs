using System;
namespace NHibernateWrapper.NHibernate.Model
{
	public interface IDeleteSupported
	{
		int? IsDeleted
		{
			get;
		}
		System.DateTime? TerminationDate
		{
			get;
		}
		string DisplayNameWithTermDate();
	}
}
