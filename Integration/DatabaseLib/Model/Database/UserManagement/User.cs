namespace EZBob.DatabaseLib.Model.Database.UserManagement {
	using System.Collections.Generic;
	using Iesi.Collections.Generic;
	using System.Linq;

	[System.Serializable]
	public class User {
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
		public virtual string FullName { get; set; }
		public virtual int IsDeleted { get; set; }
		public virtual System.DateTime CreationDate { get; set; }
		public virtual string CertificateThumbprint { get; set; }
		public virtual string DomainUserName { get; set; }
		public virtual string EMail { get; set; }
		public virtual Iesi.Collections.Generic.ISet<Role> Roles { get; set; }
		public virtual int BranchId { get; set; }
		public virtual string Password { get; set; }
		public virtual System.DateTime? PassSetTime { get; set; }
		public virtual System.DateTime? DisableDate { get; set; }
		public virtual System.DateTime? LastBadLogin { get; set; }
		public virtual System.DateTime? DeletionDate { get; set; }
		public virtual int? LoginFailedCount { get; set; }
		public virtual int? PassExpPeriod { get; set; }
		public virtual int? DeleteId { get; set; }
		public virtual User Deleter { get; set; }
		public virtual bool? ForcePassChange { get; set; }
		public virtual bool? DisablePassChange { get; set; }
		public virtual User Creator { get; set; }
		public virtual SecurityQuestion SecurityQuestion { get; set; }
		public virtual string SecurityAnswer { get; set; }
		public virtual bool IsPasswordRestored { get; set; }
		public virtual string EzPassword { get; set; }
		public virtual int? EmailStateID { get; set; }
		public virtual int? OriginID { get; set; }

		public virtual IEnumerable<Permission> Permissions {
			get { return Roles.SelectMany(r => r.Permissions); } // get
		} // Permisssions

		public User() {
			Roles = new HashedSet<Role>();
			CreationDate = System.DateTime.Now;
		} // constructor
	} // class User
} // namespace
