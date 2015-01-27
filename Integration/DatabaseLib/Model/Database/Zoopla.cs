namespace EZBob.DatabaseLib.Model.Database
{
	using System;
	using System.Web.Script.Serialization;
	using Newtonsoft.Json;

	public class Zoopla
	{
		public virtual int Id { get; set; }
		[JsonIgnore, ScriptIgnore]
		public virtual CustomerAddress CustomerAddress { get; set; }
		public virtual string AreaName { get; set; }
		public virtual int AverageSoldPrice1Year { get; set; }
		public virtual int AverageSoldPrice3Year { get; set; }
		public virtual int AverageSoldPrice5Year { get; set; }
		public virtual int AverageSoldPrice7Year { get; set; }
		public virtual int NumerOfSales1Year { get; set; }
		public virtual int NumerOfSales3Year { get; set; }
		public virtual int NumerOfSales5Year { get; set; }
		public virtual int NumerOfSales7Year { get; set; }
		public virtual double TurnOver { get; set; }
		public virtual string PricesUrl { get; set; }
		public virtual string AverageValuesGraphUrl { get; set; }
		public virtual string ValueRangesGraphUrl { get; set; }
		public virtual string ValueTrendGraphUrl { get; set; }
		public virtual string HomeValuesGraphUrl { get; set; }
		public virtual string ZooplaEstimate { get; set; }
		public virtual int ZooplaEstimateValue { get; set; }
		public virtual DateTime? UpdateDate { get; set; }
	}
}

namespace EZBob.DatabaseLib.Model.DataMapping
{
	using Database;
	using FluentNHibernate.Mapping;
	using NHibernate.Type;

	public class ZooplaMap : ClassMap<Zoopla>
	{
		public ZooplaMap()
		{
			Table("Zoopla");
			Id(x => (object)x.Id);
			References(x => x.CustomerAddress, "CustomerAddressId");
			Map(x => x.AreaName).Length(10);
			Map(x => (object)x.AverageSoldPrice1Year);
			Map(x => (object)x.AverageSoldPrice3Year);
			Map(x => (object)x.AverageSoldPrice5Year);
			Map(x => (object)x.AverageSoldPrice7Year);
			Map(x => (object)x.NumerOfSales1Year);
			Map(x => (object)x.NumerOfSales3Year);
			Map(x => (object)x.NumerOfSales5Year);
			Map(x => (object)x.NumerOfSales7Year);
			Map(x => (object)x.TurnOver);
			Map(x => x.PricesUrl).Length(100);
			Map(x => x.AverageValuesGraphUrl).Length(100);
			Map(x => x.ValueRangesGraphUrl).Length(100);
			Map(x => x.ValueTrendGraphUrl).Length(100);
			Map(x => x.HomeValuesGraphUrl).Length(100);
			Map(x => x.ZooplaEstimate).Length(30);
			Map(x => x.ZooplaEstimateValue);
			Map(x => x.UpdateDate).CustomType<UtcDateTimeType>();
		}
	}
}

namespace EZBob.DatabaseLib.Model.Database.Repository
{
	using System.Linq;
	using ApplicationMng.Repository;
	using Database;
	using NHibernate;
	using NHibernate.Linq;

	public interface IZooplaRepository : IRepository<Zoopla>
	{
		bool ExistsByAddress(CustomerAddress address);
		Zoopla GetByAddress(CustomerAddress address);
	}

	public class ZooplaRepository : NHibernateRepositoryBase<Zoopla>, IZooplaRepository
	{
		public ZooplaRepository(ISession session)
			: base(session)
		{
		}
		public bool ExistsByAddress(CustomerAddress address)
		{
			return Session.Query<Zoopla>().Any(z => z.CustomerAddress == address);
		}
		public Zoopla GetByAddress(CustomerAddress address)
		{
			return Session.Query<Zoopla>().FirstOrDefault(z => z.CustomerAddress == address);
		}
	}
}
