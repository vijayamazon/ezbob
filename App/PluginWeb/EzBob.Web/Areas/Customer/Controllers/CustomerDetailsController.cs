namespace EzBob.Web.Areas.Customer.Controllers
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Globalization;
	using System.Linq;
	using System.Web.Mvc;
	using Code.ApplicationCreator;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using ExperianLib.Ebusiness;
	using Models;
	using Code;
	using Infrastructure;
	using Infrastructure.csrf;
	using Iesi.Collections.Generic;
	using NHibernate;
	using Scorto.Web;
	using EzServiceReference;
	using log4net;

	#region class CustomerDetailsController

	public class CustomerDetailsController : Controller
	{
		#region public

		#region static method ValidatePersonalInfo

		[NonAction]
		public static void ValidatePersonalInfo(PersonalInfo personalInfo)
		{
			if (personalInfo == null)
				throw new ArgumentNullException("personalInfo");

			if (string.IsNullOrEmpty(personalInfo.FirstName))
				throw new ArgumentNullException("personalInfo." + "FirstName");

			if (string.IsNullOrEmpty(personalInfo.Surname))
				throw new ArgumentNullException("personalInfo." + "Surname");
		} // ValidatePersonalInfo

		[NonAction]
		static string UppercaseWords(string value)
		{
			char[] array = value.ToCharArray();
			// Handle the first letter in the string.
			if (array.Length >= 1)
			{
				if (char.IsLower(array[0]))
				{
					array[0] = char.ToUpper(array[0]);
				}
			}
			// Scan through the letters, checking for spaces.
			// ... Uppercase the lowercase letters following spaces.
			for (int i = 1; i < array.Length; i++)
			{
				if (array[i - 1] == ' ')
				{
					if (char.IsLower(array[i]))
					{
						array[i] = char.ToUpper(array[i]);
					}
				}
			}
			return new string(array);
		}
		#endregion static method ValidatePersonalInfo

		#region constructor

		public CustomerDetailsController(
			IEzbobWorkplaceContext context,
			DatabaseDataHelper helper,
			IPersonalInfoHistoryRepository personalInfoHistoryRepository,
			IAppCreator creator,
			ISession session,
			CashRequestBuilder crBuilder
			)
		{
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
		public JsonNetResult LinkAccountsComplete()
		{
			var customer = _context.Customer;

			customer.WizardStep = _helper.WizardSteps.GetAll().FirstOrDefault(x => x.ID == (int)WizardStepType.Marketplace);

			_session.Flush();

			ms_oLog.DebugFormat("Customer {1} ({0}): wizard step has been updated to:2", customer.Id, customer.PersonalInfo.Fullname);
			
			WizardComplete();
			return this.JsonNet(new {});
		} // LinkAccountsComplete

		#endregion method LinkAccountsComplete

		#region method WizardComplete

		[Transactional]
		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public JsonNetResult WizardComplete()
		{
			TempData.Add("WizardComplete",true);

			var customer = _context.Customer;

			ms_oLog.DebugFormat("Customer {1} ({0}): has completed wizard.", customer.Id, customer.PersonalInfo.Fullname);

			customer.WizardStep = _helper.WizardSteps.GetAll().FirstOrDefault(x => x.ID == (int)WizardStepType.AllStep);

			_session.Flush();

			ms_oLog.DebugFormat("Customer {1} ({0}): wizard step has been updated.", customer.Id, customer.PersonalInfo.Fullname);

			_crBuilder.CreateCashRequest(customer);

			ms_oLog.DebugFormat("Customer {1} ({0}): cash request created.", customer.Id, customer.PersonalInfo.Fullname);

			_creator.EmailUnderReview(_context.User, customer.PersonalInfo.FirstName, customer.Name);

			ms_oLog.DebugFormat("Customer {1} ({0}): email under review started.", customer.Id, customer.PersonalInfo.Fullname);

			_creator.Evaluate(_context.User.Id, _context.User, NewCreditLineOption.UpdateEverythingAndApplyAutoRules, Convert.ToInt32(customer.IsAvoid), false, false);

			ms_oLog.DebugFormat("Customer {1} ({0}): main strategy started.", customer.Id, customer.PersonalInfo.Fullname);

			if (!customer.IsTest)
			{
				_creator.FraudChecker(_context.User);
				ms_oLog.DebugFormat("Customer {1} ({0}): fraud check started.", customer.Id, customer.PersonalInfo.Fullname);
			} // if

			_concentAgreementHelper.Save(customer, DateTime.UtcNow);
			ms_oLog.DebugFormat("Customer {1} ({0}): consent agreement saved.", customer.Id, customer.PersonalInfo.Fullname);

			
			return this.JsonNet(new { });
		} // WizardComplete

		#endregion method WizardComplete

		#region method SaveCompany

		[Transactional]
		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public JsonNetResult SaveCompany(
			LimitedInfo limitedInfo,
			NonLimitedInfo nonLimitedInfo,
			CompanyAdditionalInfo companyAdditionalInfo,
			List<CustomerAddress> limitedCompanyAddress,
			List<CustomerAddress> nonLimitedCompanyAddress,
			List<DirectorModel> limitedDirectors,
			List<DirectorModel> nonLimitedDirectors,
			CompanyEmployeeCountInfo companyEmployeeCountInfo,
			CompanyInfo experianInfo
		)
		{
			var customer = _context.Customer;

			TypeOfBusiness nBusinessType;

			if (!Enum.TryParse(companyAdditionalInfo.TypeOfBusiness, true, out nBusinessType))
				return this.JsonNet(new { error = "Failed to parse business type: " + companyAdditionalInfo.TypeOfBusiness });

			if (customer.PersonalInfo == null)
				customer.PersonalInfo = new PersonalInfo();


			customer.PersonalInfo.TypeOfBusiness = nBusinessType;
			customer.PersonalInfo.OverallTurnOver = companyAdditionalInfo.OverallTurnOver;

			if (companyAdditionalInfo.IndustryType == "online" || companyAdditionalInfo.PartBusinessOnline)
			{
				customer.IsOffline = false;
			}
			else
			{
				customer.IsOffline = true;
			}

			customer.ConsentToSearch = companyAdditionalInfo.ConsentToSearch;
			customer.IsDirector = companyAdditionalInfo.DirectorCheck;

			ProcessCompanyInfoTemporary(nBusinessType, limitedInfo, nonLimitedInfo, companyAdditionalInfo, limitedCompanyAddress, nonLimitedCompanyAddress, limitedDirectors, nonLimitedDirectors, companyEmployeeCountInfo, experianInfo, customer);

			customer.WizardStep = _helper.WizardSteps.GetAll().FirstOrDefault(x => x.ID == (int)WizardStepType.CompanyDetails);

			ms_oLog.DebugFormat("Customer {1} ({0}): wizard step has been updated to:6", customer.Id, customer.PersonalInfo.Fullname);


			_session.Flush();

			return this.JsonNet(new { });
		}

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
			string dateOfBirth
		)
		{
			var customer = _context.Customer;

			ValidatePersonalInfo(personalInfo);

			personalInfo.DateOfBirth = DateTime.ParseExact(dateOfBirth, "d/M/yyyy", CultureInfo.InvariantCulture);
			personalInfo.Surname = UppercaseWords(personalInfo.Surname.Trim());
			personalInfo.FirstName = UppercaseWords(personalInfo.FirstName.Trim());
			personalInfo.Fullname = string.Format("{0} {1}", personalInfo.FirstName, personalInfo.Surname);

			if (customer.PersonalInfo != null)
			{
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

			customer.WizardStep = _helper.WizardSteps.GetAll().FirstOrDefault(x => x.ID == (int)WizardStepType.PersonalDetails);

			ms_oLog.DebugFormat("Customer {1} ({0}): wizard step has been updated to:5", customer.Id, customer.PersonalInfo.Fullname);

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
		)
		{
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

			if (otherPropertyAddress != null)
			{
				MakeAddress(
					otherPropertyAddress,
					addressInfo.OtherPropertyAddress,
					CustomerAddressType.OtherPropertyAddressPrev,
					addressInfo.OtherPropertyAddress,
					CustomerAddressType.OtherPropertyAddress
				);
			} // if

			var company = customer.Companies.FirstOrDefault();
			if (company != null)
			{
				company.BusinessPhone = businessPhone;
			}
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

			return this.JsonNet(new { });
		} // Edit

		#endregion method Edit

		#region method SaveEditHistory

		[NonAction]
		public void SaveEditHistory(PersonalInfoHistoryParameter oldPersonalInfo, PersonalInfoHistoryParameter newPersonalInfo)
		{
			var customer = _context.Customer;

			if (oldPersonalInfo.DaytimePhone != newPersonalInfo.DaytimePhone)
			{
				var personalInfoEditHistory = new PersonalInfoHistory
				{
					Customer = customer,
					FieldName = "Day time Phone",
					OldValue = oldPersonalInfo.DaytimePhone,
					NewValue = newPersonalInfo.DaytimePhone,
					DateModifed = DateTime.Now
				};

				_personalInfoHistoryRepository.SaveOrUpdate(personalInfoEditHistory);
			} // if

			if (oldPersonalInfo.MobilePhone != newPersonalInfo.MobilePhone)
			{
				var personalInfoEditHistory = new PersonalInfoHistory
				{
					Customer = customer,
					FieldName = "Mobile Phone",
					OldValue = oldPersonalInfo.MobilePhone,
					NewValue = newPersonalInfo.MobilePhone,
					DateModifed = DateTime.Now
				};
				_personalInfoHistoryRepository.SaveOrUpdate(personalInfoEditHistory);
			} // if

			if (oldPersonalInfo.BusinessPhone != newPersonalInfo.BusinessPhone)
			{
				var personalInfoEditHistory = new PersonalInfoHistory
				{
					Customer = customer,
					FieldName = "Business Phone",
					OldValue = oldPersonalInfo.BusinessPhone,
					NewValue = newPersonalInfo.BusinessPhone,
					DateModifed = DateTime.Now
				};

				_personalInfoHistoryRepository.SaveOrUpdate(personalInfoEditHistory);
			} // if

			if (oldPersonalInfo.OverallTurnOver != newPersonalInfo.OverallTurnOver)
			{
				var personalInfoEditHistory = new PersonalInfoHistory
				{
					Customer = customer,
					FieldName = "Total Estimated Sales",
					OldValue = oldPersonalInfo.OverallTurnOver,
					NewValue = newPersonalInfo.OverallTurnOver,
					DateModifed = DateTime.Now
				};

				_personalInfoHistoryRepository.SaveOrUpdate(personalInfoEditHistory);
			} // if

			if (oldPersonalInfo.WebSiteTurnover != newPersonalInfo.WebSiteTurnover)
			{
				var personalInfoEditHistory = new PersonalInfoHistory
				{
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
		)
		{
			if (oNewAddressList == null)
			{
				if (oCurrentAddressList != null)
					oCurrentAddressList.Clear();

				return;
			} // if new list is empty, i.e. remove all the current data

			var oNewEntries = new List<CustomerAddress>();

			foreach (var val in oNewAddressList.Where(x => x.AddressId == 0))
			{
				val.AddressType = nType;
				val.Customer = customer;
				oNewEntries.Add(val);
			} // foreach

			if (oNewEntries.Count > 0)
			{
				if (oCurrentAddressList == null)
					fSetNewValue(new HashedSet<CustomerAddress>(oNewEntries));
				else
				{
					oCurrentAddressList.Clear();
					oCurrentAddressList.AddAll(oNewEntries);
				} // if
			} // if
		} // UpdateAddresses

		#endregion static method UpdateAddresses

		#region static method ProcessCompanyInfo
		private void ProcessCompanyInfoTemporary(TypeOfBusiness type, LimitedInfo limitedInfo, NonLimitedInfo nonLimitedInfo, CompanyAdditionalInfo companyAdditionalInfo, List<CustomerAddress> limitedCompanyAddress, List<CustomerAddress> nonLimitedCompanyAddress, List<DirectorModel> limitedDirectors, List<DirectorModel> nonLimitedDirectors, CompanyEmployeeCountInfo companyEmployeeCount, CompanyInfo experianInfo, Customer customer)
		{

			CompanyInfoMap companyData;
			List<CustomerAddress> companyAddress;
			List<DirectorModel> companyDirectors;
			var experianAddress = new List<CustomerAddress>
				{
					new CustomerAddress
						{
							AddressType = CustomerAddressType.ExperianCompanyAddress,
							Organisation = companyAdditionalInfo.ExperianCompanyName,
							Line1 = companyAdditionalInfo.ExperianCompanyAddrLine1,
							Line2 = companyAdditionalInfo.ExperianCompanyAddrLine2,
							Line3 = companyAdditionalInfo.ExperianCompanyAddrLine3,
							Town = companyAdditionalInfo.ExperianCompanyAddrLine4,
							Postcode = companyAdditionalInfo.ExperianCompanyPostcode,

						}
				};

			switch (type.Reduce())
			{
				case TypeOfBusinessReduced.Limited:
					companyData = new CompanyInfoMap
						{
							BusinessPhone = limitedInfo.LimitedBusinessPhone,
							CapitalExpenditure = companyAdditionalInfo.CapitalExpenditure,
							CompanyNumber = limitedInfo.LimitedCompanyNumber,
							CompanyName = limitedInfo.LimitedCompanyName,
							PropertyOwnedByCompany = companyAdditionalInfo.PropertyOwnedByCompany,
							RentMonthLeft = companyAdditionalInfo.RentMonthsLeft,
							TypeOfBusiness = type,
							TimeAtAddress = limitedInfo.LimitedTimeAtAddress,
							YearsInCompany = companyAdditionalInfo.YearsInCompany,
						};
					companyAddress = limitedCompanyAddress;
					companyDirectors = limitedDirectors;
					break;
				case TypeOfBusinessReduced.NonLimited:
					companyData = new CompanyInfoMap
						{
							BusinessPhone = nonLimitedInfo.NonLimitedBusinessPhone,
							CapitalExpenditure = companyAdditionalInfo.CapitalExpenditure,
							CompanyName = nonLimitedInfo.NonLimitedCompanyName,
							PropertyOwnedByCompany = companyAdditionalInfo.PropertyOwnedByCompany,
							RentMonthLeft = companyAdditionalInfo.RentMonthsLeft,
							TypeOfBusiness = type,
							TimeAtAddress = nonLimitedInfo.NonLimitedTimeAtAddress,
							YearsInCompany = companyAdditionalInfo.YearsInCompany,
							TimeInBusiness = nonLimitedInfo.NonLimitedTimeInBusiness
						};
					companyAddress = nonLimitedCompanyAddress;
					companyDirectors = nonLimitedDirectors;
					break;
				default:
					return;
			}

			ProcessCompanyInfo(companyData, companyAddress, experianAddress, companyDirectors, companyEmployeeCount, experianInfo, customer);


		} // SaveCompany

		private static void ProcessCompanyInfo(
			CompanyInfoMap companyData,
			ICollection<CustomerAddress> companyAddress,
			ICollection<CustomerAddress> experianCompanyAddress,
			IEnumerable<DirectorModel> directors,
			CompanyEmployeeCountInfo companyEmployeeCount,
			CompanyInfo experianInfo,
			Customer customer
		)
		{
			if (customer.Companies == null) customer.Companies = new List<Company>();

			var company = new Company
				{
					Customer = customer,
					ExperianCompanyName = experianInfo.BusName,
					ExperianRefNum = experianInfo.BusRefNum,
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
				};

			if (directors != null)
			{
				company.Directors.AddAll(
					directors.Select(d =>
					{
						var dir = d.FromModel();

						if (dir != null)
						{
							if (dir.DirectorAddressInfo != null && dir.DirectorAddressInfo.AllAddresses != null)
							{
								foreach (var address in dir.DirectorAddressInfo.AllAddresses)
								{
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
			} // if

			if (companyAddress != null)
			{
				foreach (var val in companyAddress)
				{
					val.AddressType = CustomerAddressType.LimitedCompanyAddress; //TODO
					val.Customer = customer;
					val.Company = company;
				} // foreach

				company.CompanyAddress = new HashedSet<CustomerAddress>(companyAddress);
			} // if

			if (experianInfo != null)
			{
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
				foreach (var val in experianCompanyAddress)
				{
					val.AddressType = CustomerAddressType.ExperianCompanyAddress;
					val.Customer = customer;
					val.Company = company;
				} // foreach

				company.ExperianCompanyAddress = new HashedSet<CustomerAddress>(experianCompanyAddress);
			}

			company.CompanyEmployeeCount.Add(new CompanyEmployeeCount
			{
				BottomEarningEmployeeCount = companyEmployeeCount.BottomEarningEmployeeCount,
				Created = DateTime.UtcNow,
				Customer = customer,
				EmployeeCount = companyEmployeeCount.EmployeeCount,
				EmployeeCountChange = companyEmployeeCount.EmployeeCountChange,
				TopEarningEmployeeCount = companyEmployeeCount.TopEarningEmployeeCount,
				TotalMonthlySalary = companyEmployeeCount.TotalMonthlySalary,
				Company = company
			});

			customer.Companies.Add(company);

		} // ProcessCompanyInfo

		#endregion static method ProcessCompanyInfo

		#region method MakeAddress

		private void MakeAddress(
			IEnumerable<CustomerAddress> newAddress,
			Iesi.Collections.Generic.ISet<CustomerAddress> prevAddress,
			CustomerAddressType prevAddressType,
			Iesi.Collections.Generic.ISet<CustomerAddress> currentAddress,
			CustomerAddressType currentAddressType
		)
		{
			var newAddresses = newAddress as IList<CustomerAddress> ?? newAddress.ToList();
			var addAddress = newAddresses.Where(i => i.AddressId == 0).ToList();
			var curAddress = addAddress.LastOrDefault() ?? currentAddress.LastOrDefault();

			if (curAddress == null)
				return;

			foreach (var address in newAddresses)
			{
				address.Director = currentAddress.First().Director;
				address.Customer = currentAddress.First().Customer;
				address.Company = currentAddress.First().Company;
			} // for each new address

			foreach (var address in currentAddress)
				address.AddressType = prevAddressType;

			foreach (var item in addAddress.Where(a => a.Id != curAddress.Id))
			{
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
		)
		{
			if (oldAddress == null || newAddress == null)
				return;

			var addedAddress = newAddress.Where(n => oldAddress.All(t => t.Id != n.Id)).ToList();
			var removeAddress = oldAddress.Where(n => newAddress.All(t => t.Id != n.Id)).ToList();

			foreach (var customerAddress in removeAddress)
			{
				var personalInfoEditHistory = new PersonalInfoHistory
				{
					Customer = customer,
					FieldName = fieldName,
					OldValue = customerAddress.Id,
					NewValue = string.Empty,
					DateModifed = DateTime.Now
				};

				_personalInfoHistoryRepository.SaveOrUpdate(personalInfoEditHistory);
			} // for each removed address

			foreach (var customerAddress in addedAddress)
			{
				var personalInfoEditHistory = new PersonalInfoHistory
				{
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

		private PersonalInfoHistoryParameter PersonalInfoEditHistoryParametersBuilder(Customer customer)
		{
			string businessPhone = "";
			IList<CustomerAddress> companyAddress = null;
			var company = customer.Companies.FirstOrDefault();
			if (company != null)
			{
				businessPhone = company.BusinessPhone;
				companyAddress = company.CompanyAddress.ToList();
			}

			var personalInfo = new PersonalInfoHistoryParameter
			{
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
		private static readonly ILog ms_oLog = LogManager.GetLogger(typeof(CustomerDetailsController));

		#endregion private properties

		#endregion private
	} // class CustomerDetailsController

	#endregion class CustomerDetailsController
} // namespace EzBob.Web.Areas.Customer.Controllers
