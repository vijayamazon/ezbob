using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using EzBob.Configuration;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Repository;
using Ezbob.ExperianParser;
using Ezbob.Logger;
using log4net;
using StructureMap;

namespace EzBob.Web.Models
{
    public class CompanyScoreModel
    {
        public string result { get; set; }

        public Dictionary<string, ParsedData> dataset { get; set; }
    }

    public class CompanyScoreModelBuilder
    {

        private static readonly ILog ms_oLog = LogManager.GetLogger(typeof(CompanyScoreModelBuilder));

        private ServiceLogRepository m_oServiceLogRepository;

        public CompanyScoreModelBuilder()
        {
            m_oServiceLogRepository = ObjectFactory.GetInstance<ServiceLogRepository>();
        }

        public CompanyScoreModel Create(Customer customer)
        {
            IEnumerable<MP_ServiceLog> oServiceLogEntries =
                            m_oServiceLogRepository
                            .GetBuCustomer(customer)
                            .Where(x => x.ServiceType == "E-SeriesLimitedData")
                        ;

            if (!oServiceLogEntries.Any())
            {
                ms_oLog.InfoFormat("No data found for Company Score tab with customer id = {0}", customer.Id);
                return new CompanyScoreModel { result = "No data found." };
            } // if

            List<MP_ServiceLog> lst = oServiceLogEntries.ToList();

            lst.Sort(new ServiceLogComparer());

            var parser = new Ezbob.ExperianParser.Parser(
                DBConfigurationValues.Instance.CompanyScoreParserConfiguration,
                new SafeILog(ms_oLog)
            );

            var doc = new XmlDocument();

            try
            {
                doc.LoadXml(lst.Last().ResponseData);
            }
            catch (Exception e)
            {
                ms_oLog.Error(string.Format("Failed to parse Experian response as XML for Company Score tab with customer id = {0}", customer.Id), e);
                return new CompanyScoreModel{ result = "Failed to parse Experian response." };
            } // try

            try
            {
                Dictionary<string, ParsedData> oParsed = parser.NamedParse(doc);
                return new CompanyScoreModel { result = "ok", dataset = oParsed };
            }
            catch (Exception e)
            {
                ms_oLog.Error(string.Format("Failed to extract Company Score tab data from Experian response with customer id = {0}", customer.Id), e);
                return new CompanyScoreModel { result = "Failed to parse Experian response." };
            } // try
        }

        private class ServiceLogComparer : IComparer<MP_ServiceLog>
        {
            public int Compare(MP_ServiceLog x, MP_ServiceLog y)
            {
                return x.InsertDate.CompareTo(y.InsertDate);
            } // Compare
        } // class ServiceLogComparer


    }
}