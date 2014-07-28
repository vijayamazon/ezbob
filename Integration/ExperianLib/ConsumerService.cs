namespace ExperianLib {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Xml.Serialization;
	using ApplicationMng.Repository;
	using ConfigManager;
	using EZBob.DatabaseLib.Model;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EZBob.DatabaseLib.Model.Experian;
	using Dictionaries;
	using EzBobIntegration.Web_References.Consumer;
	using Ezbob.Backend.ModelsWithDB.Experian;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Newtonsoft.Json;
	using Parser;
	using StructureMap;
	using log4net;
	using EZBob.DatabaseLib.Repository;

	public class ConsumerService {
		#region public

		#region method ShiftLocation

		public static InputLocationDetailsMultiLineLocation ShiftLocation(InputLocationDetailsMultiLineLocation mlLocation) {
			if (mlLocation == null)
				return null;

			var oSrcLines = new List<string> {
				mlLocation.LocationLine1,
				mlLocation.LocationLine2,
				mlLocation.LocationLine3,
				mlLocation.LocationLine4,
				mlLocation.LocationLine5,
				mlLocation.LocationLine6,
			};

			var oDstLines = new List<string>();

			// If one of lines is "Flat" (without numbers) we got an error 'Location not resolved'.

			foreach (string sLine in oSrcLines)
				if (!string.IsNullOrEmpty(sLine) && sLine.ToLower() != "flat")
					oDstLines.Add(sLine);

			while (oDstLines.Count < 6)
				oDstLines.Add(null);

			string[] lines = oDstLines.ToArray();

			mlLocation = new InputLocationDetailsMultiLineLocation {
				LocationLine1 = lines[0],
				LocationLine2 = lines[1],
				LocationLine3 = lines[2],
				LocationLine4 = lines[3],
				LocationLine5 = lines[4],
				LocationLine6 = lines[5],
			};

			return mlLocation;
		} // ShiftLocation

		#endregion method ShiftLocation

		#region constructor

		public ConsumerService() {
			_repo = ObjectFactory.GetInstance<ExperianDataCacheRepository>();
			m_oRetryer = new SqlRetryer(oLog: new SafeILog(Log));

			interactiveMode = CurrentValues.Instance.ExperianInteractiveMode;
		} // constructor

		#endregion constructor

		#region GetConsumerInfo

		public ConsumerServiceResult GetConsumerInfo(
			string firstName,
			string surname,
			string gender,
			DateTime? birthDate,
			InputLocationDetailsUKLocation ukLocation,
			InputLocationDetailsMultiLineLocation mlLocation,
			string applicationType,
			int customerId,
			int directorId,
			bool checkInCacheOnly,
			bool isDirector,
			bool forceCheck
		) {
			try {
				string postcode = GetPostcode(ukLocation, mlLocation);

				// debug mode
				if (surname.StartsWith("TestSurnameDebugMode") || surname == "TestSurnameOne" || surname == "TestSurnameFile")
				{
					var data = ConsumerDebugResult(surname, birthDate, customerId, checkInCacheOnly);
					MP_ExperianDataCache experianDataCacheEntry = _repo.GetAll()
						.FirstOrDefault(x => x.CustomerId == customerId && x.DirectorId == directorId && x.CompanyRefNumber == null);

					if (experianDataCacheEntry == null) {
						var newExperianDataCacheEntry = new MP_ExperianDataCache {
							Name = firstName,
							Surname = surname,
							BirthDate = birthDate,
							PostCode = postcode,
							LastUpdateDate = DateTime.UtcNow,
							CustomerId = customerId,
							DirectorId = directorId,
							JsonPacket = JsonConvert.SerializeObject(data.Output)
						};

						_repo.SaveOrUpdate(newExperianDataCacheEntry);
					} // if
					else if (string.IsNullOrEmpty(experianDataCacheEntry.JsonPacket))
					{
						experianDataCacheEntry.JsonPacket = JsonConvert.SerializeObject(data.Output);
						_repo.SaveOrUpdate(experianDataCacheEntry);
					}

					return data;
				} // if test

				mlLocation = ShiftLocation(mlLocation);

				Log.InfoFormat("GetConsumerInfo: checking cache for {2} id {3} firstName: {0}, surname: {1} birthday: {4}, postcode: {5}", firstName, surname, isDirector ? "director" : "customer", isDirector ? directorId : customerId, birthDate, postcode);
				
				MP_ExperianDataCache cachedResponse = isDirector
					? _repo.GetDirectorFromCache(directorId, firstName, surname, birthDate, postcode)
					: _repo.GetCustomerFromCache(customerId, firstName, surname, birthDate, postcode);

				if (!forceCheck) {
					if (cachedResponse != null) {
						if (CacheNotExpired(cachedResponse) || checkInCacheOnly)
							return ParseCache(cachedResponse);
					}
					else if (checkInCacheOnly)
						return null;
				} // if

				cachedResponse = cachedResponse ?? new MP_ExperianDataCache {
					Name = firstName,
					Surname = surname,
					BirthDate = birthDate,
					PostCode = postcode
				};

				return GetServiceOutput(gender, ukLocation, mlLocation, applicationType, customerId, directorId, cachedResponse);
			}
			catch (Exception ex) {
				Log.Error(ex);
				return new ConsumerServiceResult
					{
						Data = new ExperianConsumerData
							{
								HasExperianError = true, 
								Error = "Exception: " + ex.Message
							}
					};
			} // try
		} // GetConsumerInfo

		#endregion GetConsumerInfo

		#endregion public

		#region private

		#region method SaveDefaultAccountIntoDb

		private void SaveDefaultAccountIntoDb(OutputRoot output, int customerId, MP_ServiceLog serviceLog) {
			var customerRepo = ObjectFactory.GetInstance<CustomerRepository>();

			var customer = customerRepo.Get(customerId);

			OutputFullConsumerDataConsumerDataCAIS[] cais = null;

			TryRead(() => cais = output.Output.FullConsumerData.ConsumerData.CAIS);

			if (cais == null)
				return;

			var dateAdded = DateTime.UtcNow;
			var repo = ObjectFactory.GetInstance<NHibernateRepositoryBase<ExperianDefaultAccount>>();

			foreach (var caisData in cais) {
				if (caisData.CAISDetails == null)
				{
					continue;
				}
				foreach (var detail in caisData.CAISDetails.Where(detail => detail.AccountStatus == "F")) {
					int relevantYear, relevantMonth, relevantDay;

					if (detail.SettlementDate != null) {
						relevantYear = detail.SettlementDate.CCYY;
						relevantMonth = detail.SettlementDate.MM;
						relevantDay = detail.SettlementDate.DD;
					}
					else {
						relevantYear = detail.LastUpdatedDate.CCYY;
						relevantMonth = detail.LastUpdatedDate.MM;
						relevantDay = detail.LastUpdatedDate.DD;
					} // if

					var settlementDate = new DateTime(relevantYear, relevantMonth, relevantDay);
					var currentDefBalance = 0;
					var balance = 0;
					var reg = new Regex("[^0-9,]");
					var tempDetail = detail;

					TryRead(() =>
						int.TryParse(reg.Replace(tempDetail.CurrentDefBalance.Amount, ""), out currentDefBalance)
					);

					TryRead(() => int.TryParse(reg.Replace(tempDetail.Balance.Amount, ""), out balance));

					var eda = new ExperianDefaultAccount {
						AccountType = AccountTypeDictionary.GetAccountType(detail.AccountType ?? string.Empty),
						Date = settlementDate,
						DelinquencyType = "Default",
						Customer = customer,
						DateAdded = dateAdded,
						CurrentDefBalance = currentDefBalance,
						Balance = balance,
						ServiceLog = serviceLog,
					};

					m_oRetryer.Retry(() => repo.SaveOrUpdate(eda));
				} // foreach detail in cais datum
			} // foreach cais datum in cais data
		} // SaveDefaultAccountIntoDb

		#endregion method SaveDefaultAccountIntoDb

		#region method CreateConsumerServiceResult

		private ConsumerServiceResult CreateConsumerServiceResult(
			string surname,
			int customerId,
			bool checkInCacheOnly,
			string content
		) {
			var outputRootSerializer = new XmlSerializer(typeof(OutputRoot));
			var outputRoot = (OutputRoot)outputRootSerializer.Deserialize(new StringReader(content));

			var consumerServiceResult = new ConsumerServiceResult(outputRoot) {
				ExperianResult = "Passed",
				LastUpdateDate = DateTime.Now
			};

			Log.InfoFormat("Get consumer info for test user: {0}", surname);

			if (!checkInCacheOnly) {
				var sl = ObjectFactory.GetInstance<ServiceLogRepository>();
				MP_ServiceLog oFirst = m_oRetryer.Retry(() => sl.GetFirst());
				SaveDefaultAccountIntoDb(outputRoot, customerId, oFirst);
				financialAccountsParser.Parse(content, oFirst.Id, customerId);
			} // if

			return consumerServiceResult;
		} // CreateConsumerServiceResult

		#endregion method CreateConsumerServiceResult

		#region method GetServiceOutput

		private ConsumerServiceResult GetServiceOutput(
			string gender,
			InputLocationDetailsUKLocation ukLocation,
			InputLocationDetailsMultiLineLocation mlLocation,
			string applicationType,
			int customerId,
			int directorId,
			MP_ExperianDataCache cachedResponse
		) {
			var service = new InteractiveService();

			var inputControl = new InputControl {
				ExperianReference = "",
				ReprocessFlag = "N",
				Parameters = new InputControlParameters {
					AuthPlusRequired = "Y",
					FullFBLRequired = "Y",
					DetectRequired = "N",
					InteractiveMode = interactiveMode
				}
			};

			// 1 applicant
			var applicant = new InputApplicant {
				ApplicantIdentifier = "1",
				Name = new InputApplicantName { Forename = cachedResponse.Name, Surname = cachedResponse.Surname },
				Gender = gender
			};

			if (cachedResponse.BirthDate != null) {
				applicant.DateOfBirth = new InputApplicantDateOfBirth {
					CCYY = cachedResponse.BirthDate.Value.Year,
					DD = cachedResponse.BirthDate.Value.Day,
					MM = cachedResponse.BirthDate.Value.Month,
					CCYYSpecified = true,
					DDSpecified = true,
					MMSpecified = true
				};
			}

			// 1 address
			var address = new InputLocationDetails {
				LocationIdentifier = 1,
				UKLocation = ukLocation,
				MultiLineLocation = mlLocation
			};

			// 1 Residency Information 
			var residencyInfo = new InputResidency {
				LocationIdentifier = "1",
				ResidencyDateTo =
					new InputResidencyResidencyDateTo {
						CCYY = DateTime.Now.Year,
						MM = DateTime.Now.Month,
						DD = DateTime.Now.Day
					},
				ResidencyDateFrom = new InputResidencyResidencyDateFrom { CCYY = 2010, MM = 01, DD = 01 },
				ApplicantIdentifier = "1",
				LocationCode = "01"
			};

			// 1 Third Party Data (TPD) block
			var tpd = new InputThirdPartyData {
				OutcomeCode = "",
				OptOut = "N",
				TransientAssocs = "N",
				HHOAllowed = "N",
				OptoutValidCutOff = ""
			};

			var application = new InputApplication {
				ApplicationChannel = "",
				SearchConsent = "Y",
				ApplicationType = applicationType
			};

			var input = new Input {
				Control = inputControl,
				Applicant = new[] { applicant },
				LocationDetails = new[] { address },
				Residency = new[] { residencyInfo },
				ThirdPartyData = tpd,
				Application = application
			};

			Log.InfoFormat("GetConsumerInfo: request Experian service.");

			var output = service.GetOutput(input);

			var serviceLog = Utils.WriteLog(input, output, ExperianServiceType.Consumer, customerId, directorId);

			if (output != null && output.Output.Error == null) {
				cachedResponse.LastUpdateDate = DateTime.Now;
				cachedResponse.JsonPacket = JsonConvert.SerializeObject(output);
				cachedResponse.JsonPacketInput = JsonConvert.SerializeObject(input);
				cachedResponse.CustomerId = customerId;

				if (directorId != 0)
					cachedResponse.DirectorId = directorId;

				m_oRetryer.Retry(() => _repo.SaveOrUpdate(cachedResponse));

				SaveDefaultAccountIntoDb(output, customerId, serviceLog);
				financialAccountsParser.Parse(serviceLog.ResponseData, serviceLog.Id, customerId);
			} // if

			return new ConsumerServiceResult(output);
		} // GetServiceOutput

		#endregion method GetServiceOutput

		#region method CacheNotExpired

		private bool CacheNotExpired(MP_ExperianDataCache cacheEntry) {
			int cacheIsValidForDays = CurrentValues.Instance.UpdateConsumerDataPeriodDays;
			return (DateTime.UtcNow - cacheEntry.LastUpdateDate).TotalDays <= cacheIsValidForDays;
		} // CacheNotExpired

		#endregion method CacheNotExpired

		#region method ConsumerDebugResult

		private ConsumerServiceResult ConsumerDebugResult(string surname, DateTime? birthDate, int customerId, bool checkInCacheOnly) {
			var content = string.Empty;
			string testPart = string.Empty;

			string filename = string.Empty;

			if (surname != null && surname.Contains("_")) {
				string[] splitValues = surname.Split('_');

				if (splitValues.Length > 1 && !string.IsNullOrEmpty(splitValues[1])) {
					testPart = splitValues[1];

					if (!testPart.Contains(":")) {
						int mpSeviceLogId;
						int.TryParse(testPart, out mpSeviceLogId);

						if (mpSeviceLogId > 0) {
							var log = ObjectFactory.GetInstance<ServiceLogRepository>();
							TryRead(() => m_oRetryer.Retry(() => content = log.GetById(mpSeviceLogId).ResponseData));
						} // if
					}
					else {
						try {
							filename = testPart;
							content = File.ReadAllText(filename);
						}
						catch (Exception e) {
							Log.ErrorFormat("Can't read experian file:{0}. Exception:{1}", filename, e);
						} // try
					} // if
				} // if
			} // if

			if (content == string.Empty) {
				try {
					filename = string.IsNullOrEmpty(testPart) ? @"C:\Temp\Experian.xml" : testPart;
					content = File.ReadAllText(filename);
				}
				catch (Exception e) {
					Log.ErrorFormat("Can't read experian file:{0}. Exception:{1}", filename, e);
				} // try
			} // if

			return CreateConsumerServiceResult(surname, customerId, checkInCacheOnly, content);
		} // ConsumerDebugResult

		#endregion method ConsumerDebugResult

		#region properties

		private readonly ExperianDataCacheRepository _repo;
		private readonly SqlRetryer m_oRetryer;
		private static readonly ILog Log = LogManager.GetLogger(typeof(ConsumerService));
		private FinancialAccountsParser financialAccountsParser = new FinancialAccountsParser();
		private readonly string interactiveMode;

		#endregion properties

		#region static

		#region method ParseCache

		private static ConsumerServiceResult ParseCache(MP_ExperianDataCache person) {
			Log.InfoFormat(
				"GetConsumerInfo: return data from cache for firstName={0}, surname={1}, last update date={2}",
				person.Name,
				person.Surname,
				person.LastUpdateDate
			);

			OutputRoot data = null;
			if (!string.IsNullOrEmpty(person.JsonPacket))
			{
				data = JsonConvert.DeserializeObject<OutputRoot>(person.JsonPacket);
			}

			var consumerServiceResult = new ConsumerServiceResult(data) {
				ExperianResult = person.ExperianResult,
				LastUpdateDate = person.LastUpdateDate
			};

			return consumerServiceResult;
		} // ParseCache

		#endregion method ParseCache

		#region method GetPostcode

		private static string GetPostcode(
			InputLocationDetailsUKLocation ukLocation,
			InputLocationDetailsMultiLineLocation mlLocation
		) {
			return (ukLocation != null)
				? ukLocation.Postcode
				: (mlLocation != null) ? mlLocation.LocationLine6 : string.Empty;
		} // GetPostcode

		#endregion method GetPostcode

		#region method TryRead

		private static void TryRead(Action a) {
			try {
				a();
			}
			catch (Exception e) {
				Log.Warn(e);
			} // try
		} // TryRead

		#endregion method TryRead
		
		#endregion static

		#endregion private
	} // class ConsumerService
} // namespace