using System;
using System.Linq;
using ApplicationMng.Repository;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Experian;
using NHibernate;
using Newtonsoft.Json;
using log4net;

namespace EZBob.DatabaseLib.Repository
{
	public class ExperianBankCacheRepository : NHibernateRepositoryBase<MP_ExperianBankCache>
	{
        static readonly ILog Log = LogManager.GetLogger(typeof(ExperianBankCacheRepository));

        public ExperianBankCacheRepository(ISession session): base(session)
		{
		}

	    //-----------------------------------------------------------------------------------
        public T Get<T>(string key, long? expirationPeriodSec) where T: class
        {
            Log.InfoFormat("Getting data from MP_ExperianBankCache for key '{0}'...", key);
            var item = (from i in GetAll() where i.Key == key select i).FirstOrDefault();
            if (item == null) return null;
            if (expirationPeriodSec != null)
            {
                if ((DateTime.Now - item.LastUpdateDate).TotalSeconds > expirationPeriodSec) return null;
            }
            return JsonConvert.DeserializeObject<T>(item.Data);
        }

	    //-----------------------------------------------------------------------------------
        public void Set<T>(string key, T value, MP_ServiceLog serviceLog) where T : class
        {
            Log.InfoFormat("Setting data to MP_ExperianBankCache for key '{0}', service log id={1}...", key, serviceLog.Id);
            var item = (from i in GetAll() where i.Key == key select i).FirstOrDefault() ?? new MP_ExperianBankCache();
            item.Key = key;
            item.LastUpdateDate = DateTime.Now;
            item.Data = JsonConvert.SerializeObject(value);
            item.LogItem = serviceLog;
            SaveOrUpdate(item);
        }

	}
}