using ApplicationMng.Model;
using Iesi.Collections.Generic;
using System;
namespace NHibernateWrapper.NHibernate.Model
{
	public class Permission
	{
		private ISet<Role> _roles = new HashedSet<Role>();
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
		public virtual ISet<Role> Roles
		{
			get
			{
				return this._roles;
			}
			set
			{
				this._roles = value;
			}
		}
	}
}
