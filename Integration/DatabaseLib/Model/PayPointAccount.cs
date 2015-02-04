
namespace EZBob.DatabaseLib.Model {
	using FluentNHibernate.Mapping;
	using System.Linq;
	using ApplicationMng.Repository;
	using NHibernate;

	public class PayPointAccount {
		public virtual int PayPointAccountID { get; set; }
		public virtual bool IsDefault { get; set; }
		public virtual string Mid { get; set; }
		public virtual string VpnPassword { get; set; }
		public virtual string RemotePassword { get; set; }
		public virtual string Options { get; set; }
		public virtual string TemplateUrl { get; set; }
		public virtual string ServiceUrl { get; set; }
		public virtual bool EnableCardLimit { get; set; }
		public virtual int CardLimitAmount { get; set; }
		public virtual int CardExpiryMonths { get; set; }
		public virtual bool DebugModeEnabled { get; set; }
		public virtual bool DebugModeErrorCodeNEnabled { get; set; }
		public virtual bool DebugModeIsValidCard { get; set; }
	}

	public class PayPointAccountMap : ClassMap<PayPointAccount> {
		public PayPointAccountMap() {
			Id(x => x.PayPointAccountID).GeneratedBy.Native();
			Map(x => x.IsDefault);
			Map(x => x.Mid).Length(30);
			Map(x => x.VpnPassword).Length(30);
			Map(x => x.RemotePassword).Length(30);
			Map(x => x.Options).Length(100);
			Map(x => x.TemplateUrl).Length(300);
			Map(x => x.ServiceUrl).Length(300);
			Map(x => x.EnableCardLimit);
			Map(x => x.CardLimitAmount);
			Map(x => x.CardExpiryMonths);
			Map(x => x.DebugModeEnabled);
			Map(x => x.DebugModeErrorCodeNEnabled);
			Map(x => x.DebugModeIsValidCard);
		}
	}

	public interface IPayPointAccountRepository : IRepository<PayPointAccount> {
		PayPointAccount GetDefaultAccount();
		PayPointAccount GetOldAccount();
	}

	public class PayPointAccountRepository : NHibernateRepositoryBase<PayPointAccount>, IPayPointAccountRepository {
		public PayPointAccountRepository(ISession session)
			: base(session) {
		}

		public PayPointAccount GetDefaultAccount() {
			return GetAll().FirstOrDefault(x => x.IsDefault);
		}

		public PayPointAccount GetOldAccount() {
			var account = GetAll().FirstOrDefault(x => x.Mid == "orange06");
			if (account == null) {
				account = GetAll().FirstOrDefault(x => x.Mid == "secpay");
			}

			return account;
		}
	}

}