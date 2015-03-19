namespace EZBob.DatabaseLib.Model.Loans {
	using System.Linq;
	using ApplicationMng.Repository;
	using FluentNHibernate.Mapping;
	using NHibernate;

	public class BrokerSetupFeeMap {
		public virtual int Id { get; set; }
		public virtual int MinAmount { get; set; }
		public virtual int MaxAmount { get; set; }
		public virtual int Fee { get; set; }
	} // class BrokerSetupFeeMap

	public class BrokerSetupFeeMapMap : ClassMap<BrokerSetupFeeMap> {
		public BrokerSetupFeeMapMap() {
			Id(x => x.Id);
			Map(x => x.MinAmount);
			Map(x => x.MaxAmount);
			Map(x => x.Fee);
		} // constructor
	} // class BrokerSetupFeeMapMap

	public interface IBrokerSetupFeeMapRepository : IRepository<BrokerSetupFeeMap> {
		int GetFee(int amount);
	} // interface IBrokerSetupFeeMapRepository

	public class BrokerSetupFeeMapRepository : NHibernateRepositoryBase<BrokerSetupFeeMap>, IBrokerSetupFeeMapRepository {
		public BrokerSetupFeeMapRepository(ISession session) : base(session) {}

		public int GetFee(int amount) {
			var fee = GetAll().FirstOrDefault(x => x.MinAmount <= amount && x.MaxAmount >= amount);
			return fee != null ? fee.Fee : 0;
		} // GetFee
	} // class BrokerSetupFeeMapRepository
} // namespace
