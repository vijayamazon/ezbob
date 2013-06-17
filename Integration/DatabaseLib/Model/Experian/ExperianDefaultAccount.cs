using System;
using EZBob.DatabaseLib.Model.Database;
using FluentNHibernate.Mapping;
using NHibernate.Type;

namespace EZBob.DatabaseLib.Model.Experian
{
	public class ExperianDefaultAccount
	{
		public virtual int Id { get; set; }
		public virtual DateTime DateAdded { get; set; }
		public virtual Customer Customer { get; set; }
		public virtual string AccountType { get; set; }
		public virtual DateTime Date { get; set; }
		public virtual string DelinquencyType { get; set; }
		public virtual MP_ServiceLog ServiceLog { get; set; }
		public virtual int Balance { get; set; }
		public virtual int CurrentDefBalance { get; set; }
	}

	public sealed class ExperianDefaultAccountMap : ClassMap<ExperianDefaultAccount>
	{
		public ExperianDefaultAccountMap()
		{
			Table("ExperianDefaultAccount");
			Id(x => x.Id).GeneratedBy.HiLo("1000");
			Map(x => x.AccountType);
			Map(x => x.Date).CustomType<UtcDateTimeType>();
			Map(x => x.DateAdded).CustomType<UtcDateTimeType>();
			Map(x => x.DelinquencyType);
			Map(x => x.Balance);
			Map(x => x.CurrentDefBalance);
			References(x => x.ServiceLog, "ServiceLogId");
			References(x => x.Customer, "CustomerId");
		}
	}
}