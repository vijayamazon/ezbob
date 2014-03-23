using System;
namespace ApplicationMng.Model
{
	public class SecuritySession
	{
		public enum AppSessionState
		{
			Inactive,
			Active
		}
		public virtual User User
		{
			get;
			set;
		}
		public virtual SecurityApplication SecApp
		{
			get;
			set;
		}
		public virtual int State
		{
			get;
			set;
		}
		public virtual string Id
		{
			get;
			set;
		}
		public virtual System.DateTime CreationDate
		{
			get;
			set;
		}
		public virtual System.DateTime LastAccessTime
		{
			get;
			set;
		}
		public virtual string HostAddress
		{
			get;
			set;
		}
		public SecuritySession()
		{
			this.CreationDate = System.DateTime.Now;
			this.LastAccessTime = System.DateTime.Now;
		}
		public SecuritySession(User user, SecurityApplication app)
		{
			this.CreationDate = System.DateTime.Now;
			this.LastAccessTime = System.DateTime.Now;
			this.User = user;
			this.SecApp = app;
		}
		public virtual bool IsActive(int applicationId)
		{
			return this.SecApp.Id == applicationId && this.State == 1;
		}
		public virtual bool IsActive()
		{
			return this.State == 1;
		}
	}
}
