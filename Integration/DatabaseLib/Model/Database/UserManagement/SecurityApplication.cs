namespace EZBob.DatabaseLib.Model.Database.UserManagement
{
	using Iesi.Collections.Generic;

	public class SecurityApplication
	{
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
		public virtual ISet<Role> Roles { get; set; }
		public virtual string Description { get; set; }

		public SecurityApplication()
		{
			Roles = new HashedSet<Role>();
		}
	}
}
