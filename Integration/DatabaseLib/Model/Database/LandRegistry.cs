namespace EZBob.DatabaseLib.Model.Database {
	using System;
	using System.Collections.Generic;
	using FluentNHibernate.Mapping;
	using NHibernate.Type;
	using LandRegistryLib;

	public class LandRegistry {
		public virtual int Id { get; set; }
		public virtual Customer Customer { get; set; }
		public virtual DateTime InsertDate { get; set; }
		public virtual string Postcode { get; set; }
		public virtual string TitleNumber { get; set; }
		public virtual LandRegistryRequestType RequestType { get; set; }
		public virtual LandRegistryResponseType ResponseType { get; set; }
		public virtual string Request { get; set; }
		public virtual string Response { get; set; }
		public virtual string AttachmentPath { get; set; }
		public virtual CustomerAddress CustomerAddress { get; set; }
		public virtual IList<LandRegistryOwner> Owners { get; set; }
	}

	public class LandRegistryMap : ClassMap<LandRegistry> {
		public LandRegistryMap() {
			Table("LandRegistry");
			Cache.ReadWrite().Region("LongTerm").ReadWrite();

			Id(x => x.Id);
			References(x => x.Customer, "CustomerId");
			Map(x => x.InsertDate).CustomType<UtcDateTimeType>();
			Map(x => x.Postcode).Length(15);
			Map(x => x.TitleNumber).Length(20);
			Map(x => x.RequestType).CustomType<LandRegistryRequestTypeType>();
			Map(x => x.ResponseType).CustomType<LandRegistryResponseTypeType>();
			Map(x => x.Request).CustomType("StringClob");
			Map(x => x.Response).CustomType("StringClob");
			Map(x => x.AttachmentPath).Length(300);
			References(x => x.CustomerAddress, "AddressId");

			HasMany(x => x.Owners)
				.AsBag()
				.KeyColumn("LandRegistryId")
				.Cascade.All()
				.Inverse();
		}
	}

	public class LandRegistryRequestTypeType : EnumStringType<LandRegistryRequestType> { }

	public class LandRegistryResponseTypeType : EnumStringType<LandRegistryResponseType> { }
}

namespace EZBob.DatabaseLib.Repository {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using ApplicationMng.Repository;
	using LandRegistryLib;
	using Model.Database;
	using NHibernate;
	using NHibernate.Linq;

	public class LandRegistryRepository : NHibernateRepositoryBase<LandRegistry> {
		public LandRegistryRepository(ISession session) : base(session) { }

		public IEnumerable<LandRegistry> GetByCustomer(Customer customer) {
			return GetAll().Where(x => x.Customer.Id == customer.Id).ToFuture();
		}

		public LandRegistry GetRes(int customerId, string titleNumber) {
			bool bIsEmptyTitleNumber = string.IsNullOrWhiteSpace(titleNumber);

			Func<LandRegistry, bool> oIsMatch = x => {
				if (x.Customer.Id != customerId)
					return false;

				if (!IsResRequest(x))
					return false;

				if (x.ResponseType != LandRegistryResponseType.Success)
					return false;

				if (bIsEmptyTitleNumber)
					return true;

				return x.TitleNumber == titleNumber;
			};

			return GetAll().Where(oIsMatch).OrderByDescending(x => x.InsertDate).FirstOrDefault();
		}

		public List<LandRegistry> GetEnquiry(int customerId, string postCode) {
			return GetAll()
				.Where(x =>
					   (x.Customer.Id == customerId) &&
					   (x.RequestType == LandRegistryRequestType.Enquiry || x.RequestType == LandRegistryRequestType.EnquiryPoll) &&
					   (x.ResponseType == LandRegistryResponseType.Success) &&
					   (x.Postcode == postCode))
				.OrderByDescending(x => x.InsertDate)
				.ToList();

		}

		public LandRegistry GetByTitleNumber(string titleNumber) {
			return GetAll().Where(x => x.TitleNumber == titleNumber && IsResRequest(x)).OrderByDescending(x => x.InsertDate).FirstOrDefault();
		}

		private bool IsResRequest(LandRegistry x) {
			return
				(x.RequestType == LandRegistryRequestType.Res) ||
				(x.RequestType == LandRegistryRequestType.ResPoll);
		}
	}
}