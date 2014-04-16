namespace EZBob.DatabaseLib.Model.Database.UserManagement
{
	using Iesi.Collections.Generic;

	public class Permission
	{
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
		public virtual string Description { get; set; }
		public virtual ISet<Role> Roles { get; set; }

		public Permission()
		{
			Roles = new HashedSet<Role>();
		}
	}
}
