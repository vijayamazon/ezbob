namespace EzBob.Web.Areas.Customer.Controllers {
	#region using

	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Data;
	using System.Globalization;
	using System.Linq;
	using System.Security.Principal;
	using System.Web.Mvc;
	using System.Web.Security;
	using DbConstants;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Mapping;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using ExperianLib.Ebusiness;
	using EzBob.Models;
	using Ezbob.Backend.Models;
	using Infrastructure.Attributes;
	using Models;
	using Code;
	using Infrastructure;
	using Infrastructure.csrf;
	using Iesi.Collections.Generic;
	using NHibernate;
	using EzServiceReference;
	using log4net;

	#endregion using

	public class CustomerDetailsController : Controller {
		#region public

		#region static method AddDirectorToCustomer

		public static object AddDirectorToCustomer(DirectorModel director, Customer customer, ISession session, bool bFailOnDuplicate) {
			if (customer.Company == null)
				return new { error = "Customer doesn't have a company" };

			Director dbDirector = director.FromModel();

			string sDirKey = DetailsToKey(dbDirector);

			if (sDirKey == DetailsToKey(customer))
				if (bFailOnDuplicate)
					return new { error = "This director already added" };

			dbDirector.Customer = customer;
			dbDirector.Company = customer.Company;

			var nAddressType = customer.Company.TypeOfBusiness.Reduce() == TypeOfBusinessReduced.Limited
				? CustomerAddressType.LimitedDirectorHomeAddress
				: CustomerAddressType.NonLimitedDirectorHomeAddress;

			foreach (var address in dbDirector.DirectorAddressInfo.AllAddresses) {
				address.Director = dbDirector;
				address.AddressType = nAddressType;
			} // for each

			if (customer.Company.Directors.Any()) {
				foreach (var dir in customer.Company.Directors) {
					if (sDirKey == DetailsToKey(dir))
						if (bFailOnDuplicate)
							return new { error = "This director already added" };
				} // for each existing director
			} // if customer has director(s)

			customer.Company.Directors.Add(dbDirector);

			session.Flush();
			return new { success = true };
		} // AddDirectorToCustomer

		#endregion static method AddDirectorToCustomer

		#region constructor

		public CustomerDetailsController(
			IEzbobWorkplaceContext context,
			DatabaseDataHelper helper,
			IPersonalInfoHistoryRepository personalInfoHistoryRepository,
			ISession session,
			CashRequestBuilder crBuilder,
			DirectorRepository directorRepository
		) {
			_context = context;
			_helper = helper;
			_personalInfoHistoryRepository = personalInfoHistoryRepository;
			m_oServiceClient = new ServiceClient();
			_session = session;
			_crBuilder = crBuilder;
			_directorRepository = directorRepository;
		} // constructor

		#endregion constructor

		#region method Dashboard

		[HttpGet]
		public System.Web.Mvc.ActionResult Dashboard() {
			var blm = new WizardBrokerLeadModel(Session);

			if (blm.BrokerFillsForCustomer) {
				StringActionResult sar = null;

				try {
					sar = m_oServiceClient.Instance.BrokerBackFromCustomerWizard(blm.LeadID);
				}
				catch (Exception e) {
					ms_oLog.Warn("Failed to retrieve broker details, falling back to customer's dashboard.", e);
				} // try

				if (sar != null) {
					FormsAuthentication.SignOut();
					HttpContext.User = new GenericPrincipal(new GenericIdentity(string.Empty), null);
					FormsAuthentication.SetAuthCookie(sar.Value, true);

					Session[Constant.Broker.MessageOnStart] = string.Format(
						"Customer {0} {1} ({2}) has been created.", blm.FirstName, blm.LastName, blm.LeadEmail
					);
					Session[Constant.Broker.MessageOnStartSeverity] = Constant.Severity.Info;

					blm.Unset();
					return RedirectToAction("Index", "BrokerHome", new { Area = "Broker", });
				} // if
			} // if

			blm.Unset();
			return RedirectToAction("Index", "Profile", new { Area = "Customer" });
		} // Dashboard

		#endregion method ToProfile

		#region method LinkAccountsComplete

		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public JsonResult LinkAccountsComplete() {
			WizardComplete();
			return Json(new { });
		} // LinkAccountsComplete

		#endregion method LinkAccountsComplete

		#region method TakeQuickOffer

		[Transactional]
		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public JsonResult TakeQuickOffer() {
			var customer = _context.Customer;

			Session["WizardComplete"] = false;
			TempData["WizardComplete"] = false;

			ms_oLog.DebugFormat("Customer {1} ({0}): has completed wizard by taking a quick offer.", customer.Id, customer.PersonalInfo.Fullname);

			customer.WizardStep = _helper.WizardSteps.GetAll().FirstOrDefault(x => x.ID == (int)WizardStepType.AllStep);

			_session.Flush();

			ms_oLog.DebugFormat("Customer {1} ({0}): wizard step has been updated to {2}", customer.Id, customer.PersonalInfo.Fullname, (int)WizardStepType.AllStep);

			_crBuilder.CreateQuickOfferCashRequest(customer);

			ms_oLog.DebugFormat("Customer {1} ({0}): cash request created.", customer.Id, customer.PersonalInfo.Fullname);

			_concentAgreementHelper.Save(customer, DateTime.UtcNow);

			ms_oLog.DebugFormat("Customer {1} ({0}): consent agreement saved.", customer.Id, customer.PersonalInfo.Fullname);

			return Json(new { });
		} // TakeQuickOffer

		#endregion method TakeQuickOffer

		#region method WizardComplete

		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public JsonResult WizardComplete() {
			Session["WizardComplete"] = true;
			TempData["WizardComplete"] = true;
			var customer = _context.Customer;

			ms_oLog.DebugFormat("Customer {1} ({0}): has completed wizard.", customer.Id, customer.PersonalInfo.Fullname);

			customer.WizardStep = _helper.WizardSteps.GetAll().FirstOrDefault(x => x.ID == (int)WizardStepType.AllStep);

			_session.Flush();

			ms_oLog.DebugFormat("Customer {1} ({0}): wizard step has been updated to {2}", customer.Id, customer.PersonalInfo.Fullname, (int)WizardStepType.AllStep);

			_crBuilder.CreateCashRequest(customer, CashRequestOriginator.FinishedWizard);

			ms_oLog.DebugFormat("Customer {1} ({0}): cash request created.", customer.Id, customer.PersonalInfo.Fullname);

			// Updates broker lead state if needed and sends "Email Under Review".
			m_oServiceClient.Instance.BrokerCustomerWizardComplete(customer.Id);

			ms_oLog.DebugFormat("Customer {1} ({0}): email under review started.", customer.Id, customer.PersonalInfo.Fullname);

			m_oServiceClient.Instance.MainStrategy1(_context.User.Id, _context.User.Id, NewCreditLineOption.UpdateEverythingAndApplyAutoRules, Convert.ToInt32(customer.IsAvoid));

			ms_oLog.DebugFormat("Customer {1} ({0}): main strategy started.", customer.Id, customer.PersonalInfo.Fullname);

			if (!customer.IsTest) {
				m_oServiceClient.Instance.FraudChecker(_context.User.Id, FraudMode.FullCheck);
				ms_oLog.DebugFormat("Customer {1} ({0}): fraud check started.", customer.Id, customer.PersonalInfo.Fullname);
			} // if

			_concentAgreementHelper.Save(customer, DateTime.UtcNow);
			ms_oLog.DebugFormat("Customer {1} ({0}): consent agreement saved.", customer.Id, customer.PersonalInfo.Fullname);

			return Json(new { });
		} // WizardComplete

		#endregion method WizardComplete

		#region method SaveCompany

		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public JsonResult SaveCompany(
			LimitedInfo limitedInfo,
			NonLimitedInfo nonLimitedInfo,
			CompanyAdditionalInfo companyAdditionalInfo,
			List<CustomerAddress> limitedCompanyAddress,
			List<CustomerAddress> nonLimitedCompanyAddress,
			List<DirectorModel> limitedDirectors,
			List<DirectorModel> nonLimitedDirectors,
			CompanyEmployeeCountInfo companyEmployeeCountInfo,
			CompanyInfo experianInfo
		) {
			var customer = _context.Customer;

			TypeOfBusiness nBusinessType;
			IndustryType eIndustryType;

			if (!Enum.TryParse(companyAdditionalInfo.TypeOfBusiness, true, out nBusinessType))
				return Json(new { error = "Failed to parse business type: " + companyAdditionalInfo.TypeOfBusiness });

			if (!Enum.TryParse(companyAdditionalInfo.IndustryType, true, out eIndustryType))
				return Json(new { error = "Failed to parse industry type: " + companyAdditionalInfo.IndustryType });

			string sErrorMsg = ProcessCompanyInfoTemporary(
				nBusinessType,
				companyAdditionalInfo.VatReporting,
				limitedInfo,
				nonLimitedInfo,
				companyAdditionalInfo,
				limitedCompanyAddress,
				nonLimitedCompanyAddress,
				limitedDirectors,
				nonLimitedDirectors,
				companyEmployeeCountInfo,
				experianInfo,
				customer,
				nBusinessType,
				eIndustryType
			);

			if (!string.IsNullOrWhiteSpace(sErrorMsg))
				return Json(new { error = sErrorMsg });

			if (nBusinessType != TypeOfBusiness.Entrepreneur) {
				IQueryable<Director> directors = _directorRepository.GetAll().Where(x => x.Customer.Id == customer.Id);

				foreach (Director director in directors) {
					try {
						m_oServiceClient.Instance.ExperianConsumerCheck(customer.Id, director.Id, false);
					}
					catch (Exception e) {
						ms_oLog.Error(
							"Something went pretty not so excellent while starting " +
							"an Experian consumer check for customer " + customer.Id + " and director " + director.Id + ".",
							e
						);
					} // try
				} // for each director
			} // if not Entrepreneur

			QuickOfferActionResult qoar = null;

			try {
				qoar = m_oServiceClient.Instance.QuickOfferWithPrerequisites(customer.Id, true);
			}
			catch (Exception e) {
				ms_oLog.Error("Failed to get a quick offer from the service.", e);
			} // try

			if ((qoar != null) && qoar.HasValue) {
				ms_oLog.DebugFormat("Quick offer is {0} for customer {1}.", qoar.Value.Amount, customer.Id);
				_session.Refresh(customer);
			} // if

			return Json(new { });
		}

		#endregion method SaveCompany

		#region method Save

		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public JsonResult Save(
			PersonalInfo personalInfo,
			List<CustomerAddress> personalAddress,
			List<CustomerAddress> prevPersonAddresses,
			string dateOfBirth
		) {
			SaveCustomerToDB(personalInfo, personalAddress, prevPersonAddresses, dateOfBirth);

			var customer = _context.Customer;

			try {
				m_oServiceClient.Instance.ExperianConsumerCheck(customer.Id, 0, false);
			}
			catch (Exception e) {
				ms_oLog.Error("Something went pretty not so excellent while starting an Experian consumer check for customer " + customer.Id + ".", e);
			} // try

			try {
				m_oServiceClient.Instance.CheckAml(customer.Id);
			}
			catch (Exception e) {
				ms_oLog.Error("Something went pretty not so excellent while starting an AML check for customer " + customer.Id + ".", e);
			} // try

			if (!customer.IsTest) {
				try {
					m_oServiceClient.Instance.NotifySalesOnNewCustomer(customer.Id);
				}
				catch (Exception e) {
					ms_oLog.Error("Something went pretty not so excellent while sending notification to sales for customer " + customer.Id + ".", e);
				} // try
			} // if

			return Json(new { });
		} // Save

		#endregion method Save

		#region method AddDirector

		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public JsonResult AddDirector(DirectorModel director) {
			var customer = _context.Customer;

			if (customer == null)
				return Json(new { error = "Customer not found" });

			return Json(AddDirectorToCustomer(director, customer, _session, true));
		} // AddDirector

		#endregion method AddDirector

		#region method Edit

		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		public JsonResult Edit(
			string dayTimePhone,
			string mobilePhone,
			string businessPhone,
			decimal? overallTurnOver,
			decimal? webSiteTurnOver,
			List<CustomerAddress> personalAddress,
			List<CustomerAddress> limitedCompanyAddress,
			List<CustomerAddress> nonLimitedCompanyAddress,
			List<DirectorAddressModel>[] directorAddress,
			List<CustomerAddress> otherPropertyAddress
		) {
			var customer = _context.Customer;

			var oldPersonalInfo = PersonalInfoEditHistoryParametersBuilder(customer);

			customer.PersonalInfo.DaytimePhone = dayTimePhone;
			customer.PersonalInfo.MobilePhone = mobilePhone;
			customer.PersonalInfo.OverallTurnOver = overallTurnOver;
			customer.PersonalInfo.WebSiteTurnOver = webSiteTurnOver;

			var addressInfo = customer.AddressInfo;
			MakeAddress(
				personalAddress,
				addressInfo.PrevPersonAddresses,
				CustomerAddressType.PrevPersonAddresses,
				addressInfo.PersonalAddress,
				CustomerAddressType.PersonalAddress
			);

			if (otherPropertyAddress != null) {
				MakeAddress(
					otherPropertyAddress,
					addressInfo.OtherPropertyAddress,
					CustomerAddressType.OtherPropertyAddressPrev,
					addressInfo.OtherPropertyAddress,
					CustomerAddressType.OtherPropertyAddress
				);
			} // if

			var company = customer.Company;
			if (company != null)
				company.BusinessPhone = businessPhone;

			/* TODO currently only business phone update available for company/directors
			if (customer.PersonalInfo.TypeOfBusiness.Reduce() == TypeOfBusinessReduced.Limited) {
				//customer.LimitedInfo.LimitedBusinessPhone = businessPhone;

				MakeAddress(
					limitedCompanyAddress,
					addressInfo.LimitedCompanyAddressPrev,
					CustomerAddressType.LimitedCompanyAddressPrev,
					addressInfo.LimitedCompanyAddress,
					CustomerAddressType.LimitedCompanyAddress
				);

				var directors = customer.LimitedInfo.Directors;

				if (directors.Any()) {
					foreach (var d in directors) {
						foreach (var da in directorAddress.Where(da => da.Any(x => x.DirectorId == d.Id))) {
							MakeAddress(da,
								d.DirectorAddressInfo.LimitedDirectorHomeAddressPrev,
								CustomerAddressType.LimitedDirectorHomeAddressPrev,
								d.DirectorAddressInfo.LimitedDirectorHomeAddress,
								CustomerAddressType.LimitedDirectorHomeAddress
							);
						} // foreach director address
					} // for each director
				} // if has directors
			} // if limited
			else if (customer.PersonalInfo.TypeOfBusiness.Reduce() == TypeOfBusinessReduced.NonLimited) {
				customer.NonLimitedInfo.NonLimitedBusinessPhone = businessPhone;

				MakeAddress(
					nonLimitedCompanyAddress,
					addressInfo.NonLimitedCompanyAddressPrev,
					CustomerAddressType.NonLimitedCompanyAddressPrev,
					addressInfo.NonLimitedCompanyAddress,
					CustomerAddressType.NonLimitedCompanyAddress
				);

				var directors = customer.NonLimitedInfo.Directors;

				if (directors.Any()) {
					foreach (var d in directors) {
						foreach (var da in directorAddress.Where(da => da.Any(x => x.DirectorId == d.Id))) {
							MakeAddress(
								da,
								d.DirectorAddressInfo.NonLimitedDirectorHomeAddressPrev,
								CustomerAddressType.NonLimitedDirectorHomeAddressPrev,
								d.DirectorAddressInfo.NonLimitedDirectorHomeAddress,
								CustomerAddressType.NonLimitedDirectorHomeAddress
							); // make address
						} // for each director address
					} // for each director
				} // if has directors
			} // if (non limited)
			*/
			var newPersonalInfo = PersonalInfoEditHistoryParametersBuilder(customer);

			SaveEditHistory(oldPersonalInfo, newPersonalInfo);

			return Json(new { });
		} // Edit

		#endregion method Edit

		#region method SaveEditHistory

		[NonAction]
		public void SaveEditHistory(PersonalInfoHistoryParameter oldPersonalInfo, PersonalInfoHistoryParameter newPersonalInfo) {
			var customer = _context.Customer;

			if (oldPersonalInfo.DaytimePhone != newPersonalInfo.DaytimePhone) {
				var personalInfoEditHistory = new PersonalInfoHistory {
					Customer = customer,
					FieldName = "Day time Phone",
					OldValue = oldPersonalInfo.DaytimePhone,
					NewValue = newPersonalInfo.DaytimePhone,
					DateModifed = DateTime.Now
				};

				_personalInfoHistoryRepository.SaveOrUpdate(personalInfoEditHistory);
			} // if

			if (oldPersonalInfo.MobilePhone != newPersonalInfo.MobilePhone) {
				var personalInfoEditHistory = new PersonalInfoHistory {
					Customer = customer,
					FieldName = "Mobile Phone",
					OldValue = oldPersonalInfo.MobilePhone,
					NewValue = newPersonalInfo.MobilePhone,
					DateModifed = DateTime.Now
				};
				_personalInfoHistoryRepository.SaveOrUpdate(personalInfoEditHistory);
			} // if

			if (oldPersonalInfo.BusinessPhone != newPersonalInfo.BusinessPhone) {
				var personalInfoEditHistory = new PersonalInfoHistory {
					Customer = customer,
					FieldName = "Business Phone",
					OldValue = oldPersonalInfo.BusinessPhone,
					NewValue = newPersonalInfo.BusinessPhone,
					DateModifed = DateTime.Now
				};

				_personalInfoHistoryRepository.SaveOrUpdate(personalInfoEditHistory);
			} // if

			if (oldPersonalInfo.OverallTurnOver != newPersonalInfo.OverallTurnOver) {
				var personalInfoEditHistory = new PersonalInfoHistory {
					Customer = customer,
					FieldName = "Total Estimated Sales",
					OldValue = oldPersonalInfo.OverallTurnOver,
					NewValue = newPersonalInfo.OverallTurnOver,
					DateModifed = DateTime.Now
				};

				_personalInfoHistoryRepository.SaveOrUpdate(personalInfoEditHistory);
			} // if

			if (oldPersonalInfo.WebSiteTurnover != newPersonalInfo.WebSiteTurnover) {
				var personalInfoEditHistory = new PersonalInfoHistory {
					Customer = customer,
					FieldName = "Estimated Online Sales",
					OldValue = oldPersonalInfo.WebSiteTurnover,
					NewValue = newPersonalInfo.WebSiteTurnover,
					DateModifed = DateTime.Now
				};

				_personalInfoHistoryRepository.SaveOrUpdate(personalInfoEditHistory);
			} // if

			AddAddressInfoToHistory(oldPersonalInfo.PersonalAddress, newPersonalInfo.PersonalAddress, customer, "Personal Address");
			AddAddressInfoToHistory(oldPersonalInfo.CompanyAddress, newPersonalInfo.CompanyAddress, customer, "Company Address");
		} // SaveEditHistory


		#endregion method SaveEditHistory

		#endregion public

		#region private

		#region static method UpdateAddresses

		private static void UpdateAddresses(
			Customer customer,
			IEnumerable<CustomerAddress> oNewAddressList,
			Iesi.Collections.Generic.ISet<CustomerAddress> oCurrentAddressList,
			CustomerAddressType nType,
			Action<Iesi.Collections.Generic.ISet<CustomerAddress>> fSetNewValue
		) {
			if (oNewAddressList == null) {
				if (oCurrentAddressList != null)
					oCurrentAddressList.Clear();

				return;
			} // if new list is empty, i.e. remove all the current data

			var oNewEntries = new List<CustomerAddress>();

			foreach (var val in oNewAddressList.Where(x => x.AddressId == 0)) {
				val.AddressType = nType;
				val.Customer = customer;
				oNewEntries.Add(val);
			} // foreach

			if (oNewEntries.Count > 0) {
				if (oCurrentAddressList == null)
					fSetNewValue(new HashedSet<CustomerAddress>(oNewEntries));
				else {
					oCurrentAddressList.Clear();
					oCurrentAddressList.AddAll(oNewEntries);
				} // if
			} // if
		} // UpdateAddresses

		#endregion static method UpdateAddresses

		#region static method ProcessCompanyInfoTemporary

		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		private string ProcessCompanyInfoTemporary(
			TypeOfBusiness businessType,
			string vat,
			LimitedInfo limitedInfo,
			NonLimitedInfo nonLimitedInfo,
			CompanyAdditionalInfo companyAdditionalInfo,
			List<CustomerAddress> limitedCompanyAddress,
			List<CustomerAddress> nonLimitedCompanyAddress,
			List<DirectorModel> limitedDirectors,
			List<DirectorModel> nonLimitedDirectors,
			CompanyEmployeeCountInfo companyEmployeeCount,
			CompanyInfo experianInfo, Customer customer,
			TypeOfBusiness nBusinessType,
			IndustryType eIndustryType
		) {
			if (customer.PersonalInfo == null)
				customer.PersonalInfo = new PersonalInfo();

			customer.PersonalInfo.TypeOfBusiness = nBusinessType;
			customer.PersonalInfo.IndustryType = eIndustryType;
			customer.PersonalInfo.OverallTurnOver = companyAdditionalInfo.OverallTurnOver;

			if (eIndustryType == IndustryType.Retail || eIndustryType == IndustryType.Online || companyAdditionalInfo.PartBusinessOnline)
				customer.IsOffline = false;
			else
				customer.IsOffline = true;

			customer.IsDirector = companyAdditionalInfo.DirectorCheck;

			CompanyInfoMap companyData = null;
			List<CustomerAddress> companyAddress = null;
			List<DirectorModel> companyDirectors = null;

			switch (businessType.Reduce()) {
			case TypeOfBusinessReduced.Limited:
				companyData = new CompanyInfoMap {
					BusinessPhone = limitedInfo.LimitedBusinessPhone,
					CapitalExpenditure = companyAdditionalInfo.CapitalExpenditure,
					CompanyNumber = limitedInfo.LimitedCompanyNumber,
					CompanyName = limitedInfo.LimitedCompanyName,
					PropertyOwnedByCompany = companyAdditionalInfo.PropertyOwnedByCompany,
					RentMonthLeft = companyAdditionalInfo.RentMonthsLeft,
					TypeOfBusiness = businessType,
					TimeAtAddress = limitedInfo.LimitedTimeAtAddress,
					YearsInCompany = companyAdditionalInfo.YearsInCompany,
				};

				companyAddress = limitedCompanyAddress;
				companyDirectors = limitedDirectors;
				break;

			case TypeOfBusinessReduced.NonLimited:
				companyAddress = nonLimitedCompanyAddress;
				companyDirectors = nonLimitedDirectors;
				goto case TypeOfBusinessReduced.Personal;

			case TypeOfBusinessReduced.Personal:
				companyData = new CompanyInfoMap {
					BusinessPhone = nonLimitedInfo.NonLimitedBusinessPhone,
					CapitalExpenditure = companyAdditionalInfo.CapitalExpenditure,
					CompanyName = nonLimitedInfo.NonLimitedCompanyName,
					PropertyOwnedByCompany = companyAdditionalInfo.PropertyOwnedByCompany,
					RentMonthLeft = companyAdditionalInfo.RentMonthsLeft,
					TypeOfBusiness = businessType,
					TimeAtAddress = nonLimitedInfo.NonLimitedTimeAtAddress,
					YearsInCompany = companyAdditionalInfo.YearsInCompany,
					TimeInBusiness = nonLimitedInfo.NonLimitedTimeInBusiness,
				};

				break;
			} // switch

			if (!ReferenceEquals(companyData, null)) {
				var experianAddress = new List<CustomerAddress> {
					new CustomerAddress {
						AddressType = CustomerAddressType.ExperianCompanyAddress,
						Organisation = companyAdditionalInfo.ExperianCompanyName,
						Line1 = companyAdditionalInfo.ExperianCompanyAddrLine1,
						Line2 = companyAdditionalInfo.ExperianCompanyAddrLine2,
						Line3 = companyAdditionalInfo.ExperianCompanyAddrLine3,
						Town = companyAdditionalInfo.ExperianCompanyAddrLine4,
						Postcode = companyAdditionalInfo.ExperianCompanyPostcode,
					}
				};

				companyData.VatReporting = vat != null ? (VatReporting?)Enum.Parse(typeof(VatReporting), vat) : null;

				string sErrorMsg = ProcessCompanyInfo(companyData, companyAddress, experianAddress, companyDirectors, companyEmployeeCount, experianInfo, customer);

				if (!string.IsNullOrWhiteSpace(sErrorMsg))
					return sErrorMsg;
			} // if

			customer.WizardStep = _helper.WizardSteps.GetAll().FirstOrDefault(x => x.ID == (int)WizardStepType.CompanyDetails);
			_session.Flush();

			ms_oLog.DebugFormat(
				"Customer {1} ({0}): wizard step has been updated to {2}",
				customer.Id, customer.PersonalInfo.Fullname, (int)WizardStepType.CompanyDetails
			);

			return null;
		} // ProcessCompanyInfoTemporary

		#endregion static method ProcessCompanyInfoTemporary

		#region static method ProcessCompanyInfo

		private static string ProcessCompanyInfo(
			CompanyInfoMap companyData,
			ICollection<CustomerAddress> companyAddress,
			ICollection<CustomerAddress> experianCompanyAddress,
			IEnumerable<DirectorModel> directors,
			CompanyEmployeeCountInfo companyEmployeeCount,
			CompanyInfo experianInfo,
			Customer customer
		) {
			Company company = new Company();

			company.ExperianCompanyName = experianInfo.BusName;
			company.ExperianRefNum = experianInfo.BusRefNum == "skip" ? "NotFound" : experianInfo.BusRefNum;
			company.TypeOfBusiness = companyData.TypeOfBusiness;
			company.CompanyName = companyData.CompanyName;
			company.CompanyNumber = companyData.CompanyNumber;
			company.TimeAtAddress = companyData.TimeAtAddress;
			company.TimeInBusiness = companyData.TimeInBusiness;
			company.BusinessPhone = companyData.BusinessPhone;
			company.PropertyOwnedByCompany = companyData.PropertyOwnedByCompany;
			company.YearsInCompany = companyData.YearsInCompany;
			company.RentMonthLeft = companyData.RentMonthLeft;
			company.CapitalExpenditure = companyData.CapitalExpenditure;
			company.VatReporting = companyData.VatReporting;

			customer.Company = company;

			if (directors != null) {
				company.Directors.AddAll(
					directors.Select(d => {
						var dir = d.FromModel();

						if (dir != null) {
							if (dir.DirectorAddressInfo != null && dir.DirectorAddressInfo.AllAddresses != null) {
								foreach (var address in dir.DirectorAddressInfo.AllAddresses) {
									address.AddressType = CustomerAddressType.LimitedDirectorHomeAddress;
									address.Director = dir;
								} // foreach
							} // if

							dir.Customer = customer;
							dir.Company = company;
						} // if

						return dir;
					})
					.Where(d => d != null).ToList()
				); // AddAll

				var oKeys = new System.Collections.Generic.SortedSet<string>();

				oKeys.Add(DetailsToKey(customer));

				foreach (Director oDir in company.Directors) {
					string sKey = DetailsToKey(oDir);

					if (oKeys.Contains(sKey))
						return "Duplicate director detected.";

					oKeys.Add(sKey);
				} // for each director
			} // if

			if (companyAddress != null) {
				foreach (var val in companyAddress) {
					val.AddressType = CustomerAddressType.LimitedCompanyAddress; //TODO
					val.Customer = customer;
					val.Company = company;
				} // foreach

				company.CompanyAddress = new HashedSet<CustomerAddress>(companyAddress);
			} // if

			if (experianInfo != null) {
				experianCompanyAddress = new Collection<CustomerAddress>
					{
						new CustomerAddress
							{
								Line1 = experianInfo.AddrLine1,
								Line2 = experianInfo.AddrLine2,
								Line3 = experianInfo.AddrLine3,
								Town = experianInfo.AddrLine4,
								Postcode = experianInfo.PostCode
							}
					};
				foreach (var val in experianCompanyAddress) {
					val.AddressType = CustomerAddressType.ExperianCompanyAddress;
					val.Customer = customer;
					val.Company = company;
				} // foreach

				company.ExperianCompanyAddress = new HashedSet<CustomerAddress>(experianCompanyAddress);
			}

			company.CompanyEmployeeCount.Add(new CompanyEmployeeCount {
				BottomEarningEmployeeCount = companyEmployeeCount.BottomEarningEmployeeCount,
				Created = DateTime.UtcNow,
				Customer = customer,
				EmployeeCount = companyEmployeeCount.EmployeeCount,
				EmployeeCountChange = companyEmployeeCount.EmployeeCountChange,
				TopEarningEmployeeCount = companyEmployeeCount.TopEarningEmployeeCount,
				TotalMonthlySalary = companyEmployeeCount.TotalMonthlySalary,
				Company = company
			});

			return null;
		} // ProcessCompanyInfo

		#endregion static method ProcessCompanyInfo

		#region method SaveCustomerToDB

		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		private void SaveCustomerToDB(
			PersonalInfo personalInfo,
			IEnumerable<CustomerAddress> personalAddress,
			IEnumerable<CustomerAddress> prevPersonAddresses,
			string dateOfBirth
		) {
			var customer = _context.Customer;

			if (personalInfo == null)
				throw new ArgumentNullException("personalInfo");

			if (string.IsNullOrEmpty(personalInfo.FirstName))
				throw new ArgumentNullException("personalInfo." + "FirstName");

			if (string.IsNullOrEmpty(personalInfo.Surname))
				throw new ArgumentNullException("personalInfo." + "Surname");

			personalInfo.DateOfBirth = DateTime.ParseExact(dateOfBirth, "d/M/yyyy", CultureInfo.InvariantCulture);

			customer.ConsentToSearch = personalInfo.ConsentToSearch;

			if (customer.PersonalInfo != null)
				personalInfo.OverallTurnOver = customer.PersonalInfo.OverallTurnOver;

			customer.PersonalInfo = personalInfo;

			personalInfo.Fullname = string.Format("{0} {1} {2}", personalInfo.FirstName, personalInfo.Surname, personalInfo.MiddleInitial).Trim();

			UpdateAddresses(
				customer, personalAddress, customer.AddressInfo.PersonalAddress,
				CustomerAddressType.PersonalAddress,
				lst => customer.AddressInfo.PersonalAddress = lst
			);

			UpdateAddresses(
				customer, prevPersonAddresses, customer.AddressInfo.PrevPersonAddresses,
				CustomerAddressType.PrevPersonAddresses,
				lst => customer.AddressInfo.PrevPersonAddresses = lst
			);

			customer.WizardStep = _helper.WizardSteps.GetAll().FirstOrDefault(x => x.ID == (int)WizardStepType.PersonalDetails);
			_session.Flush();

			ms_oLog.DebugFormat("Customer {1} ({0}): wizard step has been updated to {2}", customer.Id, customer.PersonalInfo.Fullname, (int)WizardStepType.PersonalDetails);
		} // SaveCustomerToDB

		#endregion method SaveCustomerToDB

		#region method MakeAddress

		private void MakeAddress(
			IEnumerable<CustomerAddress> newAddress,
			Iesi.Collections.Generic.ISet<CustomerAddress> prevAddress,
			CustomerAddressType prevAddressType,
			Iesi.Collections.Generic.ISet<CustomerAddress> currentAddress,
			CustomerAddressType currentAddressType
		) {
			var newAddresses = newAddress as IList<CustomerAddress> ?? newAddress.ToList();
			var addAddress = newAddresses.Where(i => i.AddressId == 0).ToList();
			var curAddress = addAddress.LastOrDefault() ?? currentAddress.LastOrDefault();

			if (curAddress == null)
				return;

			foreach (var address in newAddresses) {
				address.Director = currentAddress.First().Director;
				address.Customer = currentAddress.First().Customer;
				address.Company = currentAddress.First().Company;
			} // for each new address

			foreach (var address in currentAddress)
				address.AddressType = prevAddressType;

			foreach (var item in addAddress.Where(a => a.Id != curAddress.Id)) {
				item.AddressType = prevAddressType;
				prevAddress.Add(item);
			} // for each old address

			curAddress.AddressType = currentAddressType;
			currentAddress.Clear();
			currentAddress.Add(curAddress);
		} // MakeAddress

		#endregion method MakeAddress

		#region method AddAddressInfoToHistory

		private void AddAddressInfoToHistory(
			IEnumerable<CustomerAddress> oldAddress,
			IList<CustomerAddress> newAddress,
			Customer customer,
			string fieldName
		) {
			if (oldAddress == null || newAddress == null)
				return;

			var addedAddress = newAddress.Where(n => oldAddress.All(t => t.Id != n.Id)).ToList();
			var removeAddress = oldAddress.Where(n => newAddress.All(t => t.Id != n.Id)).ToList();

			foreach (var customerAddress in removeAddress) {
				var personalInfoEditHistory = new PersonalInfoHistory {
					Customer = customer,
					FieldName = fieldName,
					OldValue = customerAddress.Id,
					NewValue = string.Empty,
					DateModifed = DateTime.Now
				};

				_personalInfoHistoryRepository.SaveOrUpdate(personalInfoEditHistory);
			} // for each removed address

			foreach (var customerAddress in addedAddress) {
				var personalInfoEditHistory = new PersonalInfoHistory {
					Customer = customer,
					FieldName = fieldName,
					OldValue = string.Empty,
					NewValue = customerAddress.Id,
					DateModifed = DateTime.Now,
					AddressId = customerAddress.Id

				};

				_personalInfoHistoryRepository.SaveOrUpdate(personalInfoEditHistory);
			} // for each added address
		} // AddressInfoToHistory

		#endregion method AddAddressInfoToHistory

		#region method PersonalInfoEditHistoryParametersBuilder

		private PersonalInfoHistoryParameter PersonalInfoEditHistoryParametersBuilder(Customer customer) {
			string businessPhone = "";
			IList<CustomerAddress> companyAddress = null;
			var company = customer.Company;
			if (company != null) {
				businessPhone = company.BusinessPhone;
				companyAddress = company.CompanyAddress.ToList();
			}

			var personalInfo = new PersonalInfoHistoryParameter {
				DaytimePhone = customer.PersonalInfo.DaytimePhone,
				MobilePhone = customer.PersonalInfo.MobilePhone,
				BusinessPhone = businessPhone,
				PersonalAddress = customer.AddressInfo.PersonalAddress.ToList(),
				OverallTurnOver = customer.PersonalInfo.OverallTurnOver.ToString(),
				WebSiteTurnover = customer.PersonalInfo.WebSiteTurnOver.ToString()
			};

			if (companyAddress != null)
				personalInfo.CompanyAddress = companyAddress.ToList();

			return personalInfo;
		} // PersonalInfoEditHistoryParametersBuilder

		#endregion method PersonalInfoEditHistoryParametersBuilder

		#region method DetailsToKey

		private static string DetailsToKey(Director oDir) {
			return DetailsToKey(
				oDir.Name,
				oDir.Surname,
				oDir.DateOfBirth,
				oDir.Gender,
				oDir.DirectorAddressInfo.AllAddresses.First().Rawpostcode
			);
		} // DetailsToKey

		private static string DetailsToKey(Customer customer) {
			return DetailsToKey(
				customer.PersonalInfo.FirstName,
				customer.PersonalInfo.Surname,
				customer.PersonalInfo.DateOfBirth,
				customer.PersonalInfo.Gender,
				customer.AddressInfo.PersonalAddress.First().Rawpostcode
			);
		} // DetailsToKey

		private static string DetailsToKey(string sFirstName, string sLastName, DateTime? oBirthDate, Gender nGender, string sPostCode) {
			string sBirthDate = oBirthDate.HasValue
				? oBirthDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
				: string.Empty;

			return string.Format("f:{0}|l:{1}|b:{2}|g:{3}|p:{4}", sFirstName, sLastName, sBirthDate, nGender, sPostCode);
		} // DetailsToKey

		#endregion method DetailsToKey

		#region private properties

		private readonly IEzbobWorkplaceContext _context;
		private readonly IPersonalInfoHistoryRepository _personalInfoHistoryRepository;
		private readonly ServiceClient m_oServiceClient;
		private readonly ISession _session;
		private readonly CashRequestBuilder _crBuilder;
		private readonly IConcentAgreementHelper _concentAgreementHelper = new ConcentAgreementHelper();
		private readonly DatabaseDataHelper _helper;
		private readonly DirectorRepository _directorRepository;
		private static readonly ILog ms_oLog = LogManager.GetLogger(typeof(CustomerDetailsController));

		#endregion private properties

		#endregion private
	} // class CustomerDetailsController
} // namespace EzBob.Web.Areas.Customer.Controllers
