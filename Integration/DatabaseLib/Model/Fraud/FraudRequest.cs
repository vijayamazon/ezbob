using System;
using EZBob.DatabaseLib.Model.Database;
using FluentNHibernate.Mapping;
using NHibernate.Type;

namespace EZBob.DatabaseLib.Model.Fraud
{
	using Iesi.Collections.Generic;

	public class FraudRequest
	{
		public virtual int Id { get; set; }
		public virtual Customer Customer { get; set; }
		public virtual DateTime CheckDate { get; set; }
		public virtual ISet<FraudDetection> FraudDetections { get; set; }
	}

	public sealed class FraudRequestMap : ClassMap<FraudRequest>
	{
		public FraudRequestMap()
		{
			Table("FraudRequest");
			Id(x => x.Id);
			References(x => x.Customer, "CustomerId");
			Map(x => x.CheckDate).CustomType<UtcDateTimeType>();
			HasMany<FraudDetection>(x => x.FraudDetections)
				.AsSet()
				.KeyColumn("FraudRequestId")
				.Cascade.AllDeleteOrphan()
				.Inverse();
		}
	}
}