using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using ApplicationMng.Model;
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
using log4net;

namespace EzBob.Web.Areas.Customer.Controllers
{
    public class CustomerDetailsController : Controller
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(CustomerDetailsController));

        private readonly IEzbobWorkplaceContext _context;
        private readonly IPersonalInfoHistoryRepository _personalInfoHistoryRepository;
        private readonly IAppCreator _creator;
        private readonly ISession _session;
        private readonly CashRequestBuilder _crBuilder;
        private readonly IConcentAgreementHelper _concentAgreementHelper = new ConcentAgreementHelper();

        public CustomerDetailsController(
                                            IEzbobWorkplaceContext context, 
                                            IPersonalInfoHistoryRepository personalInfoHistoryRepository,
                                            IAppCreator creator, 
                                            ISession session,
                                            CashRequestBuilder crBuilder)
        {
            _context = context;
            _personalInfoHistoryRepository = personalInfoHistoryRepository;
            _creator = creator;
            _session = session;
            _crBuilder = crBuilder;
        }
        //---------------------------------------------------------------------------------------------------------------------------
        [Transactional]
        [Ajax]
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        public JsonNetResult Save(
            bool? noCompany,
            LimitedInfo limitedInfo,
            NonLimitedInfo nonLimitedInfo,
            PersonalInfo personalInfo,
            List<CustomerAddress> personalAddress,
            List<CustomerAddress> prevPersonAddresses,
            List<CustomerAddress> limitedCompanyAddress,
            List<CustomerAddress> nonLimitedCompanyAddress,
            List<DirectorModel> limitedDirectors,
            List<DirectorModel> nonLimitedDirectors,
            string dateOfBirth,
            List<CustomerAddress> otherPropertyAddress,
			CompanyEmployeeCountInfo companyEmployeeCountInfo
        )
        {
            var customer = _context.Customer;

            ProcessPersonal(personalInfo, personalAddress, prevPersonAddresses, dateOfBirth, otherPropertyAddress, customer);

	        switch (personalInfo.TypeOfBusiness.Reduce())
            {
                case TypeOfBusinessReduced.Limited:
                    ProcessLimited(limitedInfo, limitedCompanyAddress, limitedDirectors, customer);
                    break;
                case TypeOfBusinessReduced.NonLimited:
                    ProcessNonLimited(nonLimitedInfo, nonLimitedCompanyAddress, nonLimitedDirectors, customer);
                    break;
            }

            _crBuilder.CreateCashRequest(customer);

            customer.WizardStep = WizardStepType.AllStep;

			if (customer.IsOffline) {
				customer.CompanyEmployeeCount.Add(new CompanyEmployeeCount {
					BottomEarningEmployeeCount = companyEmployeeCountInfo.BottomEarningEmployeeCount,
					Created = DateTime.UtcNow,
					Customer = customer,
					EmployeeCount = companyEmployeeCountInfo.EmployeeCount,
					EmployeeCountChange = companyEmployeeCountInfo.EmployeeCountChange,
					TopEarningEmployeeCount = companyEmployeeCountInfo.TopEarningEmployeeCount
				});
			} // if

            _session.Flush();
            _creator.EmailUnderReview(_context.User, customer.PersonalInfo.FirstName, customer.Name);
            _creator.Evaluate(_context.User, NewCreditLineOption.UpdateEverythingAndApplyAutoRules, Convert.ToInt32(customer.IsAvoid));
			if(!customer.IsTest)
	        {
		        _creator.FraudChecker(_context.User);
	        }
	        _concentAgreementHelper.Save(customer, DateTime.UtcNow);

            return this.JsonNet(new {});
        }

        private static void ProcessPersonal(PersonalInfo personalInfo, List<CustomerAddress> personalAddress, List<CustomerAddress> prevPersonAddresses, string dateOfBirth,
			List<CustomerAddress> otherPropertyAddress, EZBob.DatabaseLib.Model.Database.Customer customer)
        {
            ValidatePersonalInfo(personalInfo);

            personalInfo.DateOfBirth = DateTime.ParseExact(dateOfBirth, "d/M/yyyy", CultureInfo.InvariantCulture);
            personalInfo.Surname = personalInfo.Surname.Trim();
            personalInfo.FirstName = personalInfo.FirstName.Trim();
            personalInfo.MiddleInitial = string.IsNullOrEmpty(personalInfo.MiddleInitial) ? "" : personalInfo.MiddleInitial.Trim();
            personalInfo.Fullname = string.Format("{0} {1} {2}", personalInfo.FirstName, personalInfo.Surname, personalInfo.MiddleInitial);

            customer.PersonalInfo = personalInfo;

            if (personalAddress != null)
            {
                foreach (var val in personalAddress)
                {
                    val.AddressType = CustomerAddressType.PersonalAddress;
                    val.Customer = customer;
                }
                customer.AddressInfo.PersonalAddress = new HashedSet<CustomerAddress>(personalAddress);
            }

            if (prevPersonAddresses != null)
            {
                foreach (var val in prevPersonAddresses)
                {
                    val.AddressType = CustomerAddressType.PrevPersonAddresses;
                    val.Customer = customer;
                }
               customer.AddressInfo.PrevPersonAddresses = new HashedSet<CustomerAddress>(prevPersonAddresses);
            }

            if (otherPropertyAddress != null)
            {
                foreach (var val in otherPropertyAddress)
                {
                    val.AddressType = CustomerAddressType.OtherPropertyAddress;
                    val.Customer = customer;
                }
                customer.AddressInfo.OtherPropertyAddress = new HashedSet<CustomerAddress>(otherPropertyAddress);
            }
        }

        private static void ProcessNonLimited(NonLimitedInfo nonLimitedInfo, List<CustomerAddress> nonLimitedCompanyAddress,
                                              IEnumerable<DirectorModel> nonLimitedDirectors, EZBob.DatabaseLib.Model.Database.Customer customer)
        {
            customer.NonLimitedInfo = nonLimitedInfo;
            if (nonLimitedDirectors != null)
            {
                customer.NonLimitedInfo.Directors.AddAll(nonLimitedDirectors.Select(d =>
                    {
                        var dir = d.FromModel();
                        if (dir != null)
                        {
                            if (dir.DirectorAddressInfo != null && dir.DirectorAddressInfo.AllAddresses != null)
                                foreach (var address in dir.DirectorAddressInfo.AllAddresses)
                                {
                                    address.AddressType = CustomerAddressType.NonLimitedDirectorHomeAddress;
                                    address.Director = dir;
                                }
                            dir.Customer = customer;
                        }
                        return dir;
                    }).Where(d => d != null).ToList());
            }
            if (nonLimitedCompanyAddress != null)
            {
                foreach (var val in nonLimitedCompanyAddress)
                {
                    val.AddressType = CustomerAddressType.NonLimitedCompanyAddress;
                    val.Customer = customer;
                }
                customer.AddressInfo.NonLimitedCompanyAddress = new HashedSet<CustomerAddress>(nonLimitedCompanyAddress);
            }
        }

        private static void ProcessLimited(LimitedInfo limitedInfo, ICollection<CustomerAddress> limitedCompanyAddress, IEnumerable<DirectorModel> limitedDirectors,
                                           EZBob.DatabaseLib.Model.Database.Customer customer)
        {
            customer.LimitedInfo = limitedInfo;
            if (limitedDirectors != null)
            {
                customer.LimitedInfo.Directors.AddAll(
                    limitedDirectors.Select(d =>
                        {
                            var dir = d.FromModel();
                            if (dir != null)
                            {
                                if (dir.DirectorAddressInfo != null && dir.DirectorAddressInfo.AllAddresses != null)
                                    foreach (var address in dir.DirectorAddressInfo.AllAddresses)
                                    {
                                        address.AddressType = CustomerAddressType.LimitedDirectorHomeAddress;
                                        address.Director = dir;
                                    }
                                dir.Customer = customer;
                            }

                            return dir;
                        }).Where(d => d != null).ToList());
            }
            if (limitedCompanyAddress != null)
            {
                foreach (var val in limitedCompanyAddress)
                {
                    val.AddressType = CustomerAddressType.LimitedCompanyAddress;
                    val.Customer = customer;
                }
                customer.AddressInfo.LimitedCompanyAddress = new HashedSet<CustomerAddress>(limitedCompanyAddress);
            }
        }

        [NonAction]
        public static void ValidatePersonalInfo(PersonalInfo personalInfo)
        {
            if (personalInfo == null)
            {
                throw new ArgumentNullException("personalInfo");
            }

            if(string.IsNullOrEmpty(personalInfo.FirstName))
            {
                throw new ArgumentNullException("personalInfo." + "FirstName");
            }

            if (string.IsNullOrEmpty(personalInfo.Surname))
            {
                throw new ArgumentNullException("personalInfo.S" + "urname");
            }
        }

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

			MakeAddress(
				otherPropertyAddress,
				addressInfo.OtherPropertyAddress,
				CustomerAddressType.OtherPropertyAddressPrev,
				addressInfo.OtherPropertyAddress,
				CustomerAddressType.OtherPropertyAddress
			);

            if (customer.PersonalInfo.TypeOfBusiness.Reduce() == TypeOfBusinessReduced.Limited)
            {
                customer.LimitedInfo.LimitedBusinessPhone = businessPhone;

                MakeAddress(limitedCompanyAddress, addressInfo.LimitedCompanyAddressPrev,
                            CustomerAddressType.LimitedCompanyAddressPrev, addressInfo.LimitedCompanyAddress,
                            CustomerAddressType.LimitedCompanyAddress);

                var directors = customer.LimitedInfo.Directors;
                if (directors.Any())
                {
                    foreach (var d in directors)
                    {
                        foreach (var da in directorAddress.Where(da => da.Any(x => x.DirectorId == d.Id)))
                        {
                            MakeAddress(da,
                                        d.DirectorAddressInfo.LimitedDirectorHomeAddressPrev,
                                        CustomerAddressType.LimitedDirectorHomeAddressPrev,
                                        d.DirectorAddressInfo.LimitedDirectorHomeAddress,
                                        CustomerAddressType.LimitedDirectorHomeAddress);
                        }
                    }
                }
            }

            else if (customer.PersonalInfo.TypeOfBusiness.Reduce() == TypeOfBusinessReduced.NonLimited)
            {
                customer.NonLimitedInfo.NonLimitedBusinessPhone = businessPhone;

                MakeAddress(nonLimitedCompanyAddress, addressInfo.NonLimitedCompanyAddressPrev,
                            CustomerAddressType.NonLimitedCompanyAddressPrev, addressInfo.NonLimitedCompanyAddress,
                            CustomerAddressType.NonLimitedCompanyAddress);

                var directors = customer.NonLimitedInfo.Directors;
                if (directors.Any())
                {
                    foreach (var d in directors)
                    {
                        foreach (var da in directorAddress.Where(da => da.Any(x => x.DirectorId == d.Id)))
                        {
                            MakeAddress(da,
                                        d.DirectorAddressInfo.NonLimitedDirectorHomeAddressPrev,
                                        CustomerAddressType.NonLimitedDirectorHomeAddressPrev,
                                        d.DirectorAddressInfo.NonLimitedDirectorHomeAddress,
                                        CustomerAddressType.NonLimitedDirectorHomeAddress);
                        }
                    }
                }
            }

            var newPersonalInfo = PersonalInfoEditHistoryParametersBuilder(customer);

            SaveEditHistory(oldPersonalInfo, newPersonalInfo);

            return this.JsonNet(new { });
        }

        private void MakeAddress(IEnumerable<CustomerAddress> newAddress, Iesi.Collections.Generic.ISet<CustomerAddress> prevAddress, CustomerAddressType prevAddressType, Iesi.Collections.Generic.ISet<CustomerAddress> currentAddress, CustomerAddressType currentAddressType)
        {
            var newAddresses = newAddress as IList<CustomerAddress> ?? newAddress.ToList();
            var addAddress = newAddresses.Where(i => i.AddressId == 0).ToList();
            var curAddress = addAddress.LastOrDefault() ?? currentAddress.LastOrDefault();

            if (curAddress == null) return;

            foreach (var address in newAddresses)
            {
                address.Director = currentAddress.First().Director;
                address.Customer = currentAddress.First().Customer;
            }

            foreach (var address in currentAddress)
            {
                address.AddressType = prevAddressType;
            }

            foreach (var item in addAddress.Where(a => a.Id != curAddress.Id))
            {
                item.AddressType = prevAddressType;
                prevAddress.Add(item);
            }

            curAddress.AddressType = currentAddressType;
            currentAddress.Clear();
            currentAddress.Add(curAddress);
        }

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
            }

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
            }

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
            }

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
            }

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
            }

            AddAddressInfoToHistory(oldPersonalInfo.PersonalAddress, newPersonalInfo.PersonalAddress, customer, "Personal Address");
            AddAddressInfoToHistory(oldPersonalInfo.CompanyAddress, newPersonalInfo.CompanyAddress, customer, "Company Address");
        }

        private void AddAddressInfoToHistory(IEnumerable<CustomerAddress> oldAddress,
                                             IList<CustomerAddress> newAddress,
                                             EZBob.DatabaseLib.Model.Database.Customer customer,
                                             string fieldName)
        {
            if (oldAddress == null || newAddress == null) return;
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
            }

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
            }
        }


        private PersonalInfoHistoryParameter PersonalInfoEditHistoryParametersBuilder(EZBob.DatabaseLib.Model.Database.Customer customer)
        {
            string businessPhone = "";
            IList<CustomerAddress> companyAddress = null;
            
            if (customer.PersonalInfo.TypeOfBusiness.Reduce() == TypeOfBusinessReduced.Limited)
            {
                businessPhone = customer.LimitedInfo.LimitedBusinessPhone;
                companyAddress = customer.AddressInfo.LimitedCompanyAddress.ToList();

            }
            else if (customer.PersonalInfo.TypeOfBusiness.Reduce() == TypeOfBusinessReduced.NonLimited)
            {
                businessPhone = customer.NonLimitedInfo.NonLimitedBusinessPhone;
                companyAddress = customer.AddressInfo.NonLimitedCompanyAddress.ToList();
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
        }
    }
}