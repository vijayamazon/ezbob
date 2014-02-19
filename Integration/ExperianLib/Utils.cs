namespace ExperianLib {
	using System;
	using System.Text;
	using ApplicationMng.Repository;
	using EZBob.DatabaseLib.Model.Database;
	using Ezbob.Database;
	using Ezbob.Logger;
	using StructureMap;
	using log4net;

	public class Utils {
		#region public

		#region generic method WriteLog

		public static MP_ServiceLog WriteLog<TX, TY>(
			TX input,
			TY output,
			string type,
			int customerId,
			int? directorId = null
		)
			where TX : class
			where TY : class
		{
			return WriteLog(XSerializer.Serialize(input), XSerializer.Serialize(output), type, customerId, directorId);
		} // WriteLog

		#endregion generic method WriteLog

		#region method WriteLog

		public static MP_ServiceLog WriteLog(string input, string output, string type, int customerId, int? directorId = null) {
			var oRetryer = new SqlRetryer(oLog: new SafeILog(Log));

			var logEntry = new MP_ServiceLog {
				InsertDate = DateTime.Now,
				RequestData = input,
				ResponseData = output,
				ServiceType = type,
			};

			try {
				oRetryer.Retry(() => {
					var customerRepo = ObjectFactory.GetInstance<NHibernateRepositoryBase<Customer>>();
					logEntry.Customer = customerRepo.Get(customerId);
				});

				if (directorId != null) {
					oRetryer.Retry(() => {
						var directorRepo = ObjectFactory.GetInstance<NHibernateRepositoryBase<Director>>();
						logEntry.Director = directorRepo.Get(directorId);
					});
				} // if

				Log.DebugFormat("Input data was: {0}", logEntry.RequestData);
				Log.DebugFormat("Output data was: {0}", logEntry.ResponseData);

				oRetryer.Retry(() => {
					var repoLog = ObjectFactory.GetInstance<NHibernateRepositoryBase<MP_ServiceLog>>();
					repoLog.SaveOrUpdate(logEntry);
				});
			}
			catch (Exception e) {
				Log.Error("Failed to save a '" + type + "' entry for customer id " + customerId + " into MP_ServiceLog.", e);
			} // try

			return logEntry;
		} // WriteLog

		#endregion method WriteLog

		#region method TryRead

		public static void TryRead(Action a, string key, StringBuilder errors) {
			try {
				a();
			}
			catch(Exception e) {
				errors.AppendFormat("Can't read value for {0} because of exception: {1}", key, e.Message);
			} // try
		} // TryRead

		#endregion method TryRead

		#endregion public

		#region private

		private static readonly ILog Log = LogManager.GetLogger(typeof(Utils));

		#endregion private
	} // class Utils
} // namespace
