﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using EZBob.DatabaseLib;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Repository;
using EzBob.Web.ApplicationCreator;
using EzBob.Web.Areas.Customer.Models;
using EzBob.Web.Code;
using EzBob.Web.Infrastructure;
using EzBob.Web.Infrastructure.csrf;
using Iesi.Collections.Generic;
using NHibernate;
using Scorto.Web;

namespace EzBob.Web.Areas.Customer.Controllers {
	#region class CustomerDetailsController

	public class CustomerDetailsController : Controller {
		#region public

		#region static method ValidatePersonalInfo

		[NonAction]
		public static void ValidatePersonalInfo(PersonalInfo personalInfo) {
			if (personalInfo == null)
				throw new ArgumentNullException("personalInfo");

			if (string.IsNullOrEmpty(personalInfo.FirstName))
				throw new ArgumentNullException("personalInfo." + "FirstName");

			if (string.IsNullOrEmpty(personalInfo.Surname))
				throw new ArgumentNullException("personalInfo.S" + "urname");
		} // ValidatePersonalInfo

		#endregion static method ValidatePersonalInfo

		#region constructor

		public CustomerDetailsController(
			IEzbobWorkplaceContext context,
			DatabaseDataHelper helper,
			IPersonalInfoHistoryRepository personalInfoHistoryRepository,
			IAppCreator creator,
			ISession session,
			CashRequestBuilder crBuilder
			) {
			_context = context;
			_helper = helper;
			_personalInfoHistoryRepository = personalInfoHistoryRepository;
			_creator = creator;
			_session = session;
			_crBuilder = crBuilder;
		}

		// constructor

		#endregion constructor

		#region method LinkAccountsComplete

		[Transactional]
		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public JsonNetResult LinkAccountsComplete() {
			var customer = _context.Customer;

			customer.WizardStep = _helper.WizardSteps.GetAll().FirstOrDefault(x => x.ID == (int)WizardStepType.Marketplace);
			
			_session.Flush();

			return this.JsonNet(new { });
		} // LinkAccountsComplete

		#endregion method LinkAccountsComplete

		#region method WizardComplete

		[Transactional]
		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public JsonNetResult WizardComplete() {
			var customer = _context.Customer;

			customer.WizardStep = _helper.WizardSteps.GetAll().FirstOrDefault(x => x.ID == (int)WizardStepType.AllStep);
			
			_session.Flush();

			_crBuilder.CreateCashRequest(customer);
			_creator.EmailUnderReview(_context.User, customer.PersonalInfo.FirstName, customer.Name);
			_creator.Evaluate(_context.User, NewCreditLineOption.UpdateEverythingAndApplyAutoRules, Convert.ToInt32(customer.IsAvoid));

			if (!customer.IsTest)
				_creator.FraudChecker(_context.User);

			_concentAgreementHelper.Save(customer, DateTime.UtcNow);

			return this.JsonNet(new { });
		} // WizardComplete

		#endregion method WizardComplete

		#region method SaveCompany

