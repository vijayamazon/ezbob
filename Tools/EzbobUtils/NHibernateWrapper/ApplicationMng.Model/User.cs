using Iesi.Collections.Generic;
using NHibernateWrapper.NHibernate.Model;
using System;
using System.Collections.Generic;
using System.Linq;
namespace ApplicationMng.Model
{
	[System.Serializable]
	public class User
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
		public virtual string FullName
		{
			get;
			set;
		}
		public virtual int IsDeleted
		{
			get;
			set;
		}
		public virtual System.DateTime CreationDate
		{
			get;
			set;
		}
		public virtual string CertificateThumbprint
		{
			get;
			set;
		}
		public virtual string DomainUserName
		{
			get;
			set;
		}
		public virtual string EMail
		{
			get;
			set;
		}
		public virtual Iesi.Collections.Generic.ISet<Role> Roles
		{
			get;
			set;
		}
		public virtual int BranchId
		{
			get;
			set;
		}
		public virtual string Password
		{
			get;
			set;
		}
		public virtual System.DateTime? PassSetTime
		{
			get;
			set;
		}
		public virtual System.DateTime? DisableDate
		{
			get;
			set;
		}
		public virtual System.DateTime? LastBadLogin
		{
			get;
			set;
		}
		public virtual System.DateTime? DeletionDate
		{
			get;
			set;
		}
		public virtual int? LoginFailedCount
		{
			get;
			set;
		}
		public virtual int? PassExpPeriod
		{
			get;
			set;
		}
		public virtual int? DeleteId
		{
			get;
			set;
		}
		public virtual User Deleter
		{
			get;
			set;
		}
		public virtual bool? ForcePassChange
		{
			get;
			set;
		}
		public virtual bool? DisablePassChange
		{
			get;
			set;
		}
		public virtual User Creator
		{
			get;
			set;
		}
		public virtual SecurityQuestion SecurityQuestion
		{
			get;
			set;
		}
		public virtual string SecurityAnswer
		{
			get;
			set;
		}
		public virtual bool IsPasswordRestored
		{
			get;
			set;
		}
		public virtual System.Collections.Generic.IEnumerable<Permission> Permissions
		{
			get
			{
				return this.Roles.SelectMany((Role r) => r.Permissions);
			}
		}
		public User()
		{
			this.Roles = new HashedSet<Role>();
			this.CreationDate = System.DateTime.Now;
		}
	}
}
