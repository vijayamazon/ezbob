using System;
using System.Linq;
using ApplicationMng.Model;
using Deveel.Web.Zoho;
using EZBob.DatabaseLib;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Loans;
using EzBob.Configuration;
using EzBob.Models;
using EzBob.Web.Areas.Customer.Models;
using EzBob.Web.Areas.Underwriter;
using EzBob.Web.Areas.Underwriter.Models;
using EzBob.Web.Code;
using EzBob.Web.Code.Agreements;
using Newtonsoft.Json;
using log4net;

namespace ZohoCRM
{
    public class ZohoFacade : IZohoFacade
    {
        private readonly IZohoConfig _config;
        private readonly MarketPlacesFacade _marketPlacesFacade;
        private readonly ProfileSummaryModelBuilder _profileSummaryModelBuilder;
        private readonly AgreementRenderer _agreementRenderer;
        private readonly ZohoCrmClient _crm;
        private readonly DatabaseDataHelper _helper;

        private static readonly ILog log = LogManager.GetLogger("ZohoCRM.ZohoFacade");

        public ZohoFacade(
                            IZohoConfig config,
                            DatabaseDataHelper helper, 
                            MarketPlacesFacade marketPlacesFacade,
                            ProfileSummaryModelBuilder profileSummaryModelBuilder,
                            AgreementRenderer agreementRenderer)
        {
            _config = config;
            _marketPlacesFacade = marketPlacesFacade;
            _profileSummaryModelBuilder = profileSummaryModelBuilder;
            _agreementRenderer = agreementRenderer;
            _crm = new ZohoCrmClient(_config.Token);
            _helper = helper;
        }

        /// <summary>
        /// Registers new lead in Zoho CRM
        /// As we know only customer's email, so provide fake name and surname,
        /// because those fields are madatory.
        /// </summary>
        /// <param name="customer"></param>
        public virtual void RegisterLead(Customer customer)
        {
            try
            {
                log.DebugFormat("Registering lead {0}", customer.Name);
                var lead = new ZohoLead("Wizard", "SignUp", customer.Name);
                
                lead.SetValue("Reg. Customers Status", "New Customer");
                lead.SetValue("IsTest", customer.IsTest ? "1" : "0");
                lead.SetValue("Step", GetStep(customer));

                var response = _crm.InsertRecord(lead);
                response.ThrowIfError();
                customer.ZohoId = response.RecordDetails.First().Id;
            }
            catch (Exception e)
            {
                log.Error("Error while registering lead.", e);
            }
        }

        private bool IsLeadConverted(Customer customer)
        {
            return _crm.GetRecordById<ZohoContact>(customer.ZohoId) != null;
        }

        public virtual void ConvertLead(Customer customer)
        {
            if (IsLeadConverted(customer))
            {
                log.WarnFormat("Lead {0} already converted. ZohoId #{1}",customer.Name, customer.ZohoId);
                return;
            }
            try
            {
                log.DebugFormat("Converting lead {0}", customer.Name);
                var cr = _crm.ConvertLead(customer.ZohoId);
                customer.ZohoId = cr.Id;

                CheckZohoId(customer);
                UpdateCustomer(customer);
            }
            catch (Exception e)
            {
                log.Error(e);
            }
        }

        public void AddFile(string id, string fileName, byte[] doc)
        {
            try
            {
                _crm.UploadFileToRecord<ZohoContact>(id, fileName, "doc", doc);
            }
            catch (Exception e)
            {
                log.Error(e);
            }
        }

        public void CreateOffer(Customer customer, CashRequest cashRequest)
        {
            try
            {
                if (string.IsNullOrEmpty(customer.ZohoId)) return;
                var p = new ZohoPotential(string.Format("{0} {1} ({2})", 
                    customer.PersonalInfo.FirstName, customer.PersonalInfo.Surname, 
                    cashRequest.Customer.CashRequests.ToList().IndexOf(cashRequest)+1));
                p.SetValue("CONTACTID", customer.ZohoId);
                p.SetValue("Stage", "Processing application");
                UpdateOffer(p, cashRequest);
                var r = _crm.InsertRecord(p);
                r.ThrowIfError();
                cashRequest.ZohoId = r.RecordDetails.First().Id;
            }
            catch (Exception e)
            {
                log.Error(e);
            }
        }

