namespace EZBob.DatabaseLib.Model.Database.UserManagement {
	using System;

	public class SecuritySession {
		public virtual User User { get; set; }
		public virtual int State { get; set; }
		public virtual string Id { get; set; }
		public virtual DateTime CreationDate { get; set; }
		public virtual DateTime LastAccessTime { get; set; }
		public virtual string HostAddress { get; set; }

		public SecuritySession() {
			CreationDate = DateTime.UtcNow;
			LastAccessTime = DateTime.UtcNow;
		}

		public SecuritySession(User user) {
			CreationDate = DateTime.UtcNow;
			LastAccessTime = DateTime.UtcNow;
			User = user;
		}

		public virtual bool IsActive() {
			return State == 1;
		}
	}
}
