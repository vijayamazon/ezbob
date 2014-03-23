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
		public virtual string TranslatedName
		{
			get
			{
				string @string = NHibernateWrapper.NHibernate.SecurityApplication.ResourceManager.GetString(this.Name);
				string result;
				if (string.IsNullOrEmpty(@string))
				{
					result = this.Name;
				}
				else
				{
					result = @string;
				}
				return result;
			}
		}
		public virtual System.Collections.Generic.IList<MenuItem> MenuItems
		{
			get;
			set;
		}
		public virtual Iesi.Collections.Generic.ISet<Role> Roles
		{
			get;
			set;
		}
		public virtual SecurityApplicationType ApplicationType
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