        public void CreateLoan(Customer customer, Loan loan)
        {
            try
            {
                if (string.IsNullOrEmpty(customer.ZohoId)) return;
                var p = new ZohoSalesOrder(string.Format("{0} {1} ({2})", 
                    customer.PersonalInfo.FirstName, 
                    customer.PersonalInfo.Surname, 
                    loan.Customer.Loans.ToList().IndexOf(loan) + 1));
                p.SetValue("CONTACTID", customer.ZohoId);

                UpdateLoanFields(p, loan);

                var r = _crm.InsertRecord(p);
                r.ThrowIfError();
                loan.ZohoId = r.RecordDetails.First().Id;

                UpdateLoanAgreements(loan);
            }
            catch (Exception e)
            {
                log.Error(e);
            }
        }

        private void UpdateLoanFields(ZohoSalesOrder l, Loan loan)
        {
            l.SetValue("Date", loan.Date);
            l.SetValue("Amount", loan.LoanAmount);
            l.SetValue("Status", loan.Status.ToString());
            l.SetValue("Balance", loan.Balance);
            l.SetValue("DateClosed", loan.DateClosed);
            l.SetValue("Repayments", loan.Repayments);
            l.SetValue("RepaymentsNum", loan.RepaymentsNum);
            l.SetValue("InterestRate", loan.InterestRate);
            l.SetValue("SetupFee", loan.SetupFee);
        }

        public void UpdateOfferOnGetCash(CashRequest cashRequest, Customer customer)
        {
            try
            {
                log.DebugFormat("CRM: update offer on get cash for customer #{0}", customer.Id);
                CheckZohoId(cashRequest);
                UpdateEntity<ZohoPotential>(p =>
                    {
                        UpdateOffer(p, cashRequest);
                        p.SetValue("Stage", ( customer.CreditSum == 0 ? "Taken" : "Partial") );
                }, cashRequest.ZohoId);
            }
            catch (Exception e)
            {
                log.Error(e);
            }
        }

        public void RejectOffer(CashRequest cashRequest)
        {
            try
            {
                CheckZohoId(cashRequest);
                UpdateEntity<ZohoPotential>(p =>
                    {
                        UpdateOffer(p, cashRequest);
                        p.SetValue("Stage", "Rejection");
                    }, cashRequest.ZohoId);
            }
            catch (Exception e)
            {
                log.Error(e);
            }
        }

        public void ApproveOffer(CashRequest cashRequest)
        {
            try
            {
                CheckZohoId(cashRequest);
                UpdateEntity<ZohoPotential>(p =>
                                                {
                                                    UpdateOffer(p, cashRequest);
                                                    p.SetValue("Stage", "Approval");
                                                }, cashRequest.ZohoId);
            }
            catch (Exception e)
            {
                log.Error(e);
            }
        }

        private void CheckZohoId(CashRequest cashRequest)
        {
            if (cashRequest.ZohoId == null || _crm.GetRecordById<ZohoPotential>(cashRequest.ZohoId) == null)
            {
                var name = string.Format("{0} {1} ({2})", 
                    cashRequest.Customer.PersonalInfo.FirstName, 
                    cashRequest.Customer.PersonalInfo.Surname, 
                    cashRequest.Customer.CashRequests.ToList().IndexOf(cashRequest)+1);
                cashRequest.ZohoId = _crm.GetRecordIdByOfferName(name);
            }
        }

        public void UpdateCashRequest(CashRequest cr)
        {
            try
            {
                CheckZohoId(cr);
                UpdateEntity<ZohoPotential>(p => UpdateOffer(p, cr), cr.ZohoId);
            }
            catch (Exception e)
            {
                log.Error(e);
            }
        }

        public virtual void UpdateOrCreate(Customer customer)
        {
            CheckZohoId(customer);
            if (string.IsNullOrEmpty(customer.ZohoId))
            {
                RegisterLead(customer);
                ConvertLead(customer);
            }
            else
            {
                UpdateCustomer(customer);
            }
        }

        private void CheckZohoId(Customer customer)
        {
            if (customer.ZohoId == null || _crm.GetRecordById<ZohoContact>(customer.ZohoId) == null)
            {
                customer.ZohoId = _crm.GetRecordIdByUserEmail(customer.Name);
            }
        }

        private void UpdateOffers(Customer customer)
        {
            foreach (var offer in customer.CashRequests)
            {
                CheckZohoId(offer);
                if (string.IsNullOrEmpty(offer.ZohoId))
                {
                    CreateOffer(customer, offer);
                }
                else
                {
                    UpdateEntity<ZohoPotential>(p =>
                    {
                        UpdateOffer(p, offer);
                    }, offer.ZohoId);
                }
            }
        }


        private void UpdateOffer(ZohoPotential zohoPotential, CashRequest cashRequest)
        {
            zohoPotential.SetValue("Valid from", cashRequest.OfferStart);
            zohoPotential.SetValue("Closing Date", cashRequest.OfferValidUntil);
            zohoPotential.SetValue("Amount", cashRequest.ApprovedSum());
            zohoPotential.SetValue("System decision", cashRequest.SystemDecision.ToString());
            zohoPotential.SetValue("Underwriter decision", cashRequest.UnderwriterDecision.ToString());
            zohoPotential.SetValue("Interest rate", cashRequest.InterestRate*100);
            zohoPotential.SetValue("Repayment period", cashRequest.RepaymentPeriod);
            zohoPotential.SetValue("Use setup fee", cashRequest.UseSetupFee ? "Yes" : "No");
        }

        public void MoreAMLInformation(Customer customer)
        {
            try
            {
                UpdateEntity<ZohoContact>( c =>
                                               {
                                                   c.SetValue("AML Status", "Action status Request sent");
                                                   c.SetValue("AML", "Action status Request sent");
                                               }, customer.ZohoId);
            }
            catch (Exception e)
            {
                log.Error(e);
            }
        }

        public void MoreBWAInformation(Customer customer)
        {
            try
            {
                UpdateEntity<ZohoContact>(c =>
                                              {
                                                  c.SetValue("BWA Status", "Action status Request sent");
                                                  c.SetValue("BWA", "Action status Request sent");
                                              }, customer.ZohoId);
            }
            catch (Exception e)
            {
                log.Error(e);
            }
        }

        private bool UpdateEntity<T>(Action<T> action, string id) where T : ZohoEntity
        {
            if (string.IsNullOrEmpty(id)) return false;

            var c = _crm.GetRecordById<T>(id);
            if (c == null)
            {
                log.Error("Entity wasn't found for the given id");
                return false;
            }
            action(c);
            var r = _crm.UpdateRecord(c);
            if (!r)
            {
                log.Error("Updating failed");
                return false;
            }
            return true;
        }

        public void UpdateCustomer(Customer customer)
        {
            try
            {
                if (string.IsNullOrEmpty(customer.ZohoId)) return;
                var res = UpdateEntity<ZohoContact>(c =>
                                              {
                                                  var pi = customer.PersonalInfo;
                                                  if (pi != null)
                                                  {
                                                      c.DateOfBirth = pi.DateOfBirth.Value;
                                                      c.FirstName = pi.FirstName;
                                                      c.LastName = pi.Surname;
                                                      c.Mobile = pi.MobilePhone;
                                                      c.Phone = pi.DaytimePhone;
                                                      c.SetValue("Claimed Overall Turnover", pi.OverallTurnOver);
                                                      c.SetValue("Claimed Website Turnover", pi.WebSiteTurnOver);
                                                      c.SetValue("Date of Birth", pi.DateOfBirth.Value);
                                                      c.SetValue("Gender", pi.Gender.ToString());
                                                  }
                                                  
                                                  c.SetValue("Customers status", "enabled");
                                                  c.SetValue("Num of loans", customer.Loans.Count);
                                                  
                                                  c.SetValue("IsTest", customer.IsTest ? "1" : "0");
                                                  c.SetValue("IsAvoid", customer.IsAvoid ? "1" : "0");
                                                  c.SetValue("Step", GetStep(customer));

                                                  if (customer.Medal.HasValue)
                                                  {
                                                      c.SetValue("Medal", customer.Medal.ToString());
                                                  }

                                                  
                                                  c.SetValue("Number of Stores", 0);

                                                  if (customer.AddressInfo != null && customer.AddressInfo.PersonalAddress.Any())
                                                  {
                                                      var address = customer.AddressInfo.PersonalAddress.Last();
                                                      c.SetValue("Mailing Street",
                                                                 address.Line1 + address.Line2 + address.Line3);
                                                      c.SetValue("Mailing City", address.Town);
                                                      c.SetValue("Mailing State", "");
                                                      c.SetValue("Mailing Country", address.Country);
                                                      c.SetValue("Mailing Zip", address.Postcode);
                                                  }

                                                  if (customer.LimitedInfo != null && !string.IsNullOrEmpty(customer.LimitedInfo.LimitedCompanyName))
                                                  {
                                                      c.SetValue("Companies Name", customer.LimitedInfo.LimitedCompanyName);
                                                      c.SetValue("Company", customer.LimitedInfo.LimitedCompanyName);
                                                      c.SetValue("Phone", customer.LimitedInfo.LimitedBusinessPhone);
                                                      c.SetValue("Companies Number", customer.LimitedInfo.LimitedRefNum);
                                                      c.SetValue("Companies Type", pi.TypeOfBusinessName);

                                                      var companyAddress = customer.AddressInfo.LimitedCompanyAddress.Last();
                                                      FillCompanyAddress(c, companyAddress);
                                                  }

                                                  if (customer.NonLimitedInfo != null && !string.IsNullOrEmpty(customer.NonLimitedInfo.NonLimitedCompanyName))
                                                  {
                                                      c.SetValue("Companies Name", customer.NonLimitedInfo.NonLimitedCompanyName);
                                                      c.SetValue("Phone", customer.NonLimitedInfo.NonLimitedBusinessPhone);
                                                      c.SetValue("Company", customer.NonLimitedInfo.NonLimitedCompanyName);
                                                      c.SetValue("Companies Number", customer.NonLimitedInfo.NonLimitedRefNum);
                                                      c.SetValue("Companies Type", pi.TypeOfBusinessName);

                                                      var companyAddress = customer.AddressInfo.NonLimitedCompanyAddress.Last();
                                                      FillCompanyAddress(c, companyAddress);
                                                  }

                                                  UpdateStoreInformation(customer, c);
                                                  UpdateLoans(customer);
                                                  UpdateOffers(customer);
                                              }, customer.ZohoId);
                if (!res)
                {
                    UpdateEntity<ZohoLead>(l =>
                                               {
                                                   l.SetValue("IsTest", customer.IsTest ? "1" : "0");
                                                   l.SetValue("Step", GetStep(customer));
                                               }, customer.ZohoId);
                }
            }
            catch (Exception e)
            {
                log.Error(e);
            }
        }

        private int GetStep(Customer customer)
        {
            if (customer.WizardStep == WizardStepType.SignUp) return 1;
            if (customer.WizardStep == WizardStepType.Marketplace) return 2;
            if (customer.WizardStep == WizardStepType.AllStep)
            {
                if (customer.Loans.Any())
                {
                    if (customer.CreditResult == CreditResultStatus.Approved && !customer.LoanForCurrentOfferIsTaken) return 8;
                    if (customer.CreditResult == CreditResultStatus.Rejected) return 9; 
                    if (customer.ActiveLoans.Any()) return 7;
                }
                else
                {
                    if (customer.CreditResult == CreditResultStatus.Rejected) return 5;
                    if (customer.CreditResult == CreditResultStatus.Approved) return 6;
                }
                return 4;
            }
            return 0;
        }

