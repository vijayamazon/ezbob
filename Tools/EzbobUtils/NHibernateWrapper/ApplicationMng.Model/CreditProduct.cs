using Iesi.Collections.Generic;
using System;
namespace ApplicationMng.Model
{
	public class CreditProduct
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
		public virtual string Description
		{
			get;
			set;
		}
		public virtual System.DateTime CreationDate
		{
			get;
			set;
		}
		public virtual User User
		{
			get;
			set;
		}
		public virtual int? IsDeleted
		{
			get;
			set;
		}
		public virtual ISet<CreditProductParam> Params
		{
			get;
			set;
		}
		public CreditProduct()
		{
			this.Params = new HashedSet<CreditProductParam>();
		}
	}
}
