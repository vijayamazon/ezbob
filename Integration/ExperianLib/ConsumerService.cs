using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using ApplicationMng.Repository;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Repository;
using EZBob.DatabaseLib.Model.Experian;
using ExperianLib.Dictionaries;
using EzBob.Configuration;
using EzBobIntegration.Web_References.Consumer;
using Newtonsoft.Json;
using StructureMap;
using log4net;

namespace ExperianLib
{
    using EZBob.DatabaseLib.Repository;

    public class ConsumerService
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (ConsumerService));
        private readonly ExperianIntegrationParams _config;
        private readonly ExperianDataCacheRepository _repo;

        public ConsumerService()
        {
            _config = ConfigurationRootBob.GetConfiguration().Experian;
            _repo = ObjectFactory.GetInstance<ExperianDataCacheRepository>();
        }

        public ConsumerServiceResult GetConsumerInfo(string firstName,
                                                     string surname,
                                                     string gender,
                                                     DateTime? birthDate,
                                                     InputLocationDetailsUKLocation ukLocation,
                                                     InputLocationDetailsMultiLineLocation mlLocation,
                                                     string applicationType,
                                                     int customerId,
                                                     int directorId,
                                                     bool checkInCacheOnly = false
            )
        {
            try
            {
                //debug mode
                if (surname == "TestSurnameDebugMode" || surname == "TestSurnameFour" || surname == "TestSurnameFile")
                {
                    return ConsumerDebugResult(surname, birthDate, customerId, checkInCacheOnly);
                }

                Log.InfoFormat("GetConsumerInfo: checking cache for firstName={0}, surname={1}...", firstName, surname);
                var postcode = GetPostcode(ukLocation, mlLocation);
                ShifLocation(mlLocation);
                var cachedResponse = _repo.GetPersonFromCache(firstName, surname, birthDate, postcode);

                if (cachedResponse != null)
                {
                    if (CacheNotExpired(cachedResponse))
                    {
                        return ParseCache(cachedResponse);
                    }
                }

                if (checkInCacheOnly)
                {
                    return null;
                }

                cachedResponse = new MP_ExperianDataCache
                    {
                        Name = firstName,
                        Surname = surname,
                        BirthDate = birthDate,
                        PostCode = postcode
                    };
                return GetServiceOutput(gender, ukLocation, mlLocation, applicationType, customerId, directorId,
                                        cachedResponse);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return new ConsumerServiceResult {Error = "Exception: " + ex.Message};
            }
        }

        private ConsumerServiceResult CreateConsumerServiceResult(string surname, DateTime? birthDate, int customerId,
                                                                  bool checkInCacheOnly, string content)
        {
            var outputRootSerializer = new XmlSerializer(typeof (OutputRoot));
            var outputRoot = (OutputRoot) outputRootSerializer.Deserialize(new StringReader(content));
            var consumerServiceResult = new ConsumerServiceResult(outputRoot, birthDate)
                {
                    ExperianResult = "Passed",
                    LastUpdateDate = DateTime.Now
                };
            Log.InfoFormat("Get consumer info for test user: {0}", surname);
            var sl = ObjectFactory.GetInstance<ServiceLogRepository>();
            if (!checkInCacheOnly)
            {
                SaveDefaultAccountIntoDb(outputRoot, customerId, sl.GetFirst());
            }
            return consumerServiceResult;
        }

        private static string GetPostcode(InputLocationDetailsUKLocation ukLocation,
                                          InputLocationDetailsMultiLineLocation mlLocation)
        {
            var postcode = (ukLocation != null)
                               ? ukLocation.Postcode
                               : (mlLocation != null) ? mlLocation.LocationLine6 : string.Empty;
            return postcode;
        }

        private static void ShifLocation(InputLocationDetailsMultiLineLocation mlLocation)
        {
            //shift of order location line
            if (mlLocation != null)
            {
                var lines = new List<string>();

                if (!string.IsNullOrEmpty(mlLocation.LocationLine1)) lines.Add(mlLocation.LocationLine1);
                if (!string.IsNullOrEmpty(mlLocation.LocationLine2)) lines.Add(mlLocation.LocationLine2);
                if (!string.IsNullOrEmpty(mlLocation.LocationLine3)) lines.Add(mlLocation.LocationLine3);
                if (!string.IsNullOrEmpty(mlLocation.LocationLine4)) lines.Add(mlLocation.LocationLine4);
                if (!string.IsNullOrEmpty(mlLocation.LocationLine5)) lines.Add(mlLocation.LocationLine5);
                if (!string.IsNullOrEmpty(mlLocation.LocationLine6)) lines.Add(mlLocation.LocationLine6);

                mlLocation = new InputLocationDetailsMultiLineLocation();

                if (lines.Count > 0) mlLocation.LocationLine1 = lines[0];
                if (lines.Count > 1) mlLocation.LocationLine2 = lines[1];
                if (lines.Count > 2) mlLocation.LocationLine3 = lines[2];
                if (lines.Count > 3) mlLocation.LocationLine4 = lines[3];
                if (lines.Count > 4) mlLocation.LocationLine5 = lines[4];
                if (lines.Count > 5) mlLocation.LocationLine6 = lines[5];
            }
        }

        private ConsumerServiceResult GetServiceOutput(string gender,
                                                       InputLocationDetailsUKLocation ukLocation,
                                                       InputLocationDetailsMultiLineLocation mlLocation,
                                                       string applicationType,
                                                       int customerId, int directorId,
                                                       MP_ExperianDataCache cachedResponse)
        {
            var service = new InteractiveService();

            var inputControl = new InputControl
                {
                    ExperianReference = "",
                    ReprocessFlag = "N",
                    Parameters =
                        new InputControlParameters
                            {
                                AuthPlusRequired = "Y",
                                FullFBLRequired = "Y",
                                DetectRequired = "N",
                                InteractiveMode = _config.InteractiveMode
                            }
                };
            // 1 applicant
            var applicant = new InputApplicant
                {
                    ApplicantIdentifier = "1",
                    Name = new InputApplicantName {Forename = cachedResponse.Name, Surname = cachedResponse.Surname},
                    Gender = gender
                };

            if (cachedResponse.BirthDate != null)
            {
                applicant.DateOfBirth = new InputApplicantDateOfBirth
                    {
                        CCYY = cachedResponse.BirthDate.Value.Year,
                        DD = cachedResponse.BirthDate.Value.Day,
                        MM = cachedResponse.BirthDate.Value.Month,
                        CCYYSpecified = true,
                        DDSpecified = true,
                        MMSpecified = true
                    };
            }
            // 1 address
            var address = new InputLocationDetails
                {
                    LocationIdentifier = 1,
                    UKLocation = ukLocation,
                    MultiLineLocation = mlLocation
                };
            //1 Residency Information 
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
                    ResidencyDateFrom = new InputResidencyResidencyDateFrom {CCYY = 2010, MM = 01, DD = 01},
                    ApplicantIdentifier = "1",
                    LocationCode = "01"
                };

            //1 Third Party Data (TPD) block
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
                    Applicant = new[] {applicant},
                    LocationDetails = new[] {address},
                    Residency = new[] {residencyInfo},
                    ThirdPartyData = tpd,
                    Application = application
                };

            Log.InfoFormat("GetConsumerInfo: request Experian service.");

            var output = service.GetOutput(input);

            var serviceLog = Utils.WriteLog(input, output, "Consumer Request", customerId);

            if (output != null && output.Output.Error == null)
            {
                cachedResponse.LastUpdateDate = DateTime.Now;
                cachedResponse.JsonPacket = JsonConvert.SerializeObject(output);
                cachedResponse.JsonPacketInput = JsonConvert.SerializeObject(input);
                cachedResponse.CustomerId = customerId;
                if (directorId != 0) cachedResponse.DirectorId = directorId;
                _repo.SaveOrUpdate(cachedResponse);
                SaveDefaultAccountIntoDb(output, customerId, serviceLog);
            }

            return new ConsumerServiceResult(output, cachedResponse.BirthDate);
        }

        private static ConsumerServiceResult ParseCache(MP_ExperianDataCache person)
        {
            Log.InfoFormat(
                "GetConsumerInfo: return data from cache for firstName={0}, surname={1}, last update date={2}",
                person.Name, person.Surname, person.LastUpdateDate);
            var consumerServiceResult =
                new ConsumerServiceResult(JsonConvert.DeserializeObject<OutputRoot>(person.JsonPacket), person.BirthDate)
                    {
                        ExperianResult = person.ExperianResult,
                        LastUpdateDate = person.LastUpdateDate
                    };
            return consumerServiceResult;
        }

        private bool CacheNotExpired(MP_ExperianDataCache person)
        {
            return (DateTime.Now - person.LastUpdateDate).TotalDays <= _config.UpdateConsumerDataPeriodDays;
        }

        private ConsumerServiceResult ConsumerDebugResult(string surname, DateTime? birthDate, int customerId,
                                                          bool checkInCacheOnly)
        {
            var customers = ObjectFactory.GetInstance<CustomerRepository>();
            var customer = customers.Get(customerId);
            var content = string.Empty;
            string middleName = null;
            TryRead(() => middleName = customer.PersonalInfo.MiddleInitial);
            int mpSeviceLogId;
            int.TryParse(middleName, out mpSeviceLogId);

            if (!string.IsNullOrEmpty(middleName) && mpSeviceLogId > 0)
            {
                var log = ObjectFactory.GetInstance<ServiceLogRepository>();
                TryRead(() => content = log.GetById(mpSeviceLogId).ResponseData);
            }
            else
            {
                try
                {
                    var filename = string.IsNullOrEmpty(middleName)? @"C:\Temp\Experian.xml": middleName;
                    content = File.ReadAllText(filename);
                }
                catch (Exception e)
                {
                    Log.ErrorFormat("Can't read experian file:{0}. Exception:{1}", @"C:\Temp\Experian.xml", e);
                }
            }

            return CreateConsumerServiceResult(surname, birthDate, customerId, checkInCacheOnly, content);
        }

        public void SaveDefaultAccountIntoDb(OutputRoot output, int customerId, MP_ServiceLog serviceLog)
        {
            var customerRepo = ObjectFactory.GetInstance<NHibernateRepositoryBase<Customer>>();
            var customer = customerRepo.Get(customerId);
            OutputFullConsumerDataConsumerDataCAIS[] cais = null;
            var dateAdded = DateTime.UtcNow;
            var repo = ObjectFactory.GetInstance<NHibernateRepositoryBase<ExperianDefaultAccount>>();

            TryRead(() => cais = output.Output.FullConsumerData.ConsumerData.CAIS);
            if (cais == null)
            {
                return;
            }
            foreach (var caisData in cais)
            {
                foreach (var detail in caisData.CAISDetails.Where(detail => detail.AccountStatus == "F"))
                {
                    int relevantYear, relevantMonth, relevantDay;

                    if (detail.SettlementDate != null)
                    {
                        relevantYear = detail.SettlementDate.CCYY;
                        relevantMonth = detail.SettlementDate.MM;
                        relevantDay = detail.SettlementDate.DD;
                    }
                    else
                    {
                        relevantYear = detail.LastUpdatedDate.CCYY;
                        relevantMonth = detail.LastUpdatedDate.MM;
                        relevantDay = detail.LastUpdatedDate.DD;
                    }

                    var settlementDate = new DateTime(relevantYear, relevantMonth, relevantDay);
                    var currentDefBalance = 0;
                    var balance = 0;
                    var reg = new Regex("[^0-9,]");

                    OutputFullConsumerDataConsumerDataCAISCAISDetails tempDetail = detail;
                    TryRead(
                        () => int.TryParse(reg.Replace(tempDetail.CurrentDefBalance.Amount, ""), out currentDefBalance));
                    TryRead(() => int.TryParse(reg.Replace(tempDetail.Balance.Amount, ""), out balance));

                    repo.Save(new ExperianDefaultAccount
                        {
                            AccountType = AccountTypeDictionary.GetAccountType(detail.AccountType ?? string.Empty),
                            Date = settlementDate,
                            DelinquencyType = "Default",
                            Customer = customer,
                            DateAdded = dateAdded,
                            CurrentDefBalance = currentDefBalance,
                            Balance = balance,
                            ServiceLog = serviceLog,
                        });
                }
            }
        }

        private static void TryRead(Action a)
        {
            try
            {
                a();
            }
            catch (Exception e)
            {
                Log.Warn(e);
            }
        }
    }
}