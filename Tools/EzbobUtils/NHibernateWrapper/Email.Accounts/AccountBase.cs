using ApplicationMng.Model;
using Iesi.Collections.Generic;
using NHibernateWrapper.NHibernate.Model;
using System;
namespace NHibernateWrapper.Email.Accounts
{
	public abstract class AccountBase : ISignable, IDeleteSupported
	{
		public virtual int Id
		{
			get;
			set;
		}
		public virtual int? IsDeleted
		{
			get;
			set;
		}
		public virtual System.DateTime? TerminationDate
		{
			get;
			set;
		}
		public virtual string Name
		{
			get;
			set;
		}
		public virtual string Description
		{
			get;
			set;
		}
		public virtual string EmailFrom
		{
			get;
			set;
		}
		public abstract string TypeName
		{
			get;
		}
		public virtual ISet<Strategy> Strategies
		{
			get;
			set;
		}
		public virtual System.DateTime? StartDate
		{
			get;
			set;
		}
		public virtual User User
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
		protected AccountBase()
		{
			this.Strategies = new HashedSet<Strategy>();
		}
		public virtual string DisplayNameWithTermDate()
		{
			return (!this.TerminationDate.HasValue) ? this.Name : string.Format("{0}( {1} )", this.Name, this.TerminationDate.Value);
		}
	}
}
