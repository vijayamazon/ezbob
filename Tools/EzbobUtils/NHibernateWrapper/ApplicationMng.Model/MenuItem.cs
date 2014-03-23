using ApplicationMng.Model.Filters;
using Iesi.Collections.Generic;
using NHibernateWrapper.NHibernate;
using System;
namespace ApplicationMng.Model
{
	public class MenuItem
	{
		private ISet<AppStatus> _statuses = new HashedSet<AppStatus>();
		private ISet<MenuItem> _tabs = new HashedSet<MenuItem>();
		public virtual int Id
		{
			get;
			set;
		}
		public virtual SecurityApplication SecApp
		{
			get;
			set;
		}
		public virtual string Caption
		{
			get;
			set;
		}
		public virtual string Description
		{
			get;
			set;
		}
		public virtual string Url
		{
			get;
			set;
		}
		public virtual FilterBase Filter
		{
			get;
			set;
		}
		public virtual string FilterData
		{
			get;
			set;
		}
		public virtual MenuItem Parent
		{
			get;
			set;
		}
		public virtual ISet<MenuItem> Tabs
		{
			get
			{
				return this._tabs;
			}
			set
			{
				this._tabs = value;
			}
		}
		
		public virtual string Path
		{
			get
			{
				return this.Url.Contains("/") ? this.Url : ("~/" + this.MvcPath);
			}
		}
		public virtual string MvcPath
		{
			get
			{
				return (this.SecApp.Name + this.GetPathToRoot()).TrimStart(new char[0]).TrimEnd(new char[0]).Replace(" ", "_") + ".aspx";
			}
		}
		public virtual ISet<AppStatus> Statuses
		{
			get
			{
				return this._statuses;
			}
			set
			{
				this._statuses = value;
			}
		}
		private string GetPathToRoot()
		{
			string result;
			if (this.Parent == null)
			{
				result = "/" + this.Caption;
			}
			else
			{
				result = this.Parent.GetPathToRoot() + "/" + this.Caption;
			}
			return result;
		}
	}
}
