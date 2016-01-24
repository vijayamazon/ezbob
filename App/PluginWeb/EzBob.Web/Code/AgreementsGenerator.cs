namespace EzBob.Web.Code {
    using System;
    using System.IO;
    using ConfigManager;
    using Ezbob.Backend.CalculateLoan.LoanCalculator.Exceptions;
    using Ezbob.Backend.Models;
    using Ezbob.Backend.Models.LegalDocs;
    using Ezbob.Backend.ModelsWithDB.LegalDocs;
    using Ezbob.Backend.ModelsWithDB.NewLoan;
    using EZBob.DatabaseLib.Model.Database;
    using EZBob.DatabaseLib.Model.Database.Loans;
    using EZBob.DatabaseLib.Model.Loans;
    using Newtonsoft.Json;
    using ServiceClientProxy;

    public interface IAgreementsGenerator {
        void RenderAgreements(Loan loan, bool isRebuld, NL_Model nlModel = null);
    }

    public class AgreementsGenerator : IAgreementsGenerator {
        private readonly AgreementsModelBuilder _builder;
        private readonly ServiceClient serviceClient;

        public AgreementsGenerator(AgreementsModelBuilder builder, ServiceClient serviceClient) {
            this._builder = builder;
            this.serviceClient = serviceClient;
        }

        /// <exception cref="OverflowException">The number of elements in is larger than <see cref="F:System.Int32.MaxValue" />.</exception>
        /// <exception cref="Exception">Alibaba can't be a Personal customer</exception>
        /// <exception cref="NullReferenceException"><paramref /> is null. </exception>
        /// <exception cref="NoScheduleException">Condition. </exception>
        public void RenderAgreements(Loan loan, bool isRebuld, NL_Model nlModel = null) {

            var model = !isRebuld ? this._builder.Build(loan.Customer, loan.LoanAmount, loan) : this._builder.ReBuild(loan.Customer, loan);
            // NL - AgreementModel in json  
            if (nlModel != null)
                nlModel.Loan.LastHistory().AgreementModel = JsonConvert.SerializeObject(this._builder.NL_BuildAgreementModel(loan.Customer, nlModel));

            var productSubTypeID = loan.CashRequest.ProductSubTypeID;
            var isRegulated = loan.Customer.PersonalInfo.TypeOfBusiness.IsRegulated();
            var originId = loan.Customer.CustomerOrigin.CustomerOriginID;

            LoanAgreementTemplate[] loanAgreementTemplates = this.serviceClient.Instance.GetLegalDocs(loan.Customer.Id, 1, originId, isRegulated, productSubTypeID ?? 0).LoanAgreementTemplates;
            foreach (var loanAgreementTemplate in loanAgreementTemplates) {
                var agreement = new LoanAgreement(loanAgreementTemplate.Name, loan, loanAgreementTemplate.Id);
                loan.Agreements.Add(agreement);
                string path1 = Path.Combine(CurrentValues.Instance.AgreementPdfLoanPath1, agreement.FilePath);
                string path2 = Path.Combine(CurrentValues.Instance.AgreementPdfLoanPath2, agreement.FilePath);
                TemplateModel templateModel = new TemplateModel {
                    Template = loanAgreementTemplate.Template
                };
                this.serviceClient.Instance.SaveAgreement(loan.Customer.Id, model, loan.RefNumber, Enum.GetName(typeof(LegalDocsEnums.LoanAgreementTemplateType), agreement.Id), templateModel, path1, path2);

                string nlpath1 = Path.Combine(CurrentValues.Instance.NL_AgreementPdfLoanPath1, agreement.FilePath);
                string nlpath2 = Path.Combine(CurrentValues.Instance.NL_AgreementPdfLoanPath2, agreement.FilePath);
                this.serviceClient.Instance.SaveAgreement(loan.Customer.Id, model, loan.RefNumber, Enum.GetName(typeof(LegalDocsEnums.LoanAgreementTemplateType), agreement.Id), templateModel, nlpath1, nlpath2);


                if (nlModel != null) {
                    nlModel.Loan.LastHistory()
                        .Agreements.Add(new NL_LoanAgreements() {
                            LoanAgreementTemplateID = agreement.TemplateID,
                            FilePath = agreement.FilePath
                        });
                }
            }
            loan.AgreementModel = JsonConvert.SerializeObject(model);

        }
    }
}
