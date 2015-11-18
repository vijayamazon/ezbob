namespace EZBob.DatabaseLib.Model.Database {
	using System.Linq;
	using ApplicationMng.Repository;
	using FluentNHibernate.Mapping;
	using NHibernate;

	public class CardInfo {
		public CardInfo() {}

		public CardInfo(string bankAccount, string sortCode) {
			BankAccount = bankAccount;
			SortCode = sortCode;
		} // constructor

		public virtual int Id { get; set; }
		public virtual string Bank { get; set; }
		public virtual string BankBIC { get; set; }
		public virtual string Branch { get; set; }
		public virtual string BranchBIC { get; set; }
		public virtual string ContactAddressLine1 { get; set; }
		public virtual string ContactAddressLine2 { get; set; }
		public virtual string ContactPostTown { get; set; }
		public virtual string ContactPostcode { get; set; }
		public virtual string ContactPhone { get; set; }
		public virtual string ContactFax { get; set; }
		public virtual bool FasterPaymentsSupported { get; set; }
		public virtual bool CHAPSSupported { get; set; }
		public virtual string SortCode { get; set; }
		public virtual string IBAN { get; set; }
		public virtual bool IsDirectDebitCapable { get; set; }
		public virtual string StatusInformation { get; set; }
		public virtual string BankAccount { get; set; }
		public virtual string BWAResult { get; set; }
		public virtual BankAccountType Type { get; set; }
		public virtual Customer Customer { get; set; }
		public virtual Broker.Broker Broker { get; set; }
		public virtual bool? IsDefault { get; set; }
	} // class CardInfo

	public class CardInfoMap : ClassMap<CardInfo> {
		public CardInfoMap() {
			Id(x => x.Id).GeneratedBy.HiLo("100");
			Map(x => x.Bank).Length(1000);
			Map(x => x.BankBIC).Length(200);
			Map(x => x.Branch).Length(1000);
			Map(x => x.BranchBIC).Length(200);
			Map(x => x.ContactAddressLine1).Length(200);
			Map(x => x.ContactAddressLine2).Length(200);
			Map(x => x.ContactPostTown).Length(200);
			Map(x => x.ContactPostcode).Length(200);
			Map(x => x.ContactPhone).Length(200);
			Map(x => x.ContactFax).Length(200);
			Map(x => x.FasterPaymentsSupported);
			Map(x => x.CHAPSSupported);
			Map(x => x.IBAN).Length(200);
			Map(x => x.IsDirectDebitCapable);
			Map(x => x.StatusInformation).Length(200);
			Map(x => x.SortCode).Length(20);
			Map(x => x.BankAccount).Length(8);
			Map(x => x.BWAResult).Length(100);
			Map(x => x.Type, "BankAccountType").CustomType<BankAccountTypeType>();
			Map(x => x.IsDefault);
			References(x => x.Customer, "CustomerId");
			References(x => x.Broker, "BrokerID");
		} // constructor
	} // class CardInfoMap

	public interface ICardInfoRepository : IRepository<CardInfo> {
		bool Exists(CardInfo cardInfo, int customerID);
	} // interface ICardInfoRepository

	public class CardInfoRepository : NHibernateRepositoryBase<CardInfo>, ICardInfoRepository {
		public CardInfoRepository(ISession session) : base(session) {}

		public bool Exists(CardInfo cardInfo, int customerID) {
			return GetAll()
				.Any(x => x.BankAccount == cardInfo.BankAccount &&
						  x.SortCode == cardInfo.SortCode &&
						  x.Customer.Id != customerID);
		} // Exists
	} // class CardInfoRepository
} // namespace
