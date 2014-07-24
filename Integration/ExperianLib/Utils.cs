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
	using ServiceClientProxy;
	using ServiceClientProxy.EzServiceReference;
	using StructureMap;
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
			var pkg = new WriteToLogPackage(input, output, type, customerId, directorId);
			WriteLog(pkg);
			return pkg.Out.ServiceLog;
		} // WriteToLog

		public static void WriteLog(WriteToLogPackage oPackage) {
			if (oPackage == null)
				throw new ArgumentNullException("oPackage", "Cannot save to MP_ServiceLog: no package specified.");

			var oRetryer = new SqlRetryer(oLog: ms_oLog);

			oPackage.Out.ServiceLog = new MP_ServiceLog {
				InsertDate = DateTime.Now,
				RequestData = oPackage.In.Request,
				ResponseData = oPackage.In.Response,
				ServiceType = oPackage.In.ServiceType.DescriptionAttr(),
			};

			try {
				oRetryer.Retry(() => {
					var customerRepo = ObjectFactory.GetInstance<NHibernateRepositoryBase<Customer>>();
					oPackage.Out.ServiceLog.Customer = customerRepo.Get(oPackage.In.CustomerID);
				});

				if (oPackage.In.DirectorID != null) {
					oRetryer.Retry(() => {
						var directorRepo = ObjectFactory.GetInstance<NHibernateRepositoryBase<Director>>();
						oPackage.Out.ServiceLog.Director = directorRepo.Get(oPackage.In.DirectorID);
					});
				} // if

				ms_oLog.Debug("Input data was: {0}", oPackage.Out.ServiceLog.RequestData);
				ms_oLog.Debug("Output data was: {0}", oPackage.Out.ServiceLog.ResponseData);

				oRetryer.Retry(() => {
					var repoLog = ObjectFactory.GetInstance<NHibernateRepositoryBase<MP_ServiceLog>>();
					repoLog.SaveOrUpdate(oPackage.Out.ServiceLog);
				});

				if (oPackage.In.ServiceType == ExperianServiceType.LimitedData)
					oPackage.Out.ExperianLtd = ObjectFactory.GetInstance<IEzServiceAccessor>().ParseExperianLtd(oPackage.Out.ServiceLog.Id);

				try {
					var historyRepo = ObjectFactory.GetInstance<ExperianHistoryRepository>();

					if (historyRepo.HasHistory(oPackage.Out.ServiceLog.Customer, oPackage.In.ServiceType)) {
						var history = new MP_ExperianHistory {
							Customer = oPackage.Out.ServiceLog.Customer,
							ServiceLogId = oPackage.Out.ServiceLog.Id,
							Date = oPackage.Out.ServiceLog.InsertDate,
							Type = oPackage.Out.ServiceLog.ServiceType,
						};

						switch (oPackage.In.ServiceType) {
						case ExperianServiceType.Consumer:
							history.Score = GetScoreFromXml(oPackage.Out.ServiceLog.ResponseData);
							history.CII = GetCIIFromXml(oPackage.Out.ServiceLog.ResponseData);
							history.CaisBalance = GetConsumerCaisBalance(oPackage.Out.ServiceLog.ResponseData);
							historyRepo.SaveOrUpdate(history);
							break;
						case ExperianServiceType.LimitedData:
							history.Score = (oPackage.Out.ExperianLtd == null) ? -1 : (oPackage.Out.ExperianLtd.CommercialDelphiScore ?? -1);
							history.CaisBalance = GetLimitedCaisBalance(oPackage.Out.ExperianLtd);
							historyRepo.SaveOrUpdate(history);
							break;
						case ExperianServiceType.NonLimitedData:
							var serviceClient = new ServiceClient();
							CompanyDataForCreditBureauActionResult notLimitedBusinessData = serviceClient.Instance.GetCompanyDataForCreditBureau(0/*should be refactored*/, oPackage.Out.ServiceLog.Customer.Id, oPackage.Out.ServiceLog.Customer.Company.ExperianRefNum);
							history.Score = notLimitedBusinessData != null ? notLimitedBusinessData.Score : 0;
							historyRepo.SaveOrUpdate(history);
							break;
						} // switch
					} // if
				}
				catch (Exception ex) {
					ms_oLog.Warn(ex, "Failed to save Experian history.");
				} // try
			}
			catch (Exception e) {
				ms_oLog.Error(e,
					"Failed to save a '{0}' entry for customer id {1} into MP_ServiceLog.",
					oPackage.In.ServiceType, oPackage.In.CustomerID 
				);
			} // try
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

		#endregion public

		#region private

		private static readonly ASafeLog ms_oLog = new SafeILog(typeof(Utils));

		#endregion private
	} // class Utils
} // namespace
