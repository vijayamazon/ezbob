using System;
using FluentNHibernate.Mapping;

namespace EZBob.DatabaseLib.Model.Database
{
    public class MP_ExperianBankCache
	{
		public virtual long Id { get; set; }
		public virtual string Key { get; set; }
		public virtual string Data { get; set; }
		public virtual MP_ServiceLog LogItem { get; set; }
		public virtual DateTime LastUpdateDate { get; set; }
	}

    //-----------------------------------------------------------------------------------
    public sealed class MP_ExperianBankCacheMap : ClassMap<MP_ExperianBankCache>
    {
        public MP_ExperianBankCacheMap()
        {
            Table("MP_ExperianBankCache");
            Id(x => x.Id);
            Map(x => x.Key, "KeyData");
            Map(x => x.Data).LazyLoad();
            Map(x => x.LastUpdateDate);
            References(x => x.LogItem, "ServiceLogId").LazyLoad();
        }
    }
}