using ApplicationMng.Model;
using System;
namespace NHibernateWrapper.NHibernate.Model
{
	public class CreditProductSignature : ISignable
	{
		public virtual int Id
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
		public virtual string Data
		{
			get;
			set;
		}
		public virtual CreditProduct CreditProduct
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
			get
			{
				return (this.CreditProduct.IsDeleted.HasValue && this.CreditProduct.IsDeleted > 0) ? this.SignedDocument : null;
			}
			set
			{
			}
		}
	}
}
