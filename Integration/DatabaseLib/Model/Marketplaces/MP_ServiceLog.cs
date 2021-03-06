namespace EZBob.DatabaseLib.Model.Database {
	using System;
	using System.ComponentModel;
	using FluentNHibernate.Mapping;
	using NHibernate.Type;

	public enum ExperianServiceType {
		[Description("AML A check")]
		Aml,
		[Description("Askville")]
		Askville,
		[Description("BWA check")]
		Bwa,
		[Description("Consumer Request")]
		Consumer,
		[Description("E-SeriesLimitedData")]
		LimitedData,
		[Description("E-SeriesNonLimitedData")]
		NonLimitedData,
		[Description("ESeriesTargeting")]
		Targeting,
		[Description("CreditSafeLtd")]
		CreditSafeLtd,
		[Description("CreditSafeNonLtd")]
		CreditSafeNonLtd,
		[Description("CreditSafeNonLtdTargeting")]
		CreditSafeNonLtdTargeting,
		[Description("CallCredit")]
		CallCredit
	} // enum ExperianServiceType

	public class MP_ServiceLog {
		public virtual long Id { get; set; }
		public virtual Customer Customer { get; set; }
		public virtual Director Director { get; set; }
		public virtual DateTime InsertDate { get; set; }
		public virtual string ServiceType { get; set; }
		public virtual string RequestData { get; set; }
		public virtual string ResponseData { get; set; }
		public virtual string CompanyRefNum { get; set; }
		public virtual string Firstname { get; set; }
		public virtual string Surname { get; set; }
		public virtual DateTime? DateOfBirth { get; set; }
		public virtual string Postcode { get; set; }
	} // class MP_ServiceLog

	public sealed class MP_ServiceLogMap : ClassMap<MP_ServiceLog> {
		public MP_ServiceLogMap() {
			Table("MP_ServiceLog");
			Id(x => x.Id);
			References(x => x.Customer, "CustomerId");
			References(x => x.Director, "DirectorId");
			Map(x => x.InsertDate);
			Map(x => x.ServiceType);
			Map(x => x.RequestData).CustomType("StringClob");
			Map(x => x.ResponseData).CustomType("StringClob");
			Map(x => x.CompanyRefNum).Length(50);
			Map(x => x.Firstname).Length(50).Nullable();
			Map(x => x.Surname).Length(50).Nullable();
			Map(x => x.DateOfBirth).CustomType<UtcDateTimeType>().Nullable();
			Map(x => x.Postcode).Length(50).Nullable();
		} // constructor
	} // class MP_ServiceLogMap
} // namespace
