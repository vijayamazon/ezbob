namespace EZBob.DatabaseLib.Model.Database.UserManagement {
	using FluentNHibernate.Mapping;

	public class UserMap : ClassMap<User> {
		public UserMap() {
			Cache.Region("LongTerm").ReadWrite();
			Table("Security_User");
			Id(u => (object)u.Id).Column("UserId").GeneratedBy.Native("SEQ_INSERT_SECURITY_USER");
			Map(u => u.Name).Column("UserName").Length(250);
			Map(u => u.FullName).Column("FullName").Length(250);
			Map(u => (object)u.IsDeleted);
			Map(u => (object)u.CreationDate);
			Map(u => u.CertificateThumbprint).Length(40);
			Map(u => u.DomainUserName).Length(250);
			Map(u => u.EMail).Length(255);
			Map(u => (object)u.BranchId);
			Map(u => u.Password).Length(200);
			Map(u => (object)u.PassSetTime, "PassSetTime");
			Map(u => (object)u.DisableDate, "DisableDate");
			Map(u => (object)u.LastBadLogin, "LastBadLogin");
			Map(u => (object)u.DeletionDate, "DeletionDate");
			Map(u => (object)u.LoginFailedCount, "LoginFailedCount");
			Map(u => (object)u.PassExpPeriod, "PassExpPeriod");
			Map(u => (object)u.ForcePassChange, "ForcePassChange");
			Map(u => (object)u.DisablePassChange, "DisablePassChange");
			Map(u => (object)u.DeleteId, "DeleteId");
			Map(u => (object)u.IsPasswordRestored);
			References(u => u.Creator, "CREATEUSERID");
			References(u => u.Deleter, "DELETEUSERID");
			HasManyToMany(u => u.Roles).AsSet().Table("Security_UserRoleRelation").ParentKeyColumn("UserId").ChildKeyColumn("RoleId").Cascade.All().Cache.Region("LongTerm").ReadWrite();
			References(u => u.SecurityQuestion, "SecurityQuestion1Id").LazyLoad();
			Map(u => u.SecurityAnswer, "SecurityAnswer1").Length(200);
			Map(u => u.EzPassword).Length(255);
			Map(u => u.EmailStateID);
			Map(u => u.OriginID);
		} // constructor
	} // class UserMap
} // namespace
