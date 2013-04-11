using System;
using System.Security.Authentication;
using System.Text.RegularExpressions;
using System.Xml;
using EZBob.DatabaseLib.Model.Database.Repository;
using ExperianLib.Web_References.IDHubService;
using EzBob.Configuration;
using Scorto.Configuration;
using StructureMap;
using log4net;
using AddressType = ExperianLib.Web_References.IDHubService.AddressType;
using GenderType = ExperianLib.Web_References.IDHubService.GenderType;

namespace ExperianLib.IdIdentityHub
{
    public class IdHubService
    {
        readonly ExperianIntegrationParams _config;
        private static readonly ILog Log = LogManager.GetLogger(typeof(IdHubService));
        private ExperianBankCacheRepository _bankCacheRepository;

        public IdHubService()
        {
            _config = ConfigurationRootBob.GetConfiguration().Experian;
            _bankCacheRepository = ObjectFactory.GetInstance<ExperianBankCacheRepository>();
        }

        //-----------------------------------------------------------------------------------
        public AuthenticationResults Authenticate(string foreName, string middleName, string surname, string gender, DateTime birth, string addressLine1, string addressLine2, string addressLine3, string town, string county, string postCode, int customerId, bool checkInCacheOnly = false, string xmlForDebug = "")
        {
            var result = new AuthenticationResults();

            var key = String.Format("{0}_{1}_{2}_{3}", foreName, middleName, surname, postCode);
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
                Addresses = new[]{address}
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
                var logItem = Utils.WriteLog(execRequest, r, "AML A check", customerId);
                _bankCacheRepository.Set(key, r, logItem);

                result.Parse(r);
            }
            catch (Exception exception)
            {
                Log.Error(exception);
                result.Error = exception.Message;
                Utils.WriteLog(execRequest.ToString(), string.Format("Excecption: {0}", exception.Message), "AML A check", customerId);
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
                var logItem = Utils.WriteLog(execRequest, r, "AML A check", customerId);
                _bankCacheRepository.Set(key, r, logItem);

                result.Parse(r);
            }
            catch (Exception exception)
            {
                Log.Error(exception);
                result.Error = exception.Message;
                Utils.WriteLog(execRequest.ToString(), string.Format("Excecption: {0}", exception.Message), "AML A check", customerId);
            }

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
                    address1 = addressLine1;
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
                    address1 = addressLine1;
                    address2 = addressLine2;
                }
                else if (addressLine1.ToUpper().StartsWith("APARTMENT") || addressLine1.ToUpper().StartsWith("FLAT"))
                {
                    flatOrAppartmentNumber = addressLine1;
                    houseNumber = Regex.Match(addressLine2, "\\d*").Value;
                    address1 = addressLine2;
                }
                else
                {
                    houseNumber = Regex.Match(addressLine2, "\\d*").Value;
                    houseName = addressLine1;
                    address1 = addressLine2;
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
                        address1 = addressLine2;
                        address2 = addressLine3;
                    }
                    else
                    {
                        houseNumber = Regex.Match(addressLine3, "\\d*").Value;
                        houseName = addressLine2;
                        address1 = addressLine3;
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
                Addresses = new[]{address},
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
                var logItem = Utils.WriteLog(execRequestBwa, r, "BWA check", customerId);
                _bankCacheRepository.Set(key, r, logItem);

                result.Parse(r);
            }
            catch (Exception exception)
            {
                Log.Error(exception);
                result.Error = exception.Message;
                Utils.WriteLog(execRequestBwa.ToString(), string.Format("Excecption: {0}", exception.Message), "BWA check", customerId);
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

                var logItem = Utils.WriteLog(execRequestBwa, r, "BWA check", customerId);
                _bankCacheRepository.Set(key, r, logItem);

                result.Parse(r);
            }
            catch (Exception exception)
            {
                Log.Error(exception);
                result.Error = exception.Message;
                Utils.WriteLog(execRequestBwa.ToString(), string.Format("Excecption: {0}", exception.Message), "BWA check", customerId);
            }

            return result;
        }

        private ProcessConfigResponseType MakeRequest( ExecuteRequestType execRequest)
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
            Log.InfoFormat("Will use certificate: {0} and URL: {1}", _config.UIdCertificateThumb, _config.AuthTokenServiceIdHub);
            var service = new AuthToken(_config.UIdCertificateThumb, "CertificateAuthentication", _config.AuthTokenServiceIdHub);
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
            ws.ClientCertificates.Add(service.GetCertificate(_config.UIdCertificateThumb));
            ws.Url = _config.IdHubService;
            return ws;
        }
    }
}
