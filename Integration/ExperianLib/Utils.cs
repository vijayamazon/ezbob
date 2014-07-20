namespace ExperianLib {
	using System;
	using System.IO;
	using System.Text;
	using System.Xml;
	using System.Xml.Linq;
	using System.Xml.XPath;
	using ApplicationMng.Repository;
	using EZBob.DatabaseLib.Model.Database;
	using EzServiceAccessor;
	using Ezbob.Backend.ModelsWithDB.Experian;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.Extensions;
	using Parser;
	using StructureMap;
	using log4net;
	using EZBob.DatabaseLib.Model.Experian;

	public class Utils {
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

		public static MP_ServiceLog WriteLog(string input, string output, ExperianServiceType type, int customerId, int? directorId = null) {
			var oRetryer = new SqlRetryer(oLog: new SafeILog(Log));

			var logEntry = new MP_ServiceLog {
				InsertDate = DateTime.Now,
				RequestData = input,
				ResponseData = output,
				ServiceType = type.DescriptionAttr(),
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

				ExperianLtd oExperianLtd = null;

				if (type == ExperianServiceType.LimitedData)
					oExperianLtd = ObjectFactory.GetInstance<IEzServiceAccessor>().ParseExperianLtd(logEntry.Id);

				try {
					var historyRepo = ObjectFactory.GetInstance<ExperianHistoryRepository>();

					if (historyRepo.HasHistory(logEntry.Customer, type)) {
						var history = new MP_ExperianHistory {
							Customer = logEntry.Customer,
							ServiceLogId = logEntry.Id,
							Date = logEntry.InsertDate,
							Type = logEntry.ServiceType,
						};

						switch (type) {
						case ExperianServiceType.Consumer:
							history.Score = GetScoreFromXml(logEntry.ResponseData);
							history.CII = GetCIIFromXml(logEntry.ResponseData);
							history.CaisBalance = GetConsumerCaisBalance(logEntry.ResponseData);
							historyRepo.SaveOrUpdate(history);
							break;
						case ExperianServiceType.LimitedData:
							history.Score = (oExperianLtd == null) ? -1 : (oExperianLtd.CommercialDelphiScore ?? -1);
							history.CaisBalance = GetLimitedCaisBalance(oExperianLtd);
							historyRepo.SaveOrUpdate(history);
							break;
						case ExperianServiceType.NonLimitedData:
							history.Score = GetNonLimitedScoreFromXml(logEntry.ResponseData);
							historyRepo.SaveOrUpdate(history);
							break;
						}
					}
				}
				catch (Exception ex) {
					Log.WarnFormat("Failed to save experian history \n{0}", ex);
				}
			}
			catch (Exception e) {
				Log.Error("Failed to save a '" + type + "' entry for customer id " + customerId + " into MP_ServiceLog.", e);
			} // try

			return logEntry;
		} // WriteToLog

		#endregion method WriteLog

		#region method TryRead

		public static void TryRead(Action a, string key, StringBuilder errors) {
			try {
				a();
			}
			catch (Exception e) {
				errors.AppendFormat("Can't read value for {0} because of exception: {1}", key, e.Message);
			} // try
		} // TryRead

		#endregion method TryRead

		#endregion public

		#region private

		private static readonly ILog Log = LogManager.GetLogger(typeof(Utils));

		public static decimal? GetConsumerCaisBalance(string xml) {
			var xmlDoc = new XmlDocument();

			var stream = new MemoryStream();
			var writer = new StreamWriter(stream);
			writer.Write(xml.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>", "<?xml version=\"1.0\" encoding=\"utf-8\"?>"));
			writer.Flush();
			stream.Position = 0;
			xmlDoc.Load(stream);

			XmlNodeList caisDetailsList = xmlDoc.SelectNodes("//CAISDetails");
			if (caisDetailsList != null) {

				decimal balance = 0;

				foreach (XmlElement currentCaisDetails in caisDetailsList) {
					try {
						FinancialAccount financialAccount = FinancialAccountsParser.HandleOneCaisDetailsBlock(currentCaisDetails);
						if (financialAccount.AccountStatus != "Settled") {
							balance += financialAccount.Balance.HasValue ? financialAccount.Balance.Value : 0;
						}
					}
					catch (Exception ex) {

					}
				}
				return balance;
			}
			return null;
		}

		public static int GetScoreFromXml(String xml) {
			try {
				var doc = XDocument.Parse(xml);
				var score = doc.XPathSelectElement("//PremiumValueData/Scoring/E5S051").Value;
				return Convert.ToInt32(score);
			}
			catch (Exception) {
				return -1;
			}
		}

		public static int GetCIIFromXml(string xml) {
			try {
				var doc = XDocument.Parse(xml);
				var cii = doc.XPathSelectElement("//PremiumValueData/CII/NDSPCII").Value;
				return Convert.ToInt32(cii);
			}
			catch (Exception) {
				return -1;
			}
		}

		public static int GetNonLimitedScoreFromXml(string xml) {
			try {
				var doc = XDocument.Parse(xml);
				var score = doc.XPathSelectElement("//REQUEST/DN40/RISKSCORE").Value;
				return Convert.ToInt32(score);
			}
			catch (Exception ex) {
				Log.WarnFormat("Failed to retrieve nonlimited score from xml {0}", ex);
				return -1;
			}
		}

		#region method GetLimitedCaisBalance

		public static decimal? GetLimitedCaisBalance(ExperianLtd oExperianLtd) {
			if (oExperianLtd == null)
				return null;

			int nFoundCount = 0;
			decimal balance = 0;

			foreach (var oRow in oExperianLtd.Children) {
				if (oRow.GetType() != typeof(ExperianLtdDL97))
					continue;

				nFoundCount++;

				var dl97 = (ExperianLtdDL97)oRow;

				if ((dl97.AccountState != null) && (dl97.AccountState != "S"))
					balance += dl97.CurrentBalance ?? 0;
			} // for each

			return nFoundCount == 0 ? (decimal?)null : balance;
		} // GetLimitedCaisBalance

		#endregion method GetLimitedCaisBalance

		#endregion private
	} // class Utils
} // namespace