        public void UpdateLoans(Customer customer)
        {
            try
            {
                if (string.IsNullOrEmpty(customer.ZohoId)) return;
                foreach (var loan in customer.Loans)
                {
                    CheckZohoId(loan);
                    if (string.IsNullOrEmpty(loan.ZohoId))
                    {
                        CreateLoan(customer, loan);
                    }
                    else
                    {
                        UpdateEntity<ZohoSalesOrder>(p => UpdateLoanFields(p, loan), loan.ZohoId);
                        UpdateLoanAgreements(loan);
                    }
                }
            }
            catch (Exception e)
            {
                log.Error(e);
            }
        }

        private void CheckZohoId(Loan loan)
        {
            if (loan.ZohoId != null && _crm.GetRecordById<ZohoSalesOrder>(loan.ZohoId) == null)
            {
                var name = string.Format("{0} {1} ({2})", 
                    loan.Customer.PersonalInfo.FirstName, 
                    loan.Customer.PersonalInfo.Surname,
                    loan.Customer.Loans.ToList().IndexOf(loan)+1);
                loan.ZohoId = _crm.GetRecordIdByLoanName(name);
            }
        }

        private void UpdateLoanAgreements(Loan loan)
        {
            if (string.IsNullOrEmpty(loan.AgreementModel)) return;
            var agreementModel = JsonConvert.DeserializeObject<AgreementModel>(loan.AgreementModel);
            foreach (var agreement in loan.Agreements.Where(a => string.IsNullOrEmpty(a.ZohoId)))
            {
                var pdf = _agreementRenderer.RenderAgreementToPdf(agreement.TemplateRef.Template, agreementModel);
                agreement.ZohoId = _crm.UploadFileToRecord<ZohoSalesOrder>(loan.ZohoId, agreement.LongFilename(), "pdf", pdf);
            }
        }

        private static void FillCompanyAddress(ZohoContact c, CustomerAddress companyAddress)
        {
            c.SetValue("Billing Street", companyAddress.Line1 + companyAddress.Line2 + companyAddress.Line3);
            c.SetValue("Billing City", companyAddress.Town);
            c.SetValue("Billing Code", companyAddress.Postcode);
            c.SetValue("Billing Country", companyAddress.Country);
        }

        private  void UpdateStoreInformation(Customer customer, ZohoContact c)
        {

            if (_marketPlacesFacade == null || _profileSummaryModelBuilder == null) return;

            var models = _marketPlacesFacade.GetMarketPlaceModels(customer);
            
            if (customer.PersonalInfo != null)
            {
                var profile = _profileSummaryModelBuilder.CreateProfile(customer);
                c.SetValue("Bureau Score", profile.CreditBureau.CreditBureauScore);
                c.SetValue("Shops turnover", profile.MarketPlaces.AnualTurnOver);
                c.SetValue("PayPal Income", FormattingUtils.FormatPounds((decimal)profile.PaymentAccounts.NetIncome));
                c.SetValue("MP Seniority", profile.MarketPlaces.Seniority);
                c.SetValue("AML", profile.AmlBwa.Aml);
                c.SetValue("AML Status", profile.AmlBwa.Aml);
                c.SetValue("BANK", profile.AmlBwa.Bwa);
                c.SetValue("BANK Status", profile.AmlBwa.Bwa);
            }
            

            var info = new PersonalInfoModel();
            info.InitFromCustomer(customer);
            var shops = customer.CustomerMarketPlaces.Where(s => s.Marketplace.Name != "Pay Pal").ToList();
            var shopsStirng = string.Join(", ", shops.Select(m => m.Marketplace.Name).Distinct().ToArray());
            
            int num = 1;
            foreach (var mp in models)
            {
                var storeUrlFieldName = string.Format("Store {0} URL", num);
                c.SetValue(storeUrlFieldName, mp.SellerInfoStoreURL);

                num = num + 1;
            }

            c.SetValue("Shops", shopsStirng);
            c.SetValue("Number of Stores", shops.Count);
            var strings = info.TopCategories != null? info.TopCategories.ToArray() : new string[]{};
            c.SetValue("Industry", string.Join(", ", strings));
        }
    }
}