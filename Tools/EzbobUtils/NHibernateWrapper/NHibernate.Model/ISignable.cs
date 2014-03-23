using System;
namespace NHibernateWrapper.NHibernate.Model
{
	public interface ISignable
	{
		string SignedDocument
		{
			get;
			set;
		}
		string SignedDocumentDelete
		{
			get;
			set;
		}
	}
}
