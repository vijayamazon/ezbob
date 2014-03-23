using FluentNHibernate.Mapping;
using NHibernateWrapper.NHibernate.Model;
using System;
namespace ApplicationMng.Model.Mappings
{
	public class UserMap : ClassMap<User>
	{
		public UserMap()
		{
			base.Cache.Region("LongTerm").ReadWrite();
			base.Table("Security_User");
			this.Id((User x) => (object)x.Id).Column("UserId").GeneratedBy.Native("SEQ_INSERT_SECURITY_USER");
			base.Map((User x) => x.Name).Column("UserName").Length(250);
			base.Map((User x) => x.FullName).Column("FullName").Length(250);
			base.Map((User x) => (object)x.IsDeleted);
			base.Map((User x) => (object)x.CreationDate);
			base.Map((User x) => x.CertificateThumbprint).Length(40);
			base.Map((User x) => x.DomainUserName).Length(250);
			base.Map((User x) => x.EMail).Length(255);
			base.Map((User x) => (object)x.BranchId);
			base.Map((User x) => x.Password).Length(200);
			base.Map((User x) => (object)x.PassSetTime, "PassSetTime");
			base.Map((User x) => (object)x.DisableDate, "DisableDate");
			base.Map((User x) => (object)x.LastBadLogin, "LastBadLogin");
			base.Map((User x) => (object)x.DeletionDate, "DeletionDate");
			base.Map((User x) => (object)x.LoginFailedCount, "LoginFailedCount");
			base.Map((User x) => (object)x.PassExpPeriod, "PassExpPeriod");
			base.Map((User x) => (object)x.ForcePassChange, "ForcePassChange");
			base.Map((User x) => (object)x.DisablePassChange, "DisablePassChange");
			base.Map((User x) => (object)x.DeleteId, "DeleteId");
			base.Map((User x) => (object)x.IsPasswordRestored);
			base.References<User>((User x) => x.Creator, "CREATEUSERID");
			base.References<User>((User x) => x.Deleter, "DELETEUSERID");
			base.HasManyToMany<Role>((User x) => x.Roles).AsSet().Table("Security_UserRoleRelation").ParentKeyColumn("UserId").ChildKeyColumn("RoleId").Cascade.All().Cache.Region("LongTerm").ReadWrite();
			base.References<SecurityQuestion>((User x) => x.SecurityQuestion, "SecurityQuestion1Id").LazyLoad();
			base.Map((User x) => x.SecurityAnswer, "SecurityAnswer1").Length(200);
		}
	}
}
