using System;
using FluentNHibernate.Mapping;

namespace EZBob.DatabaseLib.Model.Database
{
	public class CurrencyData
	{
	    public CurrencyData()
	    {
	    }

	    public CurrencyData(string name)
		{
			Name = name;
		}

		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
		public virtual decimal Price { get; set; }
		public virtual DateTime? LastUpdated { get; set; }
	}

//-----------------------------------------------------------------------------------------------------

	public sealed class CurrencyMap : ClassMap<CurrencyData>
	{
		public CurrencyMap()
		{
			Table("MP_Currency");
			Id(x => x.Id).GeneratedBy.Native();
			Map(x => x.Name).Length(50);
			Map(x => x.LastUpdated);
			Map( x => x.Price );
		    Cache.ReadWrite().Region("Longterm");
		}
	}
}