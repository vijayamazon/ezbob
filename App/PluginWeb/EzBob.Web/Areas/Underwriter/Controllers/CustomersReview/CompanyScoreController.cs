using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Xml;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Repository;
using EZBob.DatabaseLib.Repository;
using EzBob.Configuration;
using EzBob.Web.Infrastructure.csrf;
using Ezbob.Logger;
using Scorto.Web;
using StructureMap;
using log4net;

namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview {
	public class CompanyScoreController : Controller {
		private readonly ICustomerRepository m_oCustomerRepository;
		private readonly ServiceLogRepository m_oServiceLogRepository;

		private static readonly ILog ms_oLog = LogManager.GetLogger(typeof (CompanyScoreController));

		public CompanyScoreController(ICustomerRepository customerRepository) {
			m_oCustomerRepository = customerRepository;
			m_oServiceLogRepository = ObjectFactory.GetInstance<ServiceLogRepository>();
		} // constructor

		[Ajax]
		[HttpGet]
		[Transactional]
		[ValidateJsonAntiForgeryToken]
		public JsonNetResult Index(int id) {
			var customer = m_oCustomerRepository.Get(id);

			IEnumerable<MP_ServiceLog> oServiceLogEntries =
				m_oServiceLogRepository
				.GetBuCustomer(customer)
				.Where(x => x.ServiceType == "E-SeriesLimitedData")
			;

			if (!oServiceLogEntries.Any()) {
				ms_oLog.InfoFormat("No data found for Company Score tab with customer id = {0}", id);
				return this.JsonNet(new {result = "No data found."});
			} // if

			List<MP_ServiceLog> lst = oServiceLogEntries.ToList();

			lst.Sort(new ServiceLogComparer());

			var parser = new Ezbob.ExperianParser.Parser(
				DBConfigurationValues.Instance.CompanyScoreParserConfiguration,
				new SafeILog(ms_oLog)
			);

			var doc = new XmlDocument();

			try {
				doc.LoadXml(lst.Last().ResponseData);
			}
			catch (Exception e) {
				ms_oLog.Error(string.Format("Failed to parse Experian response as XML for Company Score tab with customer id = {0}", id), e);
				return this.JsonNet(new { result = "Failed to parse Experian response." });
			} // try

			try {
				Dictionary<string, List<SortedDictionary<string, string>>> oParsed = parser.NamedParse(doc);
				return this.JsonNet(new {result = "ok", data = oParsed});
			}
			catch (Exception e) {
				ms_oLog.Error(string.Format("Failed to extract Company Score tab data from Experian response with customer id = {0}", id), e);
				return this.JsonNet(new { result = "Failed to parse Experian response." });
			} // try
		} // Index

		private class ServiceLogComparer : IComparer<MP_ServiceLog> {
			public int Compare(MP_ServiceLog x, MP_ServiceLog y) {
				return x.InsertDate.CompareTo(y.InsertDate);
			} // Compare
		} // class ServiceLogComparer
	} // CompanyScoreController
} // namespace
