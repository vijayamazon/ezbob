using System;
using FluentNHibernate.Mapping;

namespace EZBob.DatabaseLib.Model.Database
{
	using System.ComponentModel;

	public enum ExperianServiceType
	{
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
		Targeting
	}
	public class MP_ServiceLog
	{
		public virtual long Id { get; set; }
		public virtual Customer Customer { get; set; }
		public virtual Director Director { get; set; }
		public virtual DateTime InsertDate { get; set; }
		public virtual string ServiceType { get; set; }
		public virtual string RequestData { get; set; }
		public virtual string ResponseData { get; set; }
	}

	//-----------------------------------------------------------------------------------
	public sealed class MP_ServiceLogMap : ClassMap<MP_ServiceLog>
	{
		public MP_ServiceLogMap()
		{
			Table("MP_ServiceLog");
			Id(x => x.Id);
			References(x => x.Customer, "CustomerId");
			References(x => x.Director, "DirectorId");
			Map(x => x.InsertDate);
			Map(x => x.ServiceType);
			Map(x => x.RequestData).CustomType("StringClob");
			Map(x => x.ResponseData).CustomType("StringClob");
		}
	}
}