namespace ExperianLib
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Xml.Serialization;
	using ApplicationMng.Repository;
	using Callcredit.CRBSB;
	using CallCreditLib;
	using ConfigManager;
	using Dictionaries;
	using Ezbob.Backend.ModelsWithDB;
	using Ezbob.Backend.ModelsWithDB.CallCredit.CallCreditData;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EZBob.DatabaseLib.Model.Experian;
	using EzBobIntegration.Web_References.Consumer;
	using EzServiceAccessor;
	using Ezbob.Backend.ModelsWithDB.Experian;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Newtonsoft.Json;
	using StructureMap;
	using log4net;
	using EZBob.DatabaseLib.Repository;

	public class ConsumerService
	{

		public static InputLocationDetailsMultiLineLocation ShiftLocation(InputLocationDetailsMultiLineLocation mlLocation)
		{
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

			mlLocation = new InputLocationDetailsMultiLineLocation
			{
				LocationLine1 = lines[0],
				LocationLine2 = lines[1],
				LocationLine3 = lines[2],
				LocationLine4 = lines[3],
				LocationLine5 = lines[4],
				LocationLine6 = lines[5],
			};

			return mlLocation;
		} // ShiftLocation

		public ConsumerService()
		{
			m_oRetryer = new SqlRetryer(oLog: new SafeILog(Log));
			interactiveMode = CurrentValues.Instance.ExperianInteractiveMode;
		} // constructor

		public ExperianConsumerData GetConsumerInfo(
			string firstName,
			string surname,
			string gender,
			DateTime? birthDate,
			InputLocationDetailsUKLocation ukLocation,
			InputLocationDetailsMultiLineLocation mlLocation,
			string applicationType,
			int customerId,
			int? directorId,
			bool checkInCacheOnly,
			bool isDirector,
			bool forceCheck
		)
		{
			try
			{
				mlLocation = ShiftLocation(mlLocation);
				string postcode = GetPostcode(ukLocation, mlLocation);

				Log.InfoFormat("GetConsumerInfo: checking cache for {2} id {3} firstName: {0}, surname: {1} birthday: {4}, postcode: {5} gender {6}  apptype: {7} \n {8} {9}", 
					firstName, surname, isDirector ? "director" : "customer", isDirector ? directorId : customerId, birthDate, postcode, gender, applicationType,
					JsonConvert.SerializeObject(ukLocation, new JsonSerializerSettings { Formatting = Formatting.Indented }),
					JsonConvert.SerializeObject(mlLocation, new JsonSerializerSettings { Formatting = Formatting.Indented }));

				ExperianConsumerData cachedResponse = ObjectFactory.GetInstance<IEzServiceAccessor>()
					.LoadExperianConsumer(1, customerId, isDirector ?  directorId : (int?)null , null);

				// debug mode
				if (surname.StartsWith("TestSurnameDebugMode") || surname == "TestSurnameOne" || surname == "TestSurnameFile")
				{
					//if (force check) or (no data in cache) or (data expired and not cache only mode)
					if (forceCheck || 
						cachedResponse.ServiceLogId == null || 
						(!checkInCacheOnly && cachedResponse.ServiceLogId != null && !CacheNotExpired(cachedResponse.InsertDate))) {
						var data = ConsumerDebugResult(surname, customerId);
						return data;
					}
					return cachedResponse;
				} // if test

				if (!forceCheck)
				{
					if (cachedResponse.ServiceLogId != null)
					{
						if (CacheNotExpired(cachedResponse.InsertDate) || checkInCacheOnly)
							return cachedResponse;
					}
					else if (checkInCacheOnly)
						return null;
				} // if

				return GetServiceOutput(gender, ukLocation, mlLocation, applicationType, customerId, directorId, firstName, surname, birthDate, postcode);
			}
			catch (Exception ex)
			{
				Log.Error(ex);
				return new ExperianConsumerData
					{
						HasExperianError = true,
						Error = "Exception: " + ex.Message
					};
			} // try
		} // GetConsumerInfo

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

		private ExperianConsumerData GetServiceOutput(
			string gender,
			InputLocationDetailsUKLocation ukLocation,
			InputLocationDetailsMultiLineLocation mlLocation,
			string applicationType,
			int customerId,
			int? directorId,
			string firstName,
			string surname,
			DateTime? birthDate,
			string postcode
		)
		{
			var service = new InteractiveService();

			var inputControl = new InputControl
			{
				ExperianReference = "",
				ReprocessFlag = "N",
				Parameters = new InputControlParameters
				{
					AuthPlusRequired = "Y",
					FullFBLRequired = "Y",
					DetectRequired = "N",
					InteractiveMode = interactiveMode
				}
			};

			// 1 applicant
			var applicant = new InputApplicant
			{
				ApplicantIdentifier = "1",
				Name = new InputApplicantName { Forename = firstName, Surname = surname },
				Gender = gender
			};

			if (birthDate != null)
			{
				applicant.DateOfBirth = new InputApplicantDateOfBirth
				{
					CCYY = birthDate.Value.Year,
					DD = birthDate.Value.Day,
					MM = birthDate.Value.Month,
					CCYYSpecified = true,
					DDSpecified = true,
					MMSpecified = true
				};
			}

			// 1 address
			var address = new InputLocationDetails
			{
				LocationIdentifier = 1,
				UKLocation = null,
				MultiLineLocation = mlLocation
			};

			// 1 Residency Information 
			var residencyInfo = new InputResidency
			{
				LocationIdentifier = "1",
				ResidencyDateTo =
					new InputResidencyResidencyDateTo
					{
						CCYY = DateTime.Now.Year,
						MM = DateTime.Now.Month,
						DD = DateTime.Now.Day
					},
				ResidencyDateFrom = new InputResidencyResidencyDateFrom { CCYY = 2010, MM = 01, DD = 01 },
				ApplicantIdentifier = "1",
				LocationCode = "01"
			};

			// 1 Third Party Data (TPD) block
			var tpd = new InputThirdPartyData
			{
				OutcomeCode = "",
				OptOut = "N",
				TransientAssocs = "N",
				HHOAllowed = "N",
				OptoutValidCutOff = ""
			};

			var application = new InputApplication
			{
				ApplicationChannel = "",
				SearchConsent = "Y",
				ApplicationType = applicationType
			};

			var input = new Input
			{
				Control = inputControl,
				Applicant = new[] { applicant },
				LocationDetails = new[] { address },
				Residency = new[] { residencyInfo },
				ThirdPartyData = tpd,
				Application = application
			};

			

			WriteToLogPackage.OutputData serviceLog;

			try {
				Log.InfoFormat("GetConsumerInfo: request Experian service.");
				var output = service.GetOutput(input);
				serviceLog = Utils.WriteLog(input, output, ExperianServiceType.Consumer, customerId, directorId, firstName, surname, birthDate, postcode);
				 SaveDefaultAccountIntoDb(output, customerId, serviceLog.ServiceLog);
			} catch (Exception ex) {
				serviceLog = new WriteToLogPackage.OutputData();
			}

			//retrieving data from call credit api
			GetCallCreditData(ukLocation, firstName, surname, birthDate, postcode, customerId, directorId);

			return serviceLog.ExperianConsumer;
		}//GetServiceOutput

		private void GetCallCreditData(InputLocationDetailsUKLocation ukLocation, string firstName, string surname, DateTime? birthDate, string postcode, int customerId, int? directorId) {
			if (!CurrentValues.Instance.CallCreditEnabled) {
				Log.InfoFormat(
					"Not retrieving CallCredit data for customer {0} director {1}: CallCredit is disabled.",
					customerId,
					directorId
				);
				return;
			} // if

			try {
				Log.InfoFormat("Retrieving CallCredit data for customer {0} director {1}", customerId, directorId);
				var searchRequest = new CT_searchapplicant {
					dob = birthDate ?? DateTime.UtcNow,
					dobSpecified = birthDate.HasValue,
					name = new[] {
						new CT_inputname {
							forename = firstName,
							surname = surname,
						}
					},
					address = new[] {
						new CT_inputaddress {
							postcode = ukLocation.Postcode,
							street1 = ukLocation.Street,
							street2 = ukLocation.Street2,
							startdateSpecified = false,
							enddateSpecified = false,
							buildingname = ukLocation.HouseName,
							buildingno = ukLocation.HouseNumber,
							posttown = ukLocation.PostTown,
						}
					},
					tpoptout = 0,
					tpoptoutSpecified = true
				};

				CallCreditLib.CallCreditGetData callCreditGetData = new CallCreditGetData(searchRequest);

				CT_SearchResult searchResponse = callCreditGetData.GetSearch07a();

				Log.InfoFormat(
					"Saving to ServiceLog CallCredit data fro customer {0} director {1}",
					customerId,
					directorId
				);

				Utils.WriteLog(
					searchRequest,
					searchResponse,
					ExperianServiceType.CallCredit,
					customerId,
					directorId,
					firstName,
					surname,
					birthDate,
					postcode
				);
			} catch (Exception ex) {
				Log.Error("Failed retrieve from data CallCredit", ex);
			}//try
		}//GetCallCreditData

		private bool CacheNotExpired(DateTime cacheDate)
		{
			int cacheIsValidForDays = CurrentValues.Instance.UpdateConsumerDataPeriodDays;
			return (DateTime.UtcNow - cacheDate).TotalDays <= cacheIsValidForDays;
		} // CacheNotExpired

		private ExperianConsumerData ConsumerDebugResult(string surname, int customerId)
		{
			var content = string.Empty;
			string testPart = string.Empty;

			string filename = string.Empty;

			if (surname != null && surname.Contains("_"))
			{
				string[] splitValues = surname.Split('_');

				if (splitValues.Length > 1 && !string.IsNullOrEmpty(splitValues[1]))
				{
					testPart = splitValues[1];

					if (!testPart.Contains(":"))
					{
						int mpSeviceLogId;
						int.TryParse(testPart, out mpSeviceLogId);

						if (mpSeviceLogId > 0)
						{
							var log = ObjectFactory.GetInstance<ServiceLogRepository>();
							TryRead(() => m_oRetryer.Retry(() => content = log.GetById(mpSeviceLogId).ResponseData));
						} // if
					}
					else
					{
						try
						{
							filename = testPart;
							content = File.ReadAllText(filename);
						}
						catch (Exception e)
						{
							Log.ErrorFormat("Can't read experian file:{0}. Exception:{1}", filename, e);
						} // try
					} // if
				} // if
			} // if

			if (string.IsNullOrEmpty(content))
			{
				try
				{
					filename = string.IsNullOrEmpty(testPart) ? @"C:\Temp\Experian.xml" : testPart;
					content = File.ReadAllText(filename);
				}
				catch (Exception e)
				{
					Log.ErrorFormat("Can't read experian file:{0}. Exception:{1}", filename, e);
				} // try
			} // if

			if (string.IsNullOrEmpty(content))
			{
				var sl = ObjectFactory.GetInstance<ServiceLogRepository>();
				MP_ServiceLog oFirst = m_oRetryer.Retry(() => sl.GetFirst());
				content = oFirst.ResponseData;
			}

			var outputRootSerializer = new XmlSerializer(typeof(OutputRoot));
			var outputRoot = (OutputRoot)outputRootSerializer.Deserialize(new StringReader(content));

			Log.InfoFormat("Get consumer info for test user: {0}", surname);
			var input = new Input { Applicant = new [] {new InputApplicant {FormattedName = surname, ClientPersonID = customerId.ToString(CultureInfo.InvariantCulture)}}};
			var serviceLog = Utils.WriteLog(input, outputRoot, ExperianServiceType.Consumer, customerId, null, null, surname, null, null);
			SaveDefaultAccountIntoDb(outputRoot, customerId, serviceLog.ServiceLog);
			var builder = new ConsumerExperianModelBuilder();

			return builder.Build(outputRoot, customerId);
		} // ConsumerDebugResult

		private readonly SqlRetryer m_oRetryer;
		private static readonly ILog Log = LogManager.GetLogger(typeof(ConsumerService));
		private readonly string interactiveMode;

		private static string GetPostcode(
			InputLocationDetailsUKLocation ukLocation,
			InputLocationDetailsMultiLineLocation mlLocation
		)
		{
			return (ukLocation != null)
				? ukLocation.Postcode
				: (mlLocation != null) ? mlLocation.LocationLine6 : string.Empty;
		} // GetPostcode

		private static void TryRead(Action a)
		{
			try
			{
				a();
			}
			catch (Exception e)
			{
				Log.Warn(e);
			} // try
		} // TryRead

	} // class ConsumerService
} // namespace
