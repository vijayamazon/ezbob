using Iesi.Collections.Generic;
using NHibernateWrapper.NHibernate;
using System;
using System.Collections.Generic;
namespace ApplicationMng.Model
{
	public class SecurityApplication
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
		public virtual Iesi.Collections.Generic.ISet<Role> Roles
		{
			get;
			set;
		}
		public virtual string Description
		{
			get;
			set;
		}
		public SecurityApplication()
		{
			this.Roles = new HashedSet<Role>();
		}
	}
}
