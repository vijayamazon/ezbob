namespace ExperianLib
{
	using System;
	using System.IO;
	using System.Text;
	using System.Xml;
	using System.Xml.Linq;
	using System.Xml.XPath;
	using ApplicationMng.Repository;
	using EZBob.DatabaseLib.Model.Database;
	using EzServiceAccessor;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.Extensions;
	using Parser;
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

				if (type == ExperianServiceType.LimitedData)
					ObjectFactory.GetInstance<IEzServiceAccessor>().ParseExperianLtd(logEntry.Id);

				try
				{
					var historyRepo = ObjectFactory.GetInstance<ExperianHistoryRepository>();
					if (historyRepo.HasHistory(logEntry.Customer, type))
					{
						var history = new MP_ExperianHistory
							{
								Customer = logEntry.Customer,
								ServiceLogId = logEntry.Id,
								Date = logEntry.InsertDate,
								Type = logEntry.ServiceType,
							};

						switch (type)
						{
							case ExperianServiceType.Consumer:
								history.Score = GetScoreFromXml(logEntry.ResponseData);
								history.CII = GetCIIFromXml(logEntry.ResponseData);
								history.CaisBalance = GetConsumerCaisBalance(logEntry.ResponseData);
								historyRepo.SaveOrUpdate(history);
								break;
							case ExperianServiceType.LimitedData:
								history.Score = GetLimitedScoreFromXml(logEntry.ResponseData);
								history.CaisBalance = GetLimitedCaisBalance(logEntry.ResponseData);
								historyRepo.SaveOrUpdate(history);
								break;
							case ExperianServiceType.NonLimitedData:
								history.Score = GetNonLimitedScoreFromXml(logEntry.ResponseData);
								historyRepo.SaveOrUpdate(history);
								break;
						}
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
		}


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

		public static decimal? GetConsumerCaisBalance(string xml)
		{
			var xmlDoc = new XmlDocument();

			var stream = new MemoryStream();
			var writer = new StreamWriter(stream);
			writer.Write(xml.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>", "<?xml version=\"1.0\" encoding=\"utf-8\"?>"));
			writer.Flush();
			stream.Position = 0;
			xmlDoc.Load(stream);

			XmlNodeList caisDetailsList = xmlDoc.SelectNodes("//CAISDetails");
			if (caisDetailsList != null)
			{

				decimal balance = 0;

				foreach (XmlElement currentCaisDetails in caisDetailsList)
				{
					try
					{
						FinancialAccount financialAccount = FinancialAccountsParser.HandleOneCaisDetailsBlock(currentCaisDetails);
						if (financialAccount.AccountStatus != "Settled")
						{
							balance += financialAccount.Balance.HasValue ? financialAccount.Balance.Value : 0;
						}
					}
					catch (Exception ex)
					{

					}
				}
				return balance;
			}
			return null;
		}

		public static int GetScoreFromXml(String xml)
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

		public static int GetCIIFromXml(string xml)
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

		public static int GetNonLimitedScoreFromXml(string xml)
		{
			try
			{
				var doc = XDocument.Parse(xml);
				var score = doc.XPathSelectElement("//REQUEST/DN40/RISKSCORE").Value;
				return Convert.ToInt32(score);
			}
			catch (Exception ex)
			{
				Log.WarnFormat("Failed to retrieve nonlimited score from xml {0}", ex);
				return -1;
			}
		}

		public static int GetLimitedScoreFromXml(string xml)
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

		public static decimal? GetLimitedCaisBalance(string xml)
		{
			var xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(xml);
			XmlNodeList dl97List = xmlDoc.SelectNodes("//DL97");

			if (dl97List != null)
			{
				decimal balance = 0;
				foreach (XmlElement dl97 in dl97List)
				{
					XmlNode stateNode = dl97.SelectSingleNode("ACCTSTATE");
					XmlNode currentBalanceNode = dl97.SelectSingleNode("CURRBALANCE");
					if (stateNode != null && stateNode.InnerText != "S")
					{
						int currBalance;
						if (currentBalanceNode != null && int.TryParse(currentBalanceNode.InnerText, out currBalance))
						{
							balance += currBalance;
						}
					}
				}
				return balance;
			}
			return null;
		}

		#endregion private
	} // class Utils
} // namespace
