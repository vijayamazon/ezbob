using NHibernateWrapper.NHibernate.Model;
using System;
using System.Diagnostics;
namespace ApplicationMng.Model
{
	[System.Diagnostics.DebuggerDisplay("Id = {_id}, Name = {_name}, Description = {_description}")]
	public class DataSource : ISignable, IDeleteSupported
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
		public virtual string Type
		{
			get;
			set;
		}
		public virtual int? IsDeleted
		{
			get;
			set;
		}
		public virtual System.DateTime? CreationDate
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
		public virtual string Document
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
			return (!this.TerminationDate.HasValue) ? this.DisplayName : string.Format("{0}( {1} )", this.DisplayName, this.TerminationDate.Value);
		}
	}
}
