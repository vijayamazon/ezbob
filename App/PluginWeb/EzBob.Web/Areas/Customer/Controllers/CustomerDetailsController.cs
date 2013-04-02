using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using ApplicationMng.Model;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Repository;
using EZBob.DatabaseLib.Model.Loans;
using EzBob.Signals.ZohoCRM;
using EzBob.Web.ApplicationCreator;
using EzBob.Web.Areas.Customer.Models;
using EzBob.Web.Code;
using EzBob.Web.Infrastructure;
using EzBob.Web.Infrastructure.csrf;
using Iesi.Collections.Generic;
using NHibernate;
using Scorto.Web;
using ZohoCRM;
using log4net;

namespace EzBob.Web.Areas.Customer.Controllers
{
    public class CustomerDetailsController : Controller
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(CustomerDetailsController));

        private readonly IEzbobWorkplaceContext _context;
        private readonly IPersonalInfoHistoryRepository _personalInfoHistoryRepository;
        private readonly IZohoFacade _crm;
        private readonly IAppCreator _creator;
        private readonly ILoanTypeRepository _loanTypes;
        private readonly ISession _session;
        private readonly IConcentAgreementHelper _concentAgreementHelper = new ConcentAgreementHelper(); 

        public CustomerDetailsController(IEzbobWorkplaceContext context, IPersonalInfoHistoryRepository personalInfoHistoryRepository, IZohoFacade crm, IAppCreator creator, ILoanTypeRepository loanTypes, ISession session)
        {
            _context = context;
            _personalInfoHistoryRepository = personalInfoHistoryRepository;
            _crm = crm;
            _creator = creator;
            _loanTypes = loanTypes;
            _session = session;
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
            string dateOfBirth
        )
        {
            var customer = _context.Customer;

            ProcessPersonal(personalInfo, personalAddress, prevPersonAddresses, dateOfBirth, customer);

            switch (personalInfo.TypeOfBusiness.Reduce())
            {
                case TypeOfBusinessReduced.Limited:
                    ProcessLimited(limitedInfo, limitedCompanyAddress, limitedDirectors, customer);
                    break;
                case TypeOfBusinessReduced.NonLimited:
                    ProcessNonLimited(nonLimitedInfo, nonLimitedCompanyAddress, nonLimitedDirectors, customer);
                    break;
            }

            customer.IsSuccessfullyRegistered = true;

            var cashRequest = new CashRequest
            {
                CreationDate = DateTime.UtcNow,
                IdCustomer = customer.Id,
                InterestRate = 0.06M,
                LoanType = _loanTypes.GetDefault(),
                RepaymentPeriod = _loanTypes.GetDefault().RepaymentPeriod
            };

            customer.WizardStep = WizardStepType.AllStep;
            customer.CashRequests.Add(cashRequest);

            _session.Flush();

            _creator.Evaluate(_context.User);

            _concentAgreementHelper.Save(customer, DateTime.UtcNow);

            try
            {
                _crm.ConvertLead(customer);
            }
            catch (Exception e)
            {
                _log.Warn("Converting lead failed");
                _log.Warn(e);
            }

            return this.JsonNet(new {});
        }

        private static void ProcessPersonal(PersonalInfo personalInfo, List<CustomerAddress> personalAddress, List<CustomerAddress> prevPersonAddresses, string dateOfBirth,
                                            EZBob.DatabaseLib.Model.Database.Customer customer)
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
                    val.AddressType = AddressType.PersonalAddress;
                }
                customer.AddressInfo.PersonalAddress = new HashedSet<CustomerAddress>(personalAddress);
            }

            if (prevPersonAddresses != null)
            {
                foreach (var val in prevPersonAddresses)
                {
                    val.AddressType = AddressType.PrevPersonAddresses;
                }
               customer.AddressInfo.PrevPersonAddresses = new HashedSet<CustomerAddress>(prevPersonAddresses);
            }
        }

        private static void ProcessNonLimited(NonLimitedInfo nonLimitedInfo, List<CustomerAddress> nonLimitedCompanyAddress,
                                              List<DirectorModel> nonLimitedDirectors, EZBob.DatabaseLib.Model.Database.Customer customer)
        {
            customer.NonLimitedInfo = nonLimitedInfo;
            if (nonLimitedDirectors != null)
            {
                customer.NonLimitedInfo.Directors.AddAll(nonLimitedDirectors.Select(d =>
                    {
                        var dir = d.FromModel();
                        if (dir != null)
                        {
                            if (dir.DirectorAddress != null)
                                foreach (var address in dir.DirectorAddress)
                                {
                                    address.AddressType = AddressType.NonLimitedDirectorHomeAddress;
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
                    val.AddressType = AddressType.NonLimitedCompanyAddress;
                }
                customer.AddressInfo.NonLimitedCompanyAddress = new HashedSet<CustomerAddress>(nonLimitedCompanyAddress);
            }
        }

        private static void ProcessLimited(LimitedInfo limitedInfo, List<CustomerAddress> limitedCompanyAddress, List<DirectorModel> limitedDirectors,
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
                                if (dir.DirectorAddress != null)
                                    foreach (var address in dir.DirectorAddress)
                                    {
                                        address.AddressType = AddressType.LimitedDirectorHomeAddress;
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
                    val.AddressType = AddressType.LimitedCompanyAddress;
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
                throw new ArgumentNullException("personalInfo.FirstName");
            }

            if (string.IsNullOrEmpty(personalInfo.Surname))
            {
                throw new ArgumentNullException("personalInfo.Surname");
            }
        }

        [Transactional]
        [Ajax]
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        public JsonNetResult Edit(string dayTimePhone, string mobilePhone, string businessPhone, decimal? overallTurnOver, decimal? webSiteTurnOver,   List<CustomerAddress> personalAddress, List<CustomerAddress> limitedCompanyAddress, List<CustomerAddress> nonLimitedCompanyAddress)
        {
            var customer = _context.Customer;

            var oldPersonalInfo = PersonalInfoEditHistoryParametersBuilder(customer);

            customer.PersonalInfo.DaytimePhone = dayTimePhone;
            customer.PersonalInfo.MobilePhone = mobilePhone;
            customer.PersonalInfo.OverallTurnOver = overallTurnOver;
            customer.PersonalInfo.WebSiteTurnOver = webSiteTurnOver;
            if (personalAddress != null)
            {
                foreach (var val in personalAddress)
                {
                    val.AddressType = AddressType.PrevPersonAddresses;
                }
                personalAddress.Last().AddressType = AddressType.PersonalAddress;
                customer.AddressInfo.PersonalAddress = new HashedSet<CustomerAddress>(personalAddress);
            }

            if (customer.PersonalInfo.TypeOfBusiness.Reduce() == TypeOfBusinessReduced.Limited)
            {
                customer.LimitedInfo.LimitedBusinessPhone = businessPhone;
                if (limitedCompanyAddress != null)
                {
                    foreach (var val in limitedCompanyAddress)
                    {
                        val.AddressType = AddressType.LimitedCompanyAddress;
                    }
                    customer.AddressInfo.LimitedCompanyAddress = new HashedSet<CustomerAddress>(limitedCompanyAddress);
                }
            }
            else if (customer.PersonalInfo.TypeOfBusiness.Reduce() == TypeOfBusinessReduced.NonLimited)
            {
                customer.NonLimitedInfo.NonLimitedBusinessPhone = businessPhone;
                if (nonLimitedCompanyAddress != null)
                {
                    foreach (var val in nonLimitedCompanyAddress)
                    {
                        val.AddressType = AddressType.NonLimitedCompanyAddress;
                    }
                    customer.AddressInfo.NonLimitedCompanyAddress = new HashedSet<CustomerAddress>(nonLimitedCompanyAddress);
                }
            }
            var newPersonalInfo = PersonalInfoEditHistoryParametersBuilder(customer);

            SaveEditHistory(oldPersonalInfo, newPersonalInfo);

            try
            {
                _crm.UpdateOrCreate(customer);
            }
            catch (Exception e)
            {
                _log.Warn("CRM: updating customer failed");
                _log.Warn(e);
            }

            return this.JsonNet(new { });
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

        private void AddAddressInfoToHistory(IList<CustomerAddress> oldAddress,
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