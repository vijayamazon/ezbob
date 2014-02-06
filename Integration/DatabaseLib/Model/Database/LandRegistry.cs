namespace EZBob.DatabaseLib.Model.Database
{
	using System;
	using FluentNHibernate.Mapping;
	using NHibernate.Type;
	
	#region class LandRegistry

	public class LandRegistry
	{
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
	} // class LandRegistry

	#endregion class LandRegistry

	#region LandRegistryMap
	public class LandRegistryMap : ClassMap<LandRegistry>
	{
		public LandRegistryMap()
		{
			Table("LandRegistry");
			Cache.ReadWrite().Region("LongTerm").ReadWrite();

			Id(x => x.Id);
			References(x => x.Customer, "CustomerId");
			
			Map(x => x.Postcode).Length(15);
			Map(x => x.TitleNumber).Length(20);
			Map(x => x.RequestType).CustomType<LandRegistryRequestTypeType>();
			Map(x => x.ResponseType).CustomType<LandRegistryResponseTypeType>();
			Map(x => x.Request).CustomType("StringClob");
			Map(x => x.Response).CustomType("StringClob");
			Map(x => x.AttachmentPath).Length(300);
		} // constructor
	} // class LandRegistryMap
	#endregion 

	public class LandRegistryRequestTypeType : EnumStringType<LandRegistryRequestType> { }

	public class LandRegistryResponseTypeType : EnumStringType<LandRegistryResponseType> { }

	public enum LandRegistryRequestType
	{
		Enquiry,//EnquiryByPropertyDescription
		EnquiryPoll,//EnquiryByPropertyDescriptionPoll
		Res,//RegisterExtractService
		ResPoll,//RegisterExtractServicePoll
	} // enum 
	
	public enum LandRegistryResponseType
	{
		Acknowledgement,
		Rejection,
		Success,
		Unkown
	} // enum 
} // namespace

namespace EZBob.DatabaseLib.Repository
{
	using System.Collections.Generic;
	using System.Linq;
	using ApplicationMng.Repository;
	using Model.Database;
	using NHibernate;
	using NHibernate.Linq;

	public class LandRegistryRepository : NHibernateRepositoryBase<LandRegistry>
	{
		public LandRegistryRepository(ISession session)
			: base(session)
		{
		}
		public IEnumerable<LandRegistry> GetByCustomer(Customer customer)
		{
			return GetAll().Where(x => x.Customer.Id == customer.Id).ToFuture();
		}

		public LandRegistry GetRes(Customer customer)
		{
			return GetAll().LastOrDefault(x => 
				x.Customer == customer && 
				(x.RequestType == LandRegistryRequestType.Res || 
				 x.RequestType == LandRegistryRequestType.ResPoll));
		}

		public LandRegistry GetEnquiry(Customer customer)
		{
			return GetAll().LastOrDefault(x =>
				x.Customer == customer &&
				(x.RequestType == LandRegistryRequestType.Enquiry ||
				 x.RequestType == LandRegistryRequestType.EnquiryPoll));
		}

		public LandRegistry GetByTitleNumber(string titleNumber)
		{
			return GetAll().LastOrDefault(x =>
				x.TitleNumber == titleNumber &&
				(x.RequestType == LandRegistryRequestType.Res ||
				 x.RequestType == LandRegistryRequestType.ResPoll));
		}
	}
}