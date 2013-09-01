using ApplicationMng.Signal;
using EZBob.DatabaseLib;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Loans;
using EZBob.DatabaseLib.Model.Loans;
using EzBob.Signals.RenderAgreements;
using Newtonsoft.Json;

namespace EzBob.Web.Code.Agreements
{
    public interface IAgreementsGenerator
    {
        void RenderAgreements(Loan loan, bool isRebuld);
    }

    public class AgreementsGenerator : IAgreementsGenerator
    {
        private readonly AgreementsModelBuilder _builder;
        private readonly IAgreementsTemplatesProvider _templates;
        private readonly DatabaseDataHelper _helper;


        public AgreementsGenerator(AgreementsModelBuilder builder, IAgreementsTemplatesProvider templates, DatabaseDataHelper helper)
        {
            _builder = builder;
            _templates = templates;
            _helper = helper;
        }

        public void RenderAgreements(Loan loan, bool isRebuld)
        {
            var typeOfBusinessReduced = loan.Customer.PersonalInfo.TypeOfBusiness.Reduce();
            var isConsumer = typeOfBusinessReduced == TypeOfBusinessReduced.Personal || typeOfBusinessReduced == TypeOfBusinessReduced.NonLimited;
            
            var model = !isRebuld ? _builder.Build(loan.Customer, loan.LoanAmount, loan) : _builder.ReBuild(loan.Customer, loan);
                
            
            var client = new RenderAgreementsSignalClient(model, loan.RefNumber);

            if (isConsumer)
            {
                var preContract = _templates.GetTemplateByName("Pre-Contract-Agreement");
                var preContractAgreement = new LoanAgreement("precontract", loan, _helper.GetOrCreateLoanAgreementTemplate(preContract)) ;
                loan.Agreements.Add(preContractAgreement);
                client.AddAgreement("precontract", preContract, preContractAgreement.FilePath);

                var contract = _templates.GetTemplateByName("CreditActAgreement");
                var contractAgreement = new LoanAgreement("Contract", loan, _helper.GetOrCreateLoanAgreementTemplate(contract));
                loan.Agreements.Add(contractAgreement);
                client.AddAgreement("Contract", contract, contractAgreement.FilePath);
            }
            else
            {
                var guarantee = _templates.GetTemplateByName("GuarantyAgreement");
                var guaranteeAgreement = new LoanAgreement("guarantee", loan, _helper.GetOrCreateLoanAgreementTemplate(guarantee));
                loan.Agreements.Add(guaranteeAgreement);
                client.AddAgreement("guarantee", guarantee, guaranteeAgreement.FilePath);

                var agreement = _templates.GetTemplateByName("PrivateCompanyLoanAgreement");
                var agreementAgreement = new LoanAgreement("agreement", loan, _helper.GetOrCreateLoanAgreementTemplate(agreement));
                loan.Agreements.Add(agreementAgreement);
                client.AddAgreement("agreement", agreement, agreementAgreement.FilePath);
            }

            loan.AgreementModel = JsonConvert.SerializeObject(model);

            client.Execute(0, new PriorityInfo(null, null));
        } 
    }
}