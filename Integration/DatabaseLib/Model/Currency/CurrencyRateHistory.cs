using System;
using FluentNHibernate.Mapping;

namespace EZBob.DatabaseLib.Model.Database
{
	public class CurrencyRateHistory
	{
		public virtual int Id { get; set; }
		public virtual CurrencyData CurrencyData { get; set; }
		public virtual decimal? Price { get; set; }
		public virtual DateTime? Updated { get; set; }
	}

//-----------------------------------------------------------------------------------------------------

	public class CurrencyRateHistoryMap : ClassMap<CurrencyRateHistory>
	{
		public CurrencyRateHistoryMap()
		{
			Table( "MP_CurrencyRateHistory" );
			Id(x => x.Id).GeneratedBy.HiLo("1000");
			References( x => x.CurrencyData, "CurrencyId" );
			Map( x => x.Price );
			Map( x => x.Updated );
            Cache.ReadWrite().Region("Longterm");
		}
	}
}