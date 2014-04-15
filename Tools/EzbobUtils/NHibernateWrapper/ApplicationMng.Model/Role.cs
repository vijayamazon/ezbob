using Iesi.Collections.Generic;
using NHibernateWrapper.NHibernate.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
namespace ApplicationMng.Model
{
	[System.Diagnostics.DebuggerDisplay("Id = {_id}, Name = {_name}, Description = {_description}")]
	public class Role
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
		public virtual Iesi.Collections.Generic.ISet<User> Users
		{
			get;
			set;
		}
		public virtual Iesi.Collections.Generic.ISet<SecurityApplication> Applications
		{
			get;
			set;
		}
		public virtual Iesi.Collections.Generic.ISet<Permission> Permissions
		{
			get;
			set;
		}
		public Role()
		{
			this.Users = new HashedSet<User>();
			this.Applications = new HashedSet<SecurityApplication>();
			this.Permissions = new HashedSet<Permission>();
		}
		public virtual void AddUsers(System.Collections.Generic.List<User> users)
		{
			foreach (User current in users)
			{
				current.Roles.Add(this);
				this.Users.Add(current);
			}
		}
		public virtual void ClearUsers()
		{
			foreach (User current in this.Users)
			{
				current.Roles.Remove(this);
			}
			this.Users.Clear();
		}
	}
}
