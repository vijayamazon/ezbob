using Iesi.Collections.Generic;
using NHibernateWrapper.Email.Accounts;
using NHibernateWrapper.NHibernate.Model;
using Scorto.SystemCalendar.Model;
using System;
using System.Collections.Generic;
namespace ApplicationMng.Model
{
	using Scorto.SystemCalendar.Model;

	public class Strategy : ISignable, IDeleteSupported
	{
		public virtual int Id
		{
			get;
			set;
		}
		public virtual string Name
		{
			get;
			set;
		}
		public virtual string Type
		{
			get;
			set;
		}
		public virtual string Description
		{
			get;
			set;
		}
		public virtual int? IsDeleted
		{
			get;
			set;
		}
		public virtual string DisplayName
		{
			get;
			set;
		}
		public virtual bool State
		{
			get;
			set;
		}
		public virtual int CurrentVersionId
		{
			get;
			set;
		}
		public virtual Iesi.Collections.Generic.ISet<AccountBase> Accounts
		{
			get;
			set;
		}
		public virtual User User
		{
			get;
			set;
		}
		public virtual System.DateTime? TerminationDate
		{
			get;
			set;
		}
		public virtual System.DateTime? CreationDate
		{
			get;
			set;
		}
		public virtual Iesi.Collections.Generic.ISet<ExportTemplate> ExportTemplates
		{
			get;
			set;
		}
		public virtual Iesi.Collections.Generic.ISet<Node> Nodes
		{
			get;
			set;
		}
		public virtual Iesi.Collections.Generic.ISet<PublicNameStrategy> PublicNameStrategies
		{
			get;
			set;
		}
		public virtual Iesi.Collections.Generic.ISet<CreditProduct> Products
		{
			get;
			set;
		}
		public virtual System.Collections.Generic.IList<Calendar> Calendars
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
		public virtual string Xml
		{
			get;
			set;
		}
		public virtual byte[] InDbFormat
		{
			get;
			set;
		}
		public Strategy()
		{
			this.Accounts = new HashedSet<AccountBase>();
			this.ExportTemplates = new HashedSet<ExportTemplate>();
			this.Nodes = new HashedSet<Node>();
			this.PublicNameStrategies = new HashedSet<PublicNameStrategy>();
			this.Calendars = new System.Collections.Generic.List<Calendar>();
		}
		public virtual string GetSigningDocument()
		{
			return this.Xml;
		}
		public virtual string DisplayNameWithTermDate()
		{
			return (!this.TerminationDate.HasValue) ? this.DisplayName : string.Concat(new object[]
			{
				this.DisplayName,
				"( ",
				this.TerminationDate.Value,
				" )"
			});
		}
	}
}
