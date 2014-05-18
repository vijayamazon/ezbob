namespace ExperianLib
{
	using System;
	using System.Text;
	using System.Xml.Linq;
	using System.Xml.XPath;
	using ApplicationMng.Repository;
	using EZBob.DatabaseLib.Model.Database;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.Extensions;
	using StructureMap;
	using log4net;
	using EZBob.DatabaseLib.Model.Experian;


	public class Utils
	{
		#region public

		#region generic method WriteLog

		public static MP_ServiceLog WriteLog<TX, TY>(
			TX input,
			TY output,
			ExperianServiceType type,
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

		public static MP_ServiceLog WriteLog(string input, string output, ExperianServiceType type, int customerId, int? directorId = null)
		{
			var oRetryer = new SqlRetryer(oLog: new SafeILog(Log));

			var logEntry = new MP_ServiceLog
			{
				InsertDate = DateTime.Now,
				RequestData = input,
				ResponseData = output,
				ServiceType = type.DescriptionAttr(),
			};

			try
			{
				oRetryer.Retry(() =>
				{
					var customerRepo = ObjectFactory.GetInstance<NHibernateRepositoryBase<Customer>>();
					logEntry.Customer = customerRepo.Get(customerId);
				});

				if (directorId != null)
				{
					oRetryer.Retry(() =>
					{
						var directorRepo = ObjectFactory.GetInstance<NHibernateRepositoryBase<Director>>();
						logEntry.Director = directorRepo.Get(directorId);
					});
				} // if

				Log.DebugFormat("Input data was: {0}", logEntry.RequestData);
				Log.DebugFormat("Output data was: {0}", logEntry.ResponseData);

				oRetryer.Retry(() =>
				{
					var repoLog = ObjectFactory.GetInstance<NHibernateRepositoryBase<MP_ServiceLog>>();
					repoLog.SaveOrUpdate(logEntry);
				});


				try
				{
					var history = new MP_ExperianHistory
						{
							Customer = logEntry.Customer,
							ServiceLogId = logEntry.Id,
							Date = logEntry.InsertDate,
							Type = logEntry.ServiceType,
						};
					var historyRepo = ObjectFactory.GetInstance<NHibernateRepositoryBase<MP_ExperianHistory>>();
					switch (type)
					{
						case ExperianServiceType.Consumer:
							history.Score = GetScoreFromXml(logEntry.ResponseData);
							history.CII = GetCIIFromXml(logEntry.ResponseData);
							historyRepo.SaveOrUpdate(history);
							break;
						case ExperianServiceType.LimitedData:
							history.Score = GetLimitedScoreFromXml(logEntry.ResponseData);
							historyRepo.SaveOrUpdate(history);
							break;
						case ExperianServiceType.NonLimitedData:
							history.Score = GetNonLimitedScoreFromXml(logEntry.ResponseData);
							historyRepo.SaveOrUpdate(history);
							break;
					}
				}
				catch (Exception ex)
				{
					Log.WarnFormat("Failed to save experian history \n{0}", ex);
				}
			}
			catch (Exception e)
			{
				Log.Error("Failed to save a '" + type + "' entry for customer id " + customerId + " into MP_ServiceLog.", e);
			} // try

			return logEntry;
		} // WriteLog

		#endregion method WriteLog

		#region method TryRead

		public static void TryRead(Action a, string key, StringBuilder errors)
		{
			try
			{
				a();
			}
			catch (Exception e)
			{
				errors.AppendFormat("Can't read value for {0} because of exception: {1}", key, e.Message);
			} // try
		} // TryRead

		#endregion method TryRead

		#endregion public

		#region private

		private static readonly ILog Log = LogManager.GetLogger(typeof(Utils));

		private static int GetScoreFromXml(String xml)
		{
			try
			{
				var doc = XDocument.Parse(xml);
				var score = doc.XPathSelectElement("//PremiumValueData/Scoring/E5S051").Value;
				return Convert.ToInt32(score);
			}
			catch (Exception)
			{
				return -1;
			}
		}

		private static int GetCIIFromXml(string xml)
		{
			try
			{
				var doc = XDocument.Parse(xml);
				var cii = doc.XPathSelectElement("//PremiumValueData/CII/NDSPCII").Value;
				return Convert.ToInt32(cii);
			}
			catch (Exception)
			{
				return -1;
			}
		}

		private static int GetNonLimitedScoreFromXml(string xml)
		{
			try
			{
				var doc = XDocument.Parse(xml);
				var score = doc.XPathSelectElement("//REQUEST/DN73/NLCDSCORE").Value;
				return Convert.ToInt32(score);
			}
			catch (Exception ex)
			{
				Log.WarnFormat("Failed to retrieve nonlimited score from xml {0}", ex);
				return -1;
			}
		}

		private static int GetLimitedScoreFromXml(string xml)
		{
			try
			{
				var doc = XDocument.Parse(xml);
				var score = doc.XPathSelectElement("//REQUEST/DL76/RISKSCORE").Value;
				return Convert.ToInt32(score);
			}
			catch (Exception ex)
			{
				Log.WarnFormat("Failed to retrieve limited score from xml {0}", ex);
				return -1;
			}
		}

		#endregion private
	} // class Utils
} // namespace
