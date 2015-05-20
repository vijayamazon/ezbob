namespace EZBob.DatabaseLib.Model.Alibaba {
	using System;
	using System.Linq;
	using ApplicationMng.Repository;
	using EZBob.DatabaseLib.Model.Database;
	using FluentNHibernate.Mapping;
	using NHibernate;

	public class AlibabaSellerBank {

		public virtual long SellerBankID { get; set; }
		public virtual AlibabaSeller Seller { get; set; }
		public virtual string BeneficiaryBank { get; set; }
		public virtual string StreetAddr1 { get; set; }
		public virtual string StreetAddr2 { get; set; }
		public virtual string City { get; set; }
		public virtual int? State { get; set; }
		public virtual string Country { get; set; }
		public virtual string PostalCode { get; set; }
		public virtual string SwiftCode { get; set; }
		public virtual string AccountNumber { get; set; }
		public virtual string WireInstructions { get; set; }
		
	}

	public class AlibabaSellerBankRepository : NHibernateRepositoryBase<AlibabaSellerBank> {
		public AlibabaSellerBankRepository(ISession session) : base(session) { }

		public AlibabaSellerBank BySeller(int sellerId) {
			return GetAll().FirstOrDefault(k => k.Seller.SellerID == sellerId);
		}
	}


	public class AlibabaSellerBankMap : ClassMap<AlibabaSellerBank> {
		public AlibabaSellerBankMap() {

			Table("AlibabaSellerBank");

			Id(x => x.SellerBankID).GeneratedBy.Identity();

			Map(x => x.BeneficiaryBank);
			Map(x => x.StreetAddr1);
			Map(x => x.StreetAddr2);
			Map(x => x.City);
			Map(x => x.State);
			Map(x => x.Country);
			Map(x => x.PostalCode);
			Map(x => x.SwiftCode);
			Map(x => x.AccountNumber);
			Map(x => x.WireInstructions);

			References(x => x.Seller, "SellerId").LazyLoad().Cascade.None();

		} // constructor
	}
}
