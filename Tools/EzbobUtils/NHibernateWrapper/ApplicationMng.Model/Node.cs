using NHibernateWrapper.NHibernate.Model;
using System;
using System.Text;
namespace ApplicationMng.Model
{
	public class Node : ISignable, IDeleteSupported
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
		public virtual string DisplayName
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
		public virtual SecurityApplication SecApp
		{
			get;
			set;
		}
		public virtual bool IsHardReaction
		{
			get;
			set;
		}
		public virtual bool ContainsPrint
		{
			get;
			set;
		}
		public virtual string Guid
		{
			get;
			set;
		}
		public virtual long? ExecutionDuration
		{
			get;
			set;
		}
		public virtual System.DateTime? TerminationDate
		{
			get;
			set;
		}
		public virtual User User
		{
			get;
			set;
		}
		public virtual System.DateTime? StartDate
		{
			get;
			set;
		}
		public virtual byte[] Ndx
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
		public virtual string DisplayNameWithTermDate()
		{
			return (!this.TerminationDate.HasValue) ? this.DisplayName : string.Format("{0}({1})", this.DisplayName, this.TerminationDate);
		}
		public virtual string GetSigningDocument()
		{
			return System.Text.Encoding.UTF8.GetString(this.Ndx);
		}
	}
}