		[Transactional]
		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public JsonNetResult SaveCompany(
			string TypeOfBusiness,
			decimal WebSiteTurnOver,
			decimal OverallTurnOver,
			LimitedInfo limitedInfo,
			NonLimitedInfo nonLimitedInfo,
			CompanyAdditionalInfo companyAdditionalInfo,
			List<CustomerAddress> limitedCompanyAddress,
			List<CustomerAddress> nonLimitedCompanyAddress,
			List<DirectorModel> limitedDirectors,
			List<DirectorModel> nonLimitedDirectors,
			CompanyEmployeeCountInfo companyEmployeeCountInfo
		) {
			var customer = _context.Customer;

			EZBob.DatabaseLib.Model.Database.TypeOfBusiness nBusinessType;

			if (!EZBob.DatabaseLib.Model.Database.TypeOfBusiness.TryParse(TypeOfBusiness, true, out nBusinessType))
				return this.JsonNet(new { error = "Failed to parse business type: " + TypeOfBusiness });

			if (customer.PersonalInfo == null)
				customer.PersonalInfo = new PersonalInfo();

			customer.PersonalInfo.TypeOfBusiness = nBusinessType;
			customer.PersonalInfo.WebSiteTurnOver = WebSiteTurnOver;
			customer.PersonalInfo.OverallTurnOver = OverallTurnOver;

			switch (nBusinessType.Reduce()) {
			case TypeOfBusinessReduced.Limited:
				ProcessLimited(limitedInfo, limitedCompanyAddress, limitedDirectors, customer);
				break;

			case TypeOfBusinessReduced.NonLimited:
				ProcessNonLimited(nonLimitedInfo, nonLimitedCompanyAddress, nonLimitedDirectors, customer);
				break;
			} // switch

			customer.CompanyAdditionalInfo = companyAdditionalInfo;

			customer.WizardStep = _helper.WizardSteps.GetAll().FirstOrDefault(x => x.ID == (int)WizardStepType.CompanyDetails);

			if (customer.IsOffline) {
				customer.CompanyEmployeeCount.Add(new CompanyEmployeeCount {
					BottomEarningEmployeeCount = companyEmployeeCountInfo.BottomEarningEmployeeCount,
					Created = DateTime.UtcNow,
					Customer = customer,
					EmployeeCount = companyEmployeeCountInfo.EmployeeCount,
					EmployeeCountChange = companyEmployeeCountInfo.EmployeeCountChange,
					TopEarningEmployeeCount = companyEmployeeCountInfo.TopEarningEmployeeCount,
					TotalMonthlySalary = companyAdditionalInfo.TotalMonthlySalary
				});
			} // if

			_session.Flush();

			return this.JsonNet(new { });
		} // SaveCompany

		#endregion method SaveCompany

		#region method Save

		[Transactional]
		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public JsonNetResult Save(
			PersonalInfo personalInfo,
			List<CustomerAddress> personalAddress,
			List<CustomerAddress> prevPersonAddresses,
			string dateOfBirth,
			List<CustomerAddress> otherPropertyAddress
		) {
			var customer = _context.Customer;

			ValidatePersonalInfo(personalInfo);

			personalInfo.DateOfBirth = DateTime.ParseExact(dateOfBirth, "d/M/yyyy", CultureInfo.InvariantCulture);
			personalInfo.Surname = personalInfo.Surname.Trim();
			personalInfo.FirstName = personalInfo.FirstName.Trim();
			personalInfo.MiddleInitial = string.IsNullOrEmpty(personalInfo.MiddleInitial) ? "" : personalInfo.MiddleInitial.Trim();
			personalInfo.Fullname = string.Format("{0} {1} {2}", personalInfo.FirstName, personalInfo.Surname, personalInfo.MiddleInitial);

			if (customer.PersonalInfo != null) {
				personalInfo.TypeOfBusiness = customer.PersonalInfo.TypeOfBusiness;
				personalInfo.WebSiteTurnOver = customer.PersonalInfo.WebSiteTurnOver;
				personalInfo.OverallTurnOver = customer.PersonalInfo.OverallTurnOver;
			} // if

			customer.PersonalInfo = personalInfo;

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

			UpdateAddresses(
				customer, otherPropertyAddress, customer.AddressInfo.OtherPropertyAddress,
				CustomerAddressType.OtherPropertyAddress,
				lst => customer.AddressInfo.OtherPropertyAddress = lst
			);

			customer.WizardStep = _helper.WizardSteps.GetAll().FirstOrDefault(x => x.ID == (int)WizardStepType.PersonalDetails);

			_session.Flush();

			return this.JsonNet(new { });
		} // Save

		#endregion method Save

