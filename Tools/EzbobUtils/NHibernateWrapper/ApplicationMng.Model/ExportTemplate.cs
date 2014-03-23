using Iesi.Collections.Generic;
using NHibernateWrapper.NHibernate.Model;
using System;
namespace ApplicationMng.Model
{
	public class ExportTemplate : ISignable, IDeleteSupported
	{
		private ISet<Strategy> _strategies = new HashedSet<Strategy>();
		public virtual int Id
		{
			get;
			set;
		}
		public virtual string FileName
		{
			get;
			set;
		}
		public virtual string Description
		{
			get;
			set;
		}
		public virtual string VariablesXml
		{
			get;
			set;
		}
		public virtual System.DateTime? UploadDate
		{
			get;
			set;
		}
		public virtual int? IsDeleted
		{
			get;
			set;
		}
		public virtual byte[] BinaryBody
		{
			get;
			set;
		}
		public virtual int ExceptionType
		{
			get;
			set;
		}
		public virtual User Creator
		{
			get;
			set;
		}
		public virtual User Deleter
		{
			get;
			set;
		}
		public virtual string DisplayName
		{
			get;
			set;
		}
		public virtual System.DateTime? TerminationDate
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
		public virtual ISet<Strategy> Strategies
		{
			get
			{
				return this._strategies;
			}
			set
			{
				this._strategies = value;
			}
		}
		public virtual string DisplayNameWithTermDate()
		{
			return (!this.TerminationDate.HasValue) ? this.DisplayName : string.Format("{0}({1})", this.DisplayName, this.TerminationDate);
		}
	}
}
