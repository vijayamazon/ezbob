namespace ExperianLib.IdIdentityHub
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Security.Authentication;
	using System.Text.RegularExpressions;
	using System.Xml;
	using ConfigManager;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Experian;
	using EZBob.DatabaseLib.Repository;
	using Iesi.Collections.Generic;
	using Web_References.IDHubService;
	using StructureMap;
	using log4net;
	using AddressType = Web_References.IDHubService.AddressType;
	using GenderType = Web_References.IDHubService.GenderType;

	public class IdHubService
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(IdHubService));
		private readonly ExperianBankCacheRepository _bankCacheRepository;
		private readonly AmlResultsRepository amlResultsRepository;
		private string uIdCertificateThumb;
		private string authTokenServiceIdHub;
		private string idHubService;

		public IdHubService()
		{
			_bankCacheRepository = ObjectFactory.GetInstance<ExperianBankCacheRepository>();
			amlResultsRepository = ObjectFactory.GetInstance<AmlResultsRepository>();
			GetConfigs();
		}

		public IdHubService(ExperianBankCacheRepository bankCacheRepository)
		{
			_bankCacheRepository = bankCacheRepository;
			amlResultsRepository = ObjectFactory.GetInstance<AmlResultsRepository>();
			GetConfigs();
		}

		private void GetConfigs()
		{
			uIdCertificateThumb = CurrentValues.Instance.ExperianUIdCertificateThumb;
			authTokenServiceIdHub = CurrentValues.Instance.ExperianAuthTokenServiceIdHub;
			idHubService = CurrentValues.Instance.ExperianIdHubService;
		}

		//-----------------------------------------------------------------------------------
		public AuthenticationResults Authenticate(string foreName, string middleName, string surname, string gender, DateTime birth, string addressLine1, string addressLine2, string addressLine3, string town, string county, string postCode, int customerId, bool checkInCacheOnly = false, string xmlForDebug = "")
		{
			var result = new AuthenticationResults();

			var key = string.Format("{0}_{1}_{2}_{3}", foreName, middleName, surname, postCode);

			if (string.IsNullOrEmpty(xmlForDebug))
			{
				Log.DebugFormat("Checking key '{0}' in cache...", key);

				var cachedResult = amlResultsRepository.GetAll().FirstOrDefault(aml => aml.LookupKey == key && aml.IsActive);

				bool foundInCache = cachedResult != null;
				if (foundInCache)
				{
					Log.DebugFormat("Will use cache value for key '{0}'", key);
					
					result.AuthenticationIndex = cachedResult.AuthenticationIndex;
					result.AuthIndexText = cachedResult.AuthIndexText;
					result.NumPrimDataItems = cachedResult.NumPrimDataItems;
					result.NumPrimDataSources = cachedResult.NumPrimDataSources;
					result.NumSecDataItems = cachedResult.NumSecDataItems;
					result.StartDateOldestPrim = cachedResult.StartDateOldestPrim;
					result.StartDateOldestSec = cachedResult.StartDateOldestSec;
					result.ReturnedHRPCount = cachedResult.HighRiskRules.Count;
					if (result.ReturnedHRPCount == 0)
					{
						result.ReturnedHRP = new ReturnedHRPType[0];
					}
					else
					{
						result.ReturnedHRP = cachedResult.HighRiskRules.Select(rule => new ReturnedHRPType {HighRiskPolRuleID = rule.RuleId, HighRiskPolRuleText = rule.RuleText}).ToArray();
					}
					result.Error = cachedResult.Error;

					return result;
				}
			}
			if (checkInCacheOnly)
				return null;

			Log.DebugFormat("Request AML A service for key '{0}'", key);
			var address = FillAddress(addressLine1, addressLine2, addressLine3, town, county, postCode);
			//AML A
			var execRequest = new ExecuteRequestType
			{
				EIHHeader = new EIHHeaderType
				{
					ClientUser = "User1",
					ReferenceId = "1234"
				},
				ResponseType = ResponseType.Detail,
				ProcessConfigReference = new ProcessConfigReferenceType { ItemElementName = ItemChoiceType.ProcessConfigName, Item = "AML A" },
				Consent = ConsentType.Yes,
				PersonalData = new PersonalDataType
				{
					Name = new NameType
					{
						Forename = foreName,
						MiddleName = middleName,
						Surname = surname
					},
					BirthDate = birth,
					BirthDateSpecified = true,
					GenderSpecified = true
				},
				Addresses = new[] { address }
			};
			execRequest.PersonalData.Gender = gender == "M" ? GenderType.Male : GenderType.Female;

			try
			{
				ProcessConfigResponseType r;
				if (string.IsNullOrEmpty(xmlForDebug))
				{
					try
					{
						r = MakeRequest(execRequest);
					}
					catch (AuthenticationException exception)
					{
						result.Error = exception.Message;
						return result;
					}
				}
				else
				{
					r = GetRequestFromXml(xmlForDebug);
				}
				var res = Utils.WriteLog(execRequest, r, ExperianServiceType.Aml, customerId);

				result.Parse(r);

				SaveAmlData(customerId, key, res, result);
			}
			catch (Exception exception)
			{
				Log.Error(exception);
				result.Error = exception.Message;
				Utils.WriteLog(execRequest, string.Format("Excecption: {0}", exception.Message), ExperianServiceType.Aml, customerId);
			}

			return result;
		}

		//-----------------------------------------------------------------------------------
		public AuthenticationResults AuthenticateForcedWithCustomAddress(string foreName, string middleName, string surname, string gender, DateTime birth,
																   string houseNumber, string houseName, string street, string district,
																   string town, string county, string postCode, int customerId, string xmlForDebug = "")
		{
			var result = new AuthenticationResults();

			var key = String.Format("{0}_{1}_{2}_{3}", foreName, middleName, surname, postCode);

			Log.DebugFormat("Request AML A service for key '{0}'", key);
			var address = new AddressType
			{
				AddressStatus = AddressStatusType.Current,
				TypeOfAddress = TypeOfAddressType.UK,
				AddressDetail = new AddressDetailType
				{
					PostCode = postCode,
					HouseNumber = houseNumber,
					HouseName = houseName,
					Address1 = street,
					Address2 = district,
					Address3 = town,
					Address4 = county,
					Country = "GB"
				}
			};

			//AML A
			var execRequest = new ExecuteRequestType
			{
				EIHHeader = new EIHHeaderType
				{
					ClientUser = "User1",
					ReferenceId = "1234"
				},
				ResponseType = ResponseType.Detail,
				ProcessConfigReference = new ProcessConfigReferenceType { ItemElementName = ItemChoiceType.ProcessConfigName, Item = "AML A" },
				Consent = ConsentType.Yes,
				PersonalData = new PersonalDataType
				{
					Name = new NameType
					{
						Forename = foreName,
						MiddleName = middleName,
						Surname = surname
					},
					BirthDate = birth,
					BirthDateSpecified = true,
					GenderSpecified = true
				},
				Addresses = new[] { address }
			};
			execRequest.PersonalData.Gender = gender == "M" ? GenderType.Male : GenderType.Female;

			try
			{
				ProcessConfigResponseType r;
				if (string.IsNullOrEmpty(xmlForDebug))
				{
					try
					{
						r = MakeRequest(execRequest);
					}
					catch (AuthenticationException exception)
					{
						result.Error = exception.Message;
						return result;
					}
				}
				else
				{
					r = GetRequestFromXml(xmlForDebug);
				}
				var writelog = Utils.WriteLog(execRequest, r, ExperianServiceType.Aml, customerId);

				SaveAmlData(customerId, key, writelog, result);

				result.Parse(r);
			}
			catch (Exception exception)
			{
				Log.Error(exception);
				result.Error = exception.Message;
				Utils.WriteLog(execRequest, string.Format("Excecption: {0}", exception.Message), ExperianServiceType.Aml, customerId);
			}

			return result;
		}

		private void SaveAmlData(int customerId, string key, WriteToLogPackage.OutputData writelog, AuthenticationResults result)
		{
			var amlResult = new AmlResults
				{
					LookupKey = key,
					CustomerId = customerId,
					ServiceLogId = writelog.ServiceLog.Id,
					Created = writelog.ServiceLog.InsertDate,
					AuthenticationDecision = result.AuthenticationDecision,
					AuthenticationIndex = result.AuthenticationIndex,
					AuthIndexText = result.AuthIndexText,
					NumPrimDataItems = result.NumPrimDataItems,
					NumPrimDataSources = result.NumPrimDataSources,
					NumSecDataItems = result.NumSecDataItems,
					StartDateOldestPrim = result.StartDateOldestPrim,
					StartDateOldestSec = result.StartDateOldestSec,
					Error = result.Error,
					IsActive = true
				};

			List<AmlResultsHighRiskRules> highRiskRules =
				result.ReturnedHRP.Select(
					rule =>
					new AmlResultsHighRiskRules
						{
							RuleId = rule.HighRiskPolRuleID,
							RuleText = rule.HighRiskPolRuleText,
							AmlResult = amlResult
						}).ToList();
			amlResult.HighRiskRules = new HashedSet<AmlResultsHighRiskRules>(highRiskRules);
			amlResultsRepository.SaveOrUpdate(amlResult);
		}

		public AuthenticationResults GetResults_ForBackfill(string xml)
		{
			var result = new AuthenticationResults();
			ProcessConfigResponseType r;
			r = GetRequestFromXml(xml);
			result.Parse(r);
			return result;
		}

		//-----------------------------------------------------------------------------------
		public AddressType FillAddress(string addressLine1, string addressLine2, string addressLine3, string town, string county, string postCode)
		{
			var flatOrAppartmentNumber = string.Empty;
			var houseName = string.Empty;
			var houseNumber = string.Empty;
			var address1 = string.Empty;
			var address2 = string.Empty;
			var poBox = string.Empty;
			if (!string.IsNullOrEmpty(addressLine1) && string.IsNullOrEmpty(addressLine2) && string.IsNullOrEmpty(addressLine3))
			{
				houseNumber = Regex.Match(addressLine1, "\\d*").Value;
				if (!string.IsNullOrEmpty(houseNumber))
				{
					var line1 = addressLine1.Split(' ');
					houseNumber = line1[0];
					address1 = string.Join(" ", line1.Skip(1));
				}
				else if (addressLine1.ToUpper().StartsWith("PO BOX"))
				{
					poBox = addressLine1;
				}
				else
				{
					houseName = addressLine1;
				}
			}
			else if (!string.IsNullOrEmpty(addressLine1) && !string.IsNullOrEmpty(addressLine2) && string.IsNullOrEmpty(addressLine3))
			{
				houseNumber = Regex.Match(addressLine1, "\\d*").Value;
				if (!string.IsNullOrEmpty(houseNumber))
				{
					var line1 = addressLine1.Split(' ');
					houseNumber = line1[0];
					address1 = string.Join(" ", line1.Skip(1));

					address2 = addressLine2;
				}
				else if (addressLine1.ToUpper().StartsWith("APARTMENT") || addressLine1.ToUpper().StartsWith("FLAT"))
				{
					flatOrAppartmentNumber = addressLine1;
					houseNumber = Regex.Match(addressLine2, "\\d*").Value;
					if (!string.IsNullOrEmpty(houseNumber))
					{
						var line2 = addressLine2.Split(' ');
						houseNumber = line2[0];
						address1 = string.Join(" ", line2.Skip(1));
					}
					else
					{
						address1 = addressLine2;
					}
				}
				else
				{
					houseNumber = Regex.Match(addressLine2, "\\d*").Value;
					houseName = addressLine1;
					if (!string.IsNullOrEmpty(houseNumber))
					{
						var line2 = addressLine2.Split(' ');
						houseNumber = line2[0];
						address1 = string.Join(" ", line2.Skip(1));
					}
					else
					{
						address1 = addressLine2;
					}
				}
			}
			else if ((!string.IsNullOrEmpty(addressLine1) && !string.IsNullOrEmpty(addressLine2) && !string.IsNullOrEmpty(addressLine3)))
			{
				if (addressLine1.ToUpper().StartsWith("APARTMENT") || addressLine1.ToUpper().StartsWith("FLAT"))
				{
					flatOrAppartmentNumber = addressLine1;
					houseNumber = Regex.Match(addressLine2, "\\d*").Value;
					if (!string.IsNullOrEmpty(houseNumber))
					{
						var line2 = addressLine2.Split(' ');
						houseNumber = line2[0];
						address1 = string.Join(" ", line2.Skip(1));

						address2 = addressLine3;
					}
					else
					{
						houseNumber = Regex.Match(addressLine3, "\\d*").Value;
						if (!string.IsNullOrEmpty(houseNumber))
						{
							var line3 = addressLine3.Split(' ');
							houseNumber = line3[0];
							address1 = string.Join(" ", line3.Skip(1));

						}
						else
						{
							address1 = addressLine3;
						}

						houseName = addressLine2;
						
					}
				}
				else if (Regex.Match(addressLine1, "^\\d[0-9a-zA-Z ]*$").Success && !addressLine1.ToUpper().Contains("UNIT") && !addressLine1.ToUpper().Contains("BLOCK"))
				{
					houseNumber = Regex.Match(addressLine1, "\\d*").Value;
					if (!string.IsNullOrEmpty(houseNumber))
					{
						var line1 = addressLine1.Split(' ');
						houseNumber = line1[0];
						address1 = string.Join(" ", line1.Skip(1));
						address2 = addressLine2;
					}
				}
				else
				{
					houseName = addressLine1;
					address1 = addressLine2;
					address2 = addressLine3;
				}
			}

			return new AddressType
			{
				AddressStatus = AddressStatusType.Current,
				TypeOfAddress = TypeOfAddressType.UK,
				AddressDetail = new AddressDetailType
				{
					FlatOrApartmentNumber = flatOrAppartmentNumber,
					HouseNumber = houseNumber,
					HouseName = houseName,
					Address1 = address1,
					Address2 = address2,
					Address3 = town,
					Address4 = county,
					Country = "GB",
					PostCode = postCode,
					POBox = poBox
				}
			};
		}

		//-----------------------------------------------------------------------------------
		public AccountVerificationResults AccountVerification(string foreName, string middleName, string surname, string gender, DateTime birth, string addressLine1, string addressLine2, string addressLine3, string town, string county, string postCode, string branchCode, string accountNumber, int customerId, bool checkInCacheOnly = false, string xmlForDebug = "")
		{
			var result = new AccountVerificationResults();

			var key = String.Format("{0}_{1}", branchCode, accountNumber);
			Log.DebugFormat("Checking key '{0}' in cache...", key);
			var cachedValue = _bankCacheRepository.Get<ProcessConfigResponseType>(key, null);
			if (cachedValue != null && string.IsNullOrEmpty(xmlForDebug))
			{
				Log.DebugFormat("Will use cache value for key '{0}'", key);
				result.Parse(cachedValue);
				return result;
			}
			if (checkInCacheOnly)
				return null;

			var address = FillAddress(addressLine1, addressLine2, addressLine3, town, county, postCode);
			//BWA
			var execRequestBwa = new ExecuteRequestType
			{
				EIHHeader = new EIHHeaderType
				{
					ClientUser = "User1",
					ReferenceId = "1234"
				},
				ResponseType = ResponseType.Detail,
				ProcessConfigReference = new ProcessConfigReferenceType { ItemElementName = ItemChoiceType.ProcessConfigName, Item = "BWA" },
				Consent = ConsentType.Yes,
				PersonalData = new PersonalDataType
				{
					Name = new NameType
					{
						Forename = foreName,
						Surname = surname,
						MiddleName = middleName
					},
					BirthDate = birth,
					BirthDateSpecified = true,
					GenderSpecified = true
				},
				Addresses = new[] { address },
				BankInformation = new BankInformationType
				{
					CheckContext = CheckContextType.DirectCredit,
					CheckContextSpecified = true,
					AccountReference = new[]
                    {
                        new AccountReferenceType{TypeOfReference = TypeOfReferenceType.BankBranchCode, Reference = branchCode, ReferenceIndex = "1"},
                        new AccountReferenceType{TypeOfReference = TypeOfReferenceType.AccountNumber, Reference = accountNumber, ReferenceIndex = "2"}
                    }
				}
			};
			execRequestBwa.PersonalData.Gender = gender == "M" ? GenderType.Male : GenderType.Female;

			try
			{
				ProcessConfigResponseType r;
				if (string.IsNullOrEmpty(xmlForDebug))
				{
					try
					{
						r = MakeRequest(execRequestBwa);
					}
					catch (AuthenticationException exception)
					{
						result.Error = exception.Message;
						return result;
					}
				}
				else
				{
					r = GetRequestFromXml(xmlForDebug);
				}
				var writeLog = Utils.WriteLog(execRequestBwa, r, ExperianServiceType.Bwa, customerId);
				_bankCacheRepository.Set(key, r, writeLog.ServiceLog);

				result.Parse(r);
			}
			catch (Exception exception)
			{
				Log.Error(exception);
				result.Error = exception.Message;
				Utils.WriteLog(execRequestBwa, string.Format("Excecption: {0}", exception.Message), ExperianServiceType.Bwa, customerId);
			}

			return result;

		}

		//-----------------------------------------------------------------------------------
		public AccountVerificationResults AccountVerificationForcedWithCustomAddress(string foreName, string middleName, string surname,
															  string gender, DateTime birth,
															  string houseNumber, string houseName, string street, string district,
															  string town, string county, string postCode,
															  string branchCode, string accountNumber, int customerId, string xmlForDebug = "")
		{
			var result = new AccountVerificationResults();

			var key = String.Format("{0}_{1}", branchCode, accountNumber);

			var address = new AddressType
			{
				AddressStatus = AddressStatusType.Current,
				TypeOfAddress = TypeOfAddressType.UK,
				AddressDetail = new AddressDetailType
				{
					PostCode = postCode,
					HouseNumber = houseNumber,
					HouseName = houseName,
					Address1 = street,
					Address2 = district,
					Address3 = town,
					Address4 = county,
					Country = "GB"
				}
			};
			//BWA
			var execRequestBwa = new ExecuteRequestType
			{
				EIHHeader = new EIHHeaderType
				{
					ClientUser = "User1",
					ReferenceId = "1234"
				},
				ResponseType = ResponseType.Detail,
				ProcessConfigReference = new ProcessConfigReferenceType { ItemElementName = ItemChoiceType.ProcessConfigName, Item = "BWA" },
				Consent = ConsentType.Yes,
				PersonalData = new PersonalDataType
				{
					Name = new NameType
					{
						Forename = foreName,
						Surname = surname,
						MiddleName = middleName
					},
					BirthDate = birth,
					BirthDateSpecified = true,
					GenderSpecified = true
				},
				Addresses = new[] { address },
				BankInformation = new BankInformationType
				{
					CheckContext = CheckContextType.DirectCredit,
					CheckContextSpecified = true,
					AccountReference = new[]
                    {
                        new AccountReferenceType{TypeOfReference = TypeOfReferenceType.BankBranchCode, Reference = branchCode, ReferenceIndex = "1"},
                        new AccountReferenceType{TypeOfReference = TypeOfReferenceType.AccountNumber, Reference = accountNumber, ReferenceIndex = "2"}
                    }
				}
			};
			execRequestBwa.PersonalData.Gender = gender == "M" ? GenderType.Male : GenderType.Female;

			try
			{
				ProcessConfigResponseType r;
				if (string.IsNullOrEmpty(xmlForDebug))
				{
					try
					{
						r = MakeRequest(execRequestBwa);
					}
					catch (AuthenticationException exception)
					{
						result.Error = exception.Message;
						return result;
					}
				}
				else
				{
					r = GetRequestFromXml(xmlForDebug);
				}

				var writeLog = Utils.WriteLog(execRequestBwa, r, ExperianServiceType.Bwa, customerId);
				_bankCacheRepository.Set(key, r, writeLog.ServiceLog);

				result.Parse(r);
			}
			catch (Exception exception)
			{
				Log.Error(exception);
				result.Error = exception.Message;
				Utils.WriteLog(execRequestBwa, string.Format("Excecption: {0}", exception.Message), ExperianServiceType.Bwa, customerId);
			}

			return result;
		}

		private ProcessConfigResponseType MakeRequest(ExecuteRequestType execRequest)
		{
			var service = InitService();
			if (service == null)
			{
				Log.Error("Auth service problems, please check");
				throw new AuthenticationException("Auth problems, please see log.");
			}
			return service.ExecuteRequestOperation(execRequest);
		}

		private static ProcessConfigResponseType GetRequestFromXml(string xml)
		{
			var xmlDoc = XmlReader.Create(new System.IO.StringReader(xml));
			var serialize = new System.Xml.Serialization.XmlSerializer(typeof(ProcessConfigResponseType));
			return serialize.Deserialize(xmlDoc) as ProcessConfigResponseType;
		}
		//-----------------------------------------------------------------------------------
		private EndpointService InitService()
		{
			Log.InfoFormat("Getting auth token...");
			Log.InfoFormat("Will use certificate: {0} and URL: {1}", uIdCertificateThumb, authTokenServiceIdHub);
			var service = new AuthToken(uIdCertificateThumb, "CertificateAuthentication", authTokenServiceIdHub);
			var token = service.GetAuthToken();
			if (token == null)
			{
				Log.Error("Auth token getting failed.");
				return null;
			}

			var ws = new EndpointService();
			var ctx = ws.RequestSoapContext;
			ctx.Security.Tokens.Add(token);
			ctx.Security.MustUnderstand = false;
			ws.ClientCertificates.Add(service.GetCertificate(uIdCertificateThumb));
			ws.Url = idHubService;
			return ws;
		}
	}
}
