namespace EZBob.DatabaseLib.Repository {
	using System;
	using System.Linq;
	using ApplicationMng.Repository;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Experian;
	using Ezbob.Database;
	using Ezbob.Logger;
	using NHibernate;
	using Newtonsoft.Json;
	using log4net;

	public class ExperianBankCacheRepository : NHibernateRepositoryBase<MP_ExperianBankCache> {
		public ExperianBankCacheRepository(ISession session) : base(session) {
			m_oRetryer = new SqlRetryer(log: new SafeILog(Log));
		} // constructor

		public virtual T Get<T>(string key, long? expirationPeriodSec) where T : class {
			return m_oRetryer.Retry(() => {
				Log.InfoFormat("Getting data from MP_ExperianBankCache for key '{0}'...", key);

				var item = GetAll().FirstOrDefault(i => i.Key == key);

				if (item == null)
					return null;

				if (expirationPeriodSec != null)
					if ((DateTime.UtcNow - item.LastUpdateDate).TotalSeconds > expirationPeriodSec)
						return null;

				return JsonConvert.DeserializeObject<T>(item.Data);
			}, "ExperianBankCacheRepository.Get(" + key + ")");
		} // Get

		public virtual void Set<T>(string key, T value, MP_ServiceLog serviceLog) where T : class {
			m_oRetryer.Retry(() => {
				Log.InfoFormat("Setting data to MP_ExperianBankCache for key '{0}', service log id={1}...", key, serviceLog.Id);

				var item = GetAll().FirstOrDefault(i => i.Key == key) ?? new MP_ExperianBankCache();

				item.Key = key;
				item.LastUpdateDate = DateTime.UtcNow;
				item.Data = JsonConvert.SerializeObject(value);
				item.LogItem = serviceLog;

				SaveOrUpdate(item);
			}, "ExperianBankCacheRepository.Set(" + key + ")");
		} // Set

		static readonly ILog Log = LogManager.GetLogger(typeof(ExperianBankCacheRepository));
		private readonly SqlRetryer m_oRetryer;
	} // class ExperianBankCacheRepository
} // namespace
