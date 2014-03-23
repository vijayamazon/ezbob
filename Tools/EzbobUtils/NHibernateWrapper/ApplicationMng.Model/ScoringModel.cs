using NHibernateWrapper.NHibernate.Model;
using System;
namespace ApplicationMng.Model
{
	public class ScoringModel : ISignable, IDeleteSupported
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
		public virtual string Guid
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
		public virtual string ModelTypeName
		{
			get;
			set;
		}
		public virtual System.DateTime? CreationDate
		{
			get;
			set;
		}
		public virtual double CutOffPoint
		{
			get;
			set;
		}
		public virtual System.DateTime? TerminationDate
		{
			get;
			set;
		}
		public virtual User CreateUser
		{
			get;
			set;
		}
		public virtual bool AllowWeightsEdit
		{
			get;
			set;
		}
		public virtual bool AllowSaveResults
		{
			get;
			set;
		}
		public virtual string PmmlFile
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