		#region method Edit

		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		[Transactional]
		public JsonNetResult Edit(
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

			if (customer.PersonalInfo.TypeOfBusiness.Reduce() == TypeOfBusinessReduced.Limited) {
				customer.LimitedInfo.LimitedBusinessPhone = businessPhone;

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

			var newPersonalInfo = PersonalInfoEditHistoryParametersBuilder(customer);

			SaveEditHistory(oldPersonalInfo, newPersonalInfo);

			return this.JsonNet(new { });
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
			EZBob.DatabaseLib.Model.Database.Customer customer,
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

		#region static method ProcessNonLimited

		private static void ProcessNonLimited(
			NonLimitedInfo nonLimitedInfo,
			List<CustomerAddress> nonLimitedCompanyAddress,
			IEnumerable<DirectorModel> nonLimitedDirectors,
			EZBob.DatabaseLib.Model.Database.Customer customer
		) {
			customer.NonLimitedInfo = nonLimitedInfo;
			if (nonLimitedDirectors != null) {
				customer.NonLimitedInfo.Directors.AddAll(nonLimitedDirectors.Select(d => {
					var dir = d.FromModel();
					if (dir != null) {
						if (dir.DirectorAddressInfo != null && dir.DirectorAddressInfo.AllAddresses != null)
							foreach (var address in dir.DirectorAddressInfo.AllAddresses) {
								address.AddressType = CustomerAddressType.NonLimitedDirectorHomeAddress;
								address.Director = dir;
							}
						dir.Customer = customer;
					}
					return dir;
				}).Where(d => d != null).ToList());
			}
			if (nonLimitedCompanyAddress != null) {
				foreach (var val in nonLimitedCompanyAddress) {
					val.AddressType = CustomerAddressType.NonLimitedCompanyAddress;
					val.Customer = customer;
				}
				customer.AddressInfo.NonLimitedCompanyAddress = new HashedSet<CustomerAddress>(nonLimitedCompanyAddress);
			}
		} // ProcessNonLimited

		#endregion static method ProcessNonLimited

		#region static method ProcessLimited

		private static void ProcessLimited(
			LimitedInfo limitedInfo,
			ICollection<CustomerAddress> limitedCompanyAddress,
			IEnumerable<DirectorModel> limitedDirectors,
			EZBob.DatabaseLib.Model.Database.Customer customer
		) {
			customer.LimitedInfo = limitedInfo;

			if (limitedDirectors != null) {
				customer.LimitedInfo.Directors.AddAll(
					limitedDirectors.Select(d => {
						var dir = d.FromModel();

						if (dir != null) {
							if (dir.DirectorAddressInfo != null && dir.DirectorAddressInfo.AllAddresses != null) {
								foreach (var address in dir.DirectorAddressInfo.AllAddresses) {
									address.AddressType = CustomerAddressType.LimitedDirectorHomeAddress;
									address.Director = dir;
								} // foreach
							} // if

							dir.Customer = customer;
						} // if

						return dir;
					})
					.Where(d => d != null).ToList()
				); // AddAll
			} // if

			if (limitedCompanyAddress != null) {
				foreach (var val in limitedCompanyAddress) {
					val.AddressType = CustomerAddressType.LimitedCompanyAddress;
					val.Customer = customer;
				} // foreach

				customer.AddressInfo.LimitedCompanyAddress = new HashedSet<CustomerAddress>(limitedCompanyAddress);
			} // if
		} // ProcessLimited

		#endregion static method ProcessLimited

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
			EZBob.DatabaseLib.Model.Database.Customer customer,
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

		private PersonalInfoHistoryParameter PersonalInfoEditHistoryParametersBuilder(EZBob.DatabaseLib.Model.Database.Customer customer) {
			string businessPhone = "";
			IList<CustomerAddress> companyAddress = null;

			if (customer.PersonalInfo.TypeOfBusiness.Reduce() == TypeOfBusinessReduced.Limited) {
				businessPhone = customer.LimitedInfo.LimitedBusinessPhone;
				companyAddress = customer.AddressInfo.LimitedCompanyAddress.ToList();
			}
			else if (customer.PersonalInfo.TypeOfBusiness.Reduce() == TypeOfBusinessReduced.NonLimited) {
				businessPhone = customer.NonLimitedInfo.NonLimitedBusinessPhone;
				companyAddress = customer.AddressInfo.NonLimitedCompanyAddress.ToList();
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

		#region private properties

		private readonly IEzbobWorkplaceContext _context;
		private readonly IPersonalInfoHistoryRepository _personalInfoHistoryRepository;
		private readonly IAppCreator _creator;
		private readonly ISession _session;
		private readonly CashRequestBuilder _crBuilder;
		private readonly IConcentAgreementHelper _concentAgreementHelper = new ConcentAgreementHelper();
        private readonly DatabaseDataHelper _helper;

		#endregion private properties

		#endregion private
	} // class CustomerDetailsController

	#endregion class CustomerDetailsController
} // namespace EzBob.Web.Areas.Customer.Controllers
