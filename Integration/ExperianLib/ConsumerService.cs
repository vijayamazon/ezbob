using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using ApplicationMng.Repository;
using EZBob.DatabaseLib.Model.Database;
using ExperianLib.Properties;
using EzBob.Configuration;
using EzBobIntegration.Web_References.Consumer;
using Newtonsoft.Json;
using Scorto.Configuration;
using StructureMap;
using log4net;

namespace ExperianLib
{
    public class ConsumerService
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ConsumerService));
        readonly ExperianIntegrationParams _config;

        public ConsumerService()
        {
            _config = ConfigurationRootBob.GetConfiguration().Experian;
        }

        public ConsumerServiceResult GetConsumerInfo(string firstName, string surname, string gender, DateTime? birthDate, 
                                                        InputLocationDetailsUKLocation ukLocation,
                                                        InputLocationDetailsMultiLineLocation mlLocation,
                                                        string applicationType,
                                                        int customerId, 
                                                        bool checkInCacheOnly = false
                                                     )
        {
            try
            {
                if (surname.IndexOf("TestSurname") == 0)
                {
                    var outputString = Resources.output007;
                    switch (surname)
                    {
                        case "TestSurnameOne":
                            outputString = Resources.output007;
                            break;
                        case "TestSurnameTwo":
                            outputString = Resources.output020;
                            break;
                        case "TestSurnameThree":
                            outputString = Resources.output022;
                            break;
                    }

                    var outputRootSerializer = new XmlSerializer(typeof(OutputRoot));
                    var outputRoot = (OutputRoot)outputRootSerializer.Deserialize(new StringReader(outputString));
                    var consumerServiceResult = new ConsumerServiceResult(outputRoot, birthDate);
                    consumerServiceResult.ExperianResult = "Passed";
                    consumerServiceResult.LastUpdateDate = DateTime.Now;
                    return consumerServiceResult;
                }


                var repo = ObjectFactory.GetInstance<NHibernateRepositoryBase<MP_ExperianDataCache>>();
                Log.InfoFormat("GetConsumerInfo: checking cache for firstName={0}, surname={1}...", firstName, surname);
                var postcode = (ukLocation != null)
                                   ? ukLocation.Postcode
                                   : (mlLocation != null) ? mlLocation.LocationLine6 : string.Empty;

                //shift of order location line
                if(mlLocation != null)
                {
                    var lines = new List<string>();

                    if ( !string.IsNullOrEmpty(mlLocation.LocationLine1) ) lines.Add( mlLocation.LocationLine1 );
                    if ( !string.IsNullOrEmpty(mlLocation.LocationLine2) ) lines.Add( mlLocation.LocationLine2 );
                    if ( !string.IsNullOrEmpty(mlLocation.LocationLine3) ) lines.Add( mlLocation.LocationLine3 );
                    if ( !string.IsNullOrEmpty(mlLocation.LocationLine4) ) lines.Add( mlLocation.LocationLine4 );
                    if ( !string.IsNullOrEmpty(mlLocation.LocationLine5) ) lines.Add( mlLocation.LocationLine5 );
                    if ( !string.IsNullOrEmpty(mlLocation.LocationLine6) ) lines.Add( mlLocation.LocationLine6 );

                    mlLocation = new InputLocationDetailsMultiLineLocation();

                    if ( lines.Count > 0 ) mlLocation.LocationLine1 = lines[0];
                    if ( lines.Count > 1 ) mlLocation.LocationLine2 = lines[1];
                    if ( lines.Count > 2 ) mlLocation.LocationLine3 = lines[2];
                    if ( lines.Count > 3 ) mlLocation.LocationLine4 = lines[3];
                    if ( lines.Count > 4 ) mlLocation.LocationLine5 = lines[4];
                    if ( lines.Count > 5 ) mlLocation.LocationLine6 = lines[5];                            
                }

                var person = (from c in repo.GetAll() where c.Name == firstName && c.Surname == surname && c.BirthDate == birthDate && c.PostCode == postcode select c).FirstOrDefault();
                if (person != null)
                {
                    if ((DateTime.Now - person.LastUpdateDate).TotalDays <= _config.UpdateConsumerDataPeriodDays)
                    {
                        Log.InfoFormat("GetConsumerInfo: return data from cache for firstName={0}, surname={1}, last update date={2}", firstName, surname, person.LastUpdateDate);
                        var consumerServiceResult = new ConsumerServiceResult(JsonConvert.DeserializeObject<OutputRoot>(person.JsonPacket), birthDate);
                        consumerServiceResult.ExperianResult = person.ExperianResult;
                        consumerServiceResult.LastUpdateDate = person.LastUpdateDate;
                        return consumerServiceResult;
                    }
                }
                else if (checkInCacheOnly)
                {
                    return null;
                }
                else
                {
                    person = new MP_ExperianDataCache { Name = firstName, Surname = surname, BirthDate = birthDate, PostCode = postcode };
                }

                var service = new InteractiveService();

                var inputControl = new InputControl
                {
                    ExperianReference = "",
                    ReprocessFlag = "N",
                    Parameters = new InputControlParameters { AuthPlusRequired = "Y", FullFBLRequired = "Y", DetectRequired = "N", InteractiveMode = _config.InteractiveMode }
                };
                // 1 applicant
                var applicant = new InputApplicant
                {
                    ApplicantIdentifier = "1",
                    Name = new InputApplicantName { Forename = firstName, Surname = surname },
                    Gender = gender
                };
                if(birthDate != null)
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
                    UKLocation = ukLocation,
                    MultiLineLocation = mlLocation
                };
                //1 Residency Information 
                var residencyInfo = new InputResidency
                {
                    LocationIdentifier = "1",
                    ResidencyDateTo = new InputResidencyResidencyDateTo { CCYY = DateTime.Now.Year, MM = DateTime.Now.Month, DD = DateTime.Now.Day },
                    ResidencyDateFrom = new InputResidencyResidencyDateFrom { CCYY = 2010, MM = 01, DD = 01 },
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
                    Applicant = new[] { applicant },
                    LocationDetails = new[] { address },
                    Residency = new[] { residencyInfo },
                    ThirdPartyData = tpd,
                    Application = application
                };

                Log.InfoFormat("GetConsumerInfo: request Experian service.");

                var output = service.GetOutput(input);

                Utils.WriteLog(input, output, "Consumer Request", customerId);

                if (output != null && output.Output.Error == null)
                {
                    person.LastUpdateDate = DateTime.Now;
                    person.JsonPacket = JsonConvert.SerializeObject(output);
                    person.JsonPacketInput = JsonConvert.SerializeObject(input);
                    repo.SaveOrUpdate(person);
                }

                return new ConsumerServiceResult(output, birthDate);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return new ConsumerServiceResult{Error = "Exception: " + ex.Message};
            }
        }
    }
}
