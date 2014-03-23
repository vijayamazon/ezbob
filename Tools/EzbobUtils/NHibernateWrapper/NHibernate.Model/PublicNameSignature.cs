using ApplicationMng.Model;
using System;
namespace NHibernateWrapper.NHibernate.Model
{
	public class PublicNameSignature : ISignable
	{
		public virtual int Id
		{
			get;
			set;
		}
		public virtual string SignedDocument
		{
			get;
			set;
		}
		public virtual string SignedDocumentDelete
		{
			get;
			set;
		}
		public virtual string Data
		{
			get;
			set;
		}
		public virtual string AllData
		{
			get;
			set;
		}
		public virtual PublicName PublicName
		{
			get;
			set;
		}
		public virtual System.DateTime? ChangeDate
		{
			get;
			set;
		}
		public virtual User User
		{
			get;
			set;
		}
		public virtual string Action
		{
			get;
			set;
		}
	}
}
