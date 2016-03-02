namespace EzBob.Web.Areas.Customer.Controllers {
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Globalization;
	using System.Linq;
	using System.Security.Principal;
	using System.Web.Mvc;
	using System.Web.Security;
	using DbConstants;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EzBob.Models;
	using Ezbob.Backend.Models;
	using Ezbob.Logger;
	using Infrastructure.Attributes;
	using Models;
	using Code;
	using Infrastructure;
	using Infrastructure.csrf;
	using Iesi.Collections.Generic;
	using NHibernate;
	using ServiceClientProxy;
	using ServiceClientProxy.EzServiceReference;

	public class CustomerDetailsController : Controller {
		public CustomerDetailsController(
			IEzbobWorkplaceContext oContext,
			DatabaseDataHelper oDatabaseHelper,
			IPersonalInfoHistoryRepository oPersonalInfoHistoryRepository,
			ISession oSession,
			CashRequestBuilder oCashRequestBuilder,
			PropertyStatusRepository propertyStatusRepository,
			CustomerPhoneRepository customerPhoneRepository,
			CustomerAddressRepository customerAddressRepository,
			CustomerRepository customerRepository
		) {
			this.context = oContext;
			this.databaseHelper = oDatabaseHelper;
			this.personalInfoHistoryRepository = oPersonalInfoHistoryRepository;
			this.serviceClient = new ServiceClient();
			this.session = oSession;
			this.cashRequestBuilder = oCashRequestBuilder;
			this.propertyStatusRepository = propertyStatusRepository;
			this.customerPhoneRepository = customerPhoneRepository;
			this.customerAddressRepository = customerAddressRepository;
			this.customerRepository = customerRepository;
		} // constructor

        public static object AddDirectorToCustomer(DirectorModel director, Customer customer, ISession session, bool bFailOnDuplicate, int userId)
        {
			if (customer.Company == null)
				return new { error = "Customer doesn't have a company." + customer.Id };

			ms_oLog.Debug("Adding a director {0} to customer {1}.", director, customer.Stringify());

			Director dbDirector = director.FromModel();

			if (dbDirector == null)
				return new { error = "Failed to parse director data." };

			string sDirKey = DetailsToKey(dbDirector);

			if (sDirKey == DetailsToKey(customer))
				if (bFailOnDuplicate)
					return new { error = "This Director already added." };

			dbDirector.Customer = customer;
			dbDirector.Company = customer.Company;
            dbDirector.UserId = userId;
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
							return new { error = "This director already added." };
				} // for each existing director
			} // if customer has director(s)

			customer.Company.Directors.Add(dbDirector);

			session.Flush();

			return new { success = true };
		} // AddDirectorToCustomer

		[HttpGet]
		public System.Web.Mvc.ActionResult Dashboard() {
			WizardComplete();

			var blm = new WizardBrokerLeadModel(Session);

			if (blm.BrokerFillsForCustomer) {
				StringActionResult sar = null;

				try {
					sar = this.serviceClient.Instance.BrokerBackFromCustomerWizard(blm.LeadID);
				}
				catch (Exception e) {
					ms_oLog.Warn(e, "Failed to retrieve broker details, falling back to customer's dashboard.");
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

		[Transactional]
		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public JsonResult OldTakeQuickOffer() {
			var customer = this.context.Customer;

			Session["WizardComplete"] = false;
			TempData["WizardComplete"] = false;

			ms_oLog.Debug("Customer {1} ({0}): has completed wizard by taking a quick offer.", customer.Id, customer.PersonalInfo.Fullname);

			customer.WizardStep = this.databaseHelper.WizardSteps.GetAll().FirstOrDefault(x => x.ID == (int)WizardStepType.AllStep);

			this.session.Flush();

			ms_oLog.Debug("Customer {1} ({0}): wizard step has been updated to {2}", customer.Id, customer.PersonalInfo.Fullname, (int)WizardStepType.AllStep);

			this.cashRequestBuilder.CreateQuickOfferCashRequest(customer, this.context.UserId);

			ms_oLog.Debug("Customer {1} ({0}): cash request created.", customer.Id, customer.PersonalInfo.Fullname);

			this.m_oConcentAgreementHelper.Save(customer, DateTime.UtcNow);

			ms_oLog.Debug("Customer {1} ({0}): consent agreement saved.", customer.Id, customer.PersonalInfo.Fullname);

			return Json(new { });
		} // TakeQuickOffer

		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public JsonResult TakeQuickOffer() {
			var customer = this.context.Customer;
			ms_oLog.Debug("Customer {1} ({0}): TakeNewQuickOffer has completed wizard by taking a quick offer.", customer.Id, customer.PersonalInfo.Fullname);
			Session["WizardComplete"] = false;
			TempData["WizardComplete"] = false;

			new Transactional(() => {
				ms_oLog.Debug("Customer {1} ({0}): has completed wizard by taking a quick offer.", customer.Id, customer.PersonalInfo.Fullname);

				customer.WizardStep = this.databaseHelper.WizardSteps.GetAll()
					.FirstOrDefault(x => x.ID == (int)WizardStepType.AllStep);

				//TODO remove
				customer.QuickOffer = new QuickOffer {
					Amount = 10000,
					ImmediateInterestRate = 0.02M,
					ImmediateSetupFee = 0.05M,
					CreationDate = DateTime.UtcNow,
					ExpirationDate = DateTime.UtcNow.AddHours(24),
					IncorporationDate = new DateTime(2010,01,01)

				};
				ms_oLog.Debug("Customer {1} ({0}): wizard step has been updated to {2}", customer.Id, customer.PersonalInfo.Fullname, (int)WizardStepType.AllStep);

				this.cashRequestBuilder.CreateQuickOfferCashRequest(customer, this.context.UserId);

				ms_oLog.Debug("Customer {1} ({0}): cash request created.", customer.Id, customer.PersonalInfo.Fullname);

				this.m_oConcentAgreementHelper.Save(customer, DateTime.UtcNow);

				ms_oLog.Debug("Customer {1} ({0}): consent agreement saved.", customer.Id, customer.PersonalInfo.Fullname);
			}).Execute();

			return Json(new { redirectUrl = Url.Action("Index", "Profile", new { Area = "Customer" }) + "#GetCash" });
		} // TakeQuickOffer


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
			CompanyInfo experianInfo,
			string promoCode
		) {
			var customer = this.context.Customer;
			customer.PromoCode = promoCode;

			EZBob.DatabaseLib.Model.Database.TypeOfBusiness nBusinessType;
			IndustryType eIndustryType;

			if (!Enum.TryParse(companyAdditionalInfo.TypeOfBusiness, true, out nBusinessType))
				return Json(new { error = "Failed to parse business type: " + companyAdditionalInfo.TypeOfBusiness });

			if (!Enum.TryParse(companyAdditionalInfo.IndustryType, true, out eIndustryType))
				return Json(new { error = "Failed to parse industry type: " + companyAdditionalInfo.IndustryType });

			string sErrorMsg = null;

			new Transactional(() => {
				sErrorMsg = ProcessCompanyInfoTemporary(
					nBusinessType,
					companyAdditionalInfo.VatRegistered,
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
			}).Execute();

			if (!string.IsNullOrWhiteSpace(sErrorMsg))
				return Json(new { error = sErrorMsg });
			
			this.serviceClient.Instance.SalesForceAddUpdateLeadAccount(customer.Id, customer.Name, customer.Id, false, false);

			if (nBusinessType != EZBob.DatabaseLib.Model.Database.TypeOfBusiness.Entrepreneur && customer.Company != null) {
				var directors = customer.Company.Directors.Where(x => !x.IsDeleted);
				foreach (var director in directors) {
					try {
						this.serviceClient.Instance.ExperianConsumerCheck(1, customer.Id, director.Id, false);
					}
					catch (Exception e) {
						ms_oLog.Error(e,
							"Something went pretty not so excellent while starting " +
							"an Experian consumer check for customer {0} and director {1}.",
							customer.Id, director.Id
						);
					} // try
				} // for each director
			} // if not Entrepreneur

			QuickOfferActionResult qoar = null;

			try {
				qoar = this.serviceClient.Instance.QuickOfferWithPrerequisites(customer.Id, true);
			}
			catch (Exception e) {
				ms_oLog.Error(e, "Failed to get a quick offer from the service.");
			} // try

			if ((qoar != null) && qoar.HasValue) {
				ms_oLog.Debug("Quick offer is {0} for customer {1}.", qoar.Value.Amount, customer.Id);
				this.session.Refresh(customer);
			} // if

            // Data Sharing with Alibaba 0001 - end of step 3
		    if (customer.IsAlibaba) {
		        this.serviceClient.Instance.DataSharing(customer.Id, AlibabaBusinessType.APPLICATION_WS_3, null);
		    }

		    return Json(new { });
		}

		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public JsonResult Save(
			PersonalInfo personalInfo,
			List<CustomerAddress> personalAddress,
			List<CustomerAddress> prevPersonAddresses,
			List<CustomerAddress> otherPropertiesAddresses,
			string dateOfBirth) {
			new Transactional(
				() => SaveCustomerToDB(personalInfo, personalAddress, prevPersonAddresses, otherPropertiesAddresses, dateOfBirth)
			).Execute();

			var customer = this.context.Customer;

			try {
				this.serviceClient.Instance.ExperianConsumerCheck(1, customer.Id, null, false);
			}
			catch (Exception e) {
				ms_oLog.Error(e, "Something went pretty not so excellent while starting an Experian consumer check for customer {0}.", customer.Id);
			} // try

			try {
				this.serviceClient.Instance.CheckAml(customer.Id, 1);
			}
			catch (Exception e) {
				ms_oLog.Error(e, "Something went pretty not so excellent while starting an AML check for customer {0}.", +customer.Id);
			} // try

			this.serviceClient.Instance.SalesForceAddUpdateLeadAccount(customer.Id, customer.Name, customer.Id, false, false);

			if (!customer.IsTest) {
				try {
					this.serviceClient.Instance.NotifySalesOnNewCustomer(customer.Id);
				}
				catch (Exception e) {
					ms_oLog.Error(e, "Something went pretty not so excellent while sending notification to sales for customer {0}.", customer.Id);
				} // try
			} // if

			return Json(new { });
		} // Save

		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public JsonResult AddDirector(DirectorModel director) {
			var customer = this.context.Customer;

			if (customer == null)
				return Json(new { error = "Customer not found" });
            var response = AddDirectorToCustomer(director, customer, this.session, true, this.context.UserId);
			this.serviceClient.Instance.SalesForceAddUpdateContact(customer.Id, customer.Id, null, director.Email); 
			return Json(response);
		} // AddDirector

		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		[Transactional]
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
			List<CustomerAddress> otherPropertiesAddresses
		) {
			var customer = this.context.Customer;

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

			var toBeRemoved = new List<CustomerAddress>();
			foreach (CustomerAddress previouslyDefinedOtherProperty in customer.AddressInfo.OtherPropertiesAddresses)
			{
				bool isInNewList = otherPropertiesAddresses != null && otherPropertiesAddresses.Any(newlyDefinedOtherProperty => newlyDefinedOtherProperty.AddressId == previouslyDefinedOtherProperty.AddressId);

				if (!isInNewList)
				{
					toBeRemoved.Add(previouslyDefinedOtherProperty);
				}
			}

			foreach (CustomerAddress removedAddress in toBeRemoved)
			{
				customer.AddressInfo.OtherPropertiesAddresses.Remove(removedAddress);
				removedAddress.AddressType = CustomerAddressType.OtherPropertyAddressRemoved;
			}

			if (otherPropertiesAddresses != null)
			{
				foreach (CustomerAddress otherPropertyAddress in otherPropertiesAddresses)
				{
					if (otherPropertyAddress.AddressId == 0)
					{
						otherPropertyAddress.AddressType = CustomerAddressType.OtherPropertyAddress;
						otherPropertyAddress.Customer = customer;
						this.customerAddressRepository.SaveOrUpdate(otherPropertyAddress);
						customer.AddressInfo.OtherPropertiesAddresses.Add(otherPropertyAddress);
					}
				}
			}

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

			this.serviceClient.Instance.SalesForceAddUpdateLeadAccount(customer.Id, customer.Name, customer.Id, false, false);
			this.serviceClient.Instance.SalesForceAddUpdateContact(customer.Id, customer.Id, null, null);

			return Json(new { });
		} // Edit

		[NonAction]
		public void SaveEditHistory(PersonalInfoHistoryParameter oldPersonalInfo, PersonalInfoHistoryParameter newPersonalInfo) {
			var customer = this.context.Customer;

			if (oldPersonalInfo.DaytimePhone != newPersonalInfo.DaytimePhone) {
				var personalInfoEditHistory = new PersonalInfoHistory {
					Customer = customer,
					FieldName = "Day time Phone",
					OldValue = oldPersonalInfo.DaytimePhone,
					NewValue = newPersonalInfo.DaytimePhone,
					DateModifed = DateTime.Now
				};

				this.personalInfoHistoryRepository.SaveOrUpdate(personalInfoEditHistory);
			} // if

			if (oldPersonalInfo.MobilePhone != newPersonalInfo.MobilePhone) {
				var personalInfoEditHistory = new PersonalInfoHistory {
					Customer = customer,
					FieldName = "Mobile Phone",
					OldValue = oldPersonalInfo.MobilePhone,
					NewValue = newPersonalInfo.MobilePhone,
					DateModifed = DateTime.Now
				};
				this.personalInfoHistoryRepository.SaveOrUpdate(personalInfoEditHistory);
			} // if

			if (oldPersonalInfo.BusinessPhone != newPersonalInfo.BusinessPhone) {
				var personalInfoEditHistory = new PersonalInfoHistory {
					Customer = customer,
					FieldName = "Business Phone",
					OldValue = oldPersonalInfo.BusinessPhone,
					NewValue = newPersonalInfo.BusinessPhone,
					DateModifed = DateTime.Now
				};

				this.personalInfoHistoryRepository.SaveOrUpdate(personalInfoEditHistory);
			} // if

			if (oldPersonalInfo.OverallTurnOver != newPersonalInfo.OverallTurnOver) {
				var personalInfoEditHistory = new PersonalInfoHistory {
					Customer = customer,
					FieldName = "Total Estimated Sales",
					OldValue = oldPersonalInfo.OverallTurnOver,
					NewValue = newPersonalInfo.OverallTurnOver,
					DateModifed = DateTime.Now
				};

				this.personalInfoHistoryRepository.SaveOrUpdate(personalInfoEditHistory);
			} // if

			if (oldPersonalInfo.WebSiteTurnover != newPersonalInfo.WebSiteTurnover) {
				var personalInfoEditHistory = new PersonalInfoHistory {
					Customer = customer,
					FieldName = "Estimated Online Sales",
					OldValue = oldPersonalInfo.WebSiteTurnover,
					NewValue = newPersonalInfo.WebSiteTurnover,
					DateModifed = DateTime.Now
				};

				this.personalInfoHistoryRepository.SaveOrUpdate(personalInfoEditHistory);
			} // if

			AddAddressInfoToHistory(oldPersonalInfo.PersonalAddress, newPersonalInfo.PersonalAddress, customer, "Personal Address");
			AddAddressInfoToHistory(oldPersonalInfo.CompanyAddress, newPersonalInfo.CompanyAddress, customer, "Company Address");
			AddAddressInfoToHistory(oldPersonalInfo.OtherPropertiesAddresses, newPersonalInfo.OtherPropertiesAddresses, customer, "Other Properties Addresses");
		} // SaveEditHistory

		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public JsonResult SaveExperianDirector(
			int directorID,
			string email,
			string mobilePhone,
			string line1,
			string line2,
			string line3,
			string town,
			string county,
			string postcode
		) {
			ms_oLog.Debug("Saving Experian director (CustomerDetails controller): {0}: {1} {2}, {3} {4} {5} {6} {7} {8}",
				directorID,
				email,
				mobilePhone,
				line1,
				line2,
				line3,
				town,
				county,
				postcode
			);

			var m = new Esigner {
				DirectorID = directorID,
				Email = (email ?? string.Empty).Trim(),
				MobilePhone = (mobilePhone ?? string.Empty).Trim(),
				Line1 = (line1 ?? string.Empty).Trim(),
				Line2 = (line2 ?? string.Empty).Trim(),
				Line3 = (line3 ?? string.Empty).Trim(),
				Town = (town ?? string.Empty).Trim(),
				County = (county ?? string.Empty).Trim(),
				Postcode = (postcode ?? string.Empty).Trim(),
			};

			string sValidation = m.ValidateExperianDirectorDetails();

			if (!string.IsNullOrWhiteSpace(sValidation))
				return Json(new { success = false, error = sValidation, });

			try {
				this.serviceClient.Instance.UpdateExperianDirectorDetails(null, this.context.UserId, m);
			}
			catch (Exception e) {
				ms_oLog.Warn(e, "Failed to save experian director details.");
				return Json(new { success = false, error = string.Empty, });
			} // try

			return Json(new { success = true, error = string.Empty, });
		} // SaveExperianDirector

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

		private string ProcessCompanyInfoTemporary(
			EZBob.DatabaseLib.Model.Database.TypeOfBusiness businessType,
			bool vatRegistered,
			LimitedInfo limitedInfo,
			NonLimitedInfo nonLimitedInfo,
			CompanyAdditionalInfo companyAdditionalInfo,
			List<CustomerAddress> limitedCompanyAddress,
			List<CustomerAddress> nonLimitedCompanyAddress,
			List<DirectorModel> limitedDirectors,
			List<DirectorModel> nonLimitedDirectors,
			CompanyEmployeeCountInfo companyEmployeeCount,
			CompanyInfo experianInfo, Customer customer,
			EZBob.DatabaseLib.Model.Database.TypeOfBusiness nBusinessType,
			IndustryType eIndustryType
		) {
			if (customer.PersonalInfo == null)
				customer.PersonalInfo = new PersonalInfo();

			customer.PersonalInfo.TypeOfBusiness = nBusinessType;
			customer.PersonalInfo.IndustryType = eIndustryType;
			customer.PersonalInfo.OverallTurnOver = companyAdditionalInfo.OverallTurnOver;
			customer.Turnovers.Add(new CustomerTurnover {
				CustomerID = customer.Id,
				Timestamp = DateTime.UtcNow,
				Turnover = companyAdditionalInfo.OverallTurnOver ?? 0,
			});
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
						Postcode = companyAdditionalInfo.ExperianCompanyPostcode
					}
				};

				companyData.VatRegistered = vatRegistered;

				string sErrorMsg = ProcessCompanyInfo(companyData, companyAddress, experianAddress, companyDirectors, companyEmployeeCount, experianInfo, customer);

				if (!string.IsNullOrWhiteSpace(sErrorMsg))
					return sErrorMsg;
			} // if

			customer.WizardStep = this.databaseHelper.WizardSteps.GetAll().FirstOrDefault(x => x.ID == (int)WizardStepType.CompanyDetails);
			this.session.Flush();

			ms_oLog.Debug(
				"Customer {1} ({0}): wizard step has been updated to {2}",
				customer.Id, customer.PersonalInfo.Fullname, (int)WizardStepType.CompanyDetails
			);

			return null;
		} // ProcessCompanyInfoTemporary

		private static string ProcessCompanyInfo(
			CompanyInfoMap companyData,
			ICollection<CustomerAddress> companyAddress,
			ICollection<CustomerAddress> experianCompanyAddress,
			IEnumerable<DirectorModel> directors,
			CompanyEmployeeCountInfo companyEmployeeCount,
			CompanyInfo experianInfo,
			Customer customer
		) {
			var company = new Company {
				ExperianCompanyName = experianInfo.BusName,
				ExperianRefNum = experianInfo.BusRefNum == "skip" ? "NotFound" : experianInfo.BusRefNum,
				TypeOfBusiness = companyData.TypeOfBusiness,
				CompanyName = companyData.CompanyName,
				CompanyNumber = companyData.CompanyNumber,
				TimeAtAddress = companyData.TimeAtAddress,
				TimeInBusiness = companyData.TimeInBusiness,
				BusinessPhone = companyData.BusinessPhone,
				PropertyOwnedByCompany = companyData.PropertyOwnedByCompany,
				YearsInCompany = companyData.YearsInCompany,
				RentMonthLeft = companyData.RentMonthLeft,
				CapitalExpenditure = companyData.CapitalExpenditure,
				VatReporting = companyData.VatReporting,
				VatRegistered = companyData.VatRegistered
			};

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
								} // for each
							} // if

							dir.Customer = customer;
							dir.Company = company;
						} // if

						return dir;
					})
					.Where(d => d != null).ToList()
				); // AddAll

				var oKeys = new System.Collections.Generic.SortedSet<string> {
					DetailsToKey(customer)
				};

				foreach (Director oDir in company.Directors) {

					string sKey = DetailsToKey(oDir);

					if (oKeys.Contains(sKey))
						return "Duplicate director detected.";

					oKeys.Add(sKey);
				} // for each director
			} // if

			if (companyAddress != null) {
				foreach (var val in companyAddress) {
					val.AddressType = CustomerAddressType.LimitedCompanyAddress; //TODO set proper address type?
					val.Customer = customer;
					val.Company = company;
				} // for each

				company.CompanyAddress = new HashedSet<CustomerAddress>(companyAddress);
			} // if

			if (experianInfo != null) {
				experianCompanyAddress = new Collection<CustomerAddress> {
					new CustomerAddress {
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
				} // for each

				company.ExperianCompanyAddress = new HashedSet<CustomerAddress>(experianCompanyAddress);
			} // if

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

		private void WizardComplete() {
			Session["WizardComplete"] = true;
			TempData["WizardComplete"] = true;

			var customer = this.context.Customer;

			ms_oLog.Debug("Customer {1} ({0}): has completed wizard.", customer.Id, customer.PersonalInfo.Fullname);

			new Transactional(() => {
				customer.WizardStep = this.databaseHelper.WizardSteps.GetAll().FirstOrDefault(x => x.ID == (int)WizardStepType.AllStep);
				this.customerRepository.SaveOrUpdate(customer);
				this.session.Flush();
				ms_oLog.Debug("Customer {1} ({0}): wizard step has been updated to {2}", customer.Id, customer.PersonalInfo.Fullname, (int)WizardStepType.AllStep);

				this.m_oConcentAgreementHelper.Save(customer, DateTime.UtcNow);
				ms_oLog.Debug("Customer {1} ({0}): consent agreement saved.", customer.Id, customer.PersonalInfo.Fullname);

				customer.AddAlibabaDefaultBankAccount();
			}).Execute();

			// Updates broker lead state if needed and sends "Email Under Review".
			this.serviceClient.Instance.BrokerCustomerWizardComplete(customer.Id);

			this.serviceClient.Instance.SalesForceAddUpdateLeadAccount(customer.Id, customer.Name, customer.Id, false, false);

			if (customer.Company != null && customer.Company.Directors.Any()) {
				foreach (Director director in customer.Company.Directors) {
					this.serviceClient.Instance.SalesForceAddUpdateContact(customer.Id, customer.Id, director.Id, director.Email);
				}
			}

			ms_oLog.Debug("Customer {1} ({0}): email under review started.", customer.Id, customer.PersonalInfo.Fullname);

			// finish wizard => Main strategy runs Alibaba 001 ("data sharing") full info
			new MainStrategyClient(
				customer.Id,
				customer.Id,
				customer.IsAvoid,
				NewCreditLineOption.UpdateEverythingAndApplyAutoRules,
				null,
				EZBob.DatabaseLib.Model.Database.CashRequestOriginator.FinishedWizard
			).ExecuteAsync();

			ms_oLog.Debug("Customer {1} ({0}): main strategy started.", customer.Id, customer.PersonalInfo.Fullname);
		} // WizardComplete

		/// <summary>
		/// Saves customer's details to DB.
		/// </summary>
		/// <param name="personalInfo">Customer's personal info.</param>
		/// <param name="personalAddress">Customer's current address.</param>
		/// <param name="prevPersonAddresses">Customer's previous address(es).</param>
		/// <param name="otherPropertiesAddresses">Customer's other properties addresses</param>
		/// <param name="dateOfBirth">Customer's date of birth in format d/M/yyyy.</param>
		/// <remarks>
		/// Passing a date is kinda tricky therefore external string parameter is used for
		/// date of birth. Depending on connection method (GET/POST) different (local/invariant)
		/// culture information are used to parse date.
		/// </remarks>
		private void SaveCustomerToDB(
			PersonalInfo personalInfo,
			IEnumerable<CustomerAddress> personalAddress,
			IEnumerable<CustomerAddress> prevPersonAddresses,
			List<CustomerAddress> otherPropertiesAddresses,
			string dateOfBirth // do not remove this argument, see remarks above
		) {
			ms_oLog.Debug("SaveCustomerToDB(personalInfo.birthdate = {0}, dateOfBirth = {1}.",
				personalInfo.DateOfBirth.HasValue ? personalInfo.DateOfBirth.Value.ToString("d-MMMM-yyyy", CultureInfo.InvariantCulture) : "NULL",
				dateOfBirth
			);

			var customer = this.context.Customer;

			if (personalInfo == null)
				throw new ArgumentNullException("personalInfo");

			personalInfo.FirstName = personalInfo.FirstName.Trim();
			if (personalInfo.MiddleInitial != null) {
				personalInfo.MiddleInitial = personalInfo.MiddleInitial.Trim();
			}
			personalInfo.Surname = personalInfo.Surname.Trim();

			if (string.IsNullOrEmpty(personalInfo.FirstName))
				throw new ArgumentNullException("personalInfo." + "FirstName");

			if (string.IsNullOrEmpty(personalInfo.Surname))
				throw new ArgumentNullException("personalInfo." + "Surname");

			personalInfo.DateOfBirth = DateTime.ParseExact(dateOfBirth, "d/M/yyyy", CultureInfo.InvariantCulture);

			customer.ConsentToSearch = personalInfo.ConsentToSearch;

			if (customer.PersonalInfo != null) {
				personalInfo.OverallTurnOver = customer.PersonalInfo.OverallTurnOver;
				personalInfo.MobilePhoneVerified = customer.PersonalInfo.MobilePhoneVerified;
			}
			customer.PropertyStatus = this.propertyStatusRepository.GetAll().FirstOrDefault(x => x.Id == personalInfo.PropertyStatus);

			customer.PersonalInfo = personalInfo;

			customer.PersonalInfo.SetFullName();

			UpdateAddresses(
				customer, personalAddress, 
				customer.AddressInfo.PersonalAddress,
				CustomerAddressType.PersonalAddress, 
				lst => customer.AddressInfo.PersonalAddress = lst
			);

			UpdateAddresses(
				customer, prevPersonAddresses, 
				customer.AddressInfo.PrevPersonAddresses,
				CustomerAddressType.PrevPersonAddresses, 
				lst => customer.AddressInfo.PrevPersonAddresses = lst
			);

			UpdateAddresses(
				customer, otherPropertiesAddresses, 
				customer.AddressInfo.OtherPropertiesAddresses,
				CustomerAddressType.OtherPropertyAddress,
				lst => customer.AddressInfo.OtherPropertiesAddresses = lst
			);

			SavePhones(customer.Id, personalInfo.MobilePhone, personalInfo.DaytimePhone);

			customer.WizardStep = this.databaseHelper.WizardSteps.GetAll().FirstOrDefault(x => x.ID == (int)WizardStepType.PersonalDetails);
			this.session.Flush();

			ms_oLog.Debug("Customer {1} ({0}): wizard step has been updated to {2}", customer.Id, customer.PersonalInfo.Fullname, (int)WizardStepType.PersonalDetails);
		}

		private void SavePhones(int customerId, string mobilePhone, string daytimePhone) {
			var customerMobilePhoneEntry = new CustomerPhone {
				CustomerId = customerId,
				PhoneType = "Mobile",
				Phone = mobilePhone,
				IsCurrent = true
			};
			this.customerPhoneRepository.SaveOrUpdate(customerMobilePhoneEntry);

			var customerDaytimePhoneEntry = new CustomerPhone {
				CustomerId = customerId,
				PhoneType = "Daytime",
				Phone = daytimePhone,
				IsCurrent = true
			};
			this.customerPhoneRepository.SaveOrUpdate(customerDaytimePhoneEntry);
		} // SavePhones

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

				this.personalInfoHistoryRepository.SaveOrUpdate(personalInfoEditHistory);
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

				this.personalInfoHistoryRepository.SaveOrUpdate(personalInfoEditHistory);
			} // for each added address
		} // AddressInfoToHistory

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
				OtherPropertiesAddresses = customer.AddressInfo.OtherPropertiesAddresses.ToList(),
				OverallTurnOver = customer.PersonalInfo.OverallTurnOver.ToString(),
				WebSiteTurnover = customer.PersonalInfo.WebSiteTurnOver.ToString()
			};

			if (companyAddress != null)
				personalInfo.CompanyAddress = companyAddress.ToList();

			return personalInfo;
		} // PersonalInfoEditHistoryParametersBuilder

		private static string DetailsToKey(Director oDir) {
			CustomerAddress addr = oDir.DirectorAddressInfo.AllAddresses.FirstOrDefault();

			string sRawpostcode = addr == null ? string.Empty : addr.Rawpostcode;

			return DetailsToKey(
				oDir.Name,
				oDir.Surname,
				oDir.DateOfBirth,
				oDir.Gender,
				sRawpostcode
			);
		} // DetailsToKey

		private static string DetailsToKey(Customer customer) {
			CustomerAddress addr = customer.AddressInfo.PersonalAddress.FirstOrDefault();

			string sRawpostcode = addr == null ? string.Empty : addr.Rawpostcode;

			return DetailsToKey(
				customer.PersonalInfo.FirstName,
				customer.PersonalInfo.Surname,
				customer.PersonalInfo.DateOfBirth,
				customer.PersonalInfo.Gender,
				sRawpostcode
			);
		} // DetailsToKey

		private static string DetailsToKey(string sFirstName, string sLastName, DateTime? oBirthDate, Gender nGender, string sPostCode) {
			string sBirthDate = oBirthDate.HasValue
				? oBirthDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
				: string.Empty;

			return string.Format("f:{0}|l:{1}|b:{2}|g:{3}|p:{4}", sFirstName, sLastName, sBirthDate, nGender, sPostCode);
		} // DetailsToKey

		private readonly IEzbobWorkplaceContext context;
		private readonly IPersonalInfoHistoryRepository personalInfoHistoryRepository;
		private readonly ServiceClient serviceClient;
		private readonly ISession session;
		private readonly CashRequestBuilder cashRequestBuilder;
		private readonly IConcentAgreementHelper m_oConcentAgreementHelper = new ConcentAgreementHelper();
		private readonly DatabaseDataHelper databaseHelper;
		private readonly PropertyStatusRepository propertyStatusRepository;
		private readonly CustomerPhoneRepository customerPhoneRepository;
		private readonly CustomerAddressRepository customerAddressRepository;
		private readonly CustomerRepository customerRepository;
		private static readonly ASafeLog ms_oLog = new SafeILog(typeof(CustomerDetailsController));
	} // class CustomerDetailsController
} // namespace EzBob.Web.Areas.Customer.Controllers
