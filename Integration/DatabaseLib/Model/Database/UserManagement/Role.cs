namespace EZBob.DatabaseLib.Model.Database.UserManagement
{
	using System.Diagnostics;
	using Iesi.Collections.Generic;

	[DebuggerDisplay("Id = {_id}, Name = {_name}, Description = {_description}")]
	public class Role
	{
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
		public virtual string Description { get; set; }
		public virtual ISet<User> Users { get; set; }
		public virtual ISet<SecurityApplication> Applications { get; set; }
		public virtual ISet<Permission> Permissions { get; set; }

		public Role()
		{
			Users = new HashedSet<User>();
			Applications = new HashedSet<SecurityApplication>();
			Permissions = new HashedSet<Permission>();
		}

		public virtual void AddUsers(System.Collections.Generic.List<User> users)
		{
			foreach (User current in users)
			{
				current.Roles.Add(this);
				Users.Add(current);
			}
		}

		public virtual void ClearUsers()
		{
			foreach (User current in Users)
			{
				current.Roles.Remove(this);
			}
			Users.Clear();
		}
	}
}
