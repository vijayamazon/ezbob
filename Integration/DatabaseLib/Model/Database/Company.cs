namespace EZBob.DatabaseLib.Model.Database
{
	using FluentNHibernate.Mapping;
	using Iesi.Collections.Generic;
	using NHibernate.Type;

	public class Company
	{
		public virtual int Id { get; set; }
		public virtual TypeOfBusiness TypeOfBusiness { get; set; }
		public virtual VatReporting? VatReporting { get; set; }
		public virtual bool VatRegistered { get; set; }
		public virtual string CompanyNumber { get; set; }
		public virtual string CompanyName { get; set; }
		public virtual int? TimeAtAddress { get; set; }
		public virtual string TimeInBusiness { get; set; }
		public virtual string BusinessPhone { get; set; }
		public virtual bool? PropertyOwnedByCompany { get; set; }
		public virtual string YearsInCompany { get; set; }
		public virtual string RentMonthLeft { get; set; }
		public virtual double? CapitalExpenditure { get; set; }
		public virtual string ExperianRefNum { get; set; }
		public virtual string ExperianCompanyName { get; set; }

		private ISet<CustomerAddress> _companyAddress = new HashedSet<CustomerAddress>();
		public virtual ISet<CustomerAddress> CompanyAddress
		{
			get { return _companyAddress; }
			set { _companyAddress = value; }
		} //CompanyAddress

		private ISet<CustomerAddress> _experianCompanyAddress = new HashedSet<CustomerAddress>();
		public virtual ISet<CustomerAddress> ExperianCompanyAddress
		{
			get { return _experianCompanyAddress; }
			set { _experianCompanyAddress = value; }
		} //CompanyAddress

		private ISet<CompanyEmployeeCount> _companyEmployeeCount = new HashedSet<CompanyEmployeeCount>();
		public virtual ISet<CompanyEmployeeCount> CompanyEmployeeCount
		{
			get { return _companyEmployeeCount; }
			set { _companyEmployeeCount = value; }
		} // CompanyEmployeeCount

		private ISet<Director> _directors = new HashedSet<Director>();

		public virtual ISet<Director> Directors
		{
			get { return _directors; }
			set { _directors = value; }
		} // Directors

	} // class Company

	public class CompanyMap : ClassMap<Company>
	{
		public CompanyMap()
		{
			Table("Company");
			Cache.ReadWrite().Region("LongTerm").ReadWrite();

			Id(x => x.Id);
			Map(x => x.TypeOfBusiness).CustomType<TypeOfBusinessType>();
			Map(x => x.VatReporting).CustomType<VatReportingType>();
			Map(x => x.VatRegistered);
			Map(x => x.CompanyNumber).Length(100);
			Map(x => x.CompanyName).Length(300);
			Map(x => x.TimeAtAddress);
			Map(x => x.TimeInBusiness).Length(250);
			Map(x => x.BusinessPhone).Length(250);
			Map(x => x.PropertyOwnedByCompany).Length(250);
			Map(x => x.YearsInCompany).Length(250);
			Map(x => x.RentMonthLeft).Length(250);
			Map(x => x.ExperianRefNum).Length(250);
			Map(x => x.ExperianCompanyName).Length(250);

			HasMany(x => x.CompanyAddress)
				 .AsSet()
				 .KeyColumn("CompanyId")
				 .Where(a => a.AddressType != CustomerAddressType.ExperianCompanyAddress)
				 .Cascade.All()
				 .Inverse()
				 .Cache.ReadWrite().Region("LongTerm").ReadWrite();

			HasMany(x => x.ExperianCompanyAddress)
				 .AsSet()
				 .KeyColumn("CompanyId")
				 .Where(a => a.AddressType == CustomerAddressType.ExperianCompanyAddress)
				 .Cascade.All()
				 .Inverse()
				 .Cache.ReadWrite().Region("LongTerm").ReadWrite();

			HasMany(x => x.Directors)
				 .AsSet()
				 .KeyColumn("CompanyId")
				 .Cascade.All()
				 .Inverse()
				 .Cache.ReadWrite().Region("LongTerm").ReadWrite();

			HasMany(x => x.CompanyEmployeeCount)
				.AsSet()
				.KeyColumn("CompanyId")
				.Cascade.All()
				.Inverse()
				.Cache.ReadWrite().Region("LongTerm").ReadWrite();
		} // constructor
	} // class CompanyMap

	public class TypeOfBusinessType : EnumStringType<TypeOfBusiness> { }

	public class IndustryTypeType : EnumStringType<IndustryType> { }

	public class VatReportingType: EnumStringType<VatReporting> { }
} // namespace
