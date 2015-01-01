namespace EzBob.Web.Code.Agreements {
	using System.IO;
	using ConfigManager;
	using Ezbob.Backend.Models;
	using EzBob.Models.Agreements;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Loans;
	using Newtonsoft.Json;
	using ServiceClientProxy;

	public interface IAgreementsGenerator {
		void RenderAgreements(Loan loan, bool isRebuld);
	}

	public class AgreementsGenerator : IAgreementsGenerator {
		public AgreementsGenerator(AgreementsModelBuilder builder, IAgreementsTemplatesProvider templates, DatabaseDataHelper helper) {
			this._builder = builder;
			this._templates = templates;
			this._helper = helper;
			this.m_oServiceClient = new ServiceClient();
		}

		public void RenderAgreements(Loan loan, bool isRebuld) {
			var typeOfBusinessReduced = loan.Customer.PersonalInfo.TypeOfBusiness.AgreementReduce();

			var model = !isRebuld ? this._builder.Build(loan.Customer, loan.LoanAmount, loan) : this._builder.ReBuild(loan.Customer, loan);

			var isAlibaba = loan.Customer.IsAlibaba;
			var alibaba = isAlibaba ? "Alibaba" : string.Empty;

			string path1;
			string path2;
			TemplateModel template;
			if (typeOfBusinessReduced == TypeOfBusinessAgreementReduced.Personal) {
				var preContract = this._templates.GetTemplateByName(alibaba + "Pre-Contract-Agreement");
				var preContractAgreement = new LoanAgreement("precontract", loan, this._helper.GetOrCreateLoanAgreementTemplate(preContract, isAlibaba ? LoanAgreementTemplateType.AlibabaPreContractAgreement : LoanAgreementTemplateType.PreContractAgreement));
				loan.Agreements.Add(preContractAgreement);

				path1 = Path.Combine(CurrentValues.Instance.AgreementPdfLoanPath1, preContractAgreement.FilePath);
				path2 = Path.Combine(CurrentValues.Instance.AgreementPdfLoanPath2, preContractAgreement.FilePath);
				template = new TemplateModel {
					Template = preContract
				};
				this.m_oServiceClient.Instance.SaveAgreement(loan.Customer.Id, model, loan.RefNumber, "precontract", template, path1, path2);

				var contract = this._templates.GetTemplateByName(alibaba + "CreditActAgreement");
				var contractAgreement = new LoanAgreement("Contract", loan, this._helper.GetOrCreateLoanAgreementTemplate(contract, isAlibaba ? LoanAgreementTemplateType.AlibabaCreditActAgreement : LoanAgreementTemplateType.CreditActAgreement));
				loan.Agreements.Add(contractAgreement);

				path1 = Path.Combine(CurrentValues.Instance.AgreementPdfLoanPath1, contractAgreement.FilePath);
				path2 = Path.Combine(CurrentValues.Instance.AgreementPdfLoanPath2, contractAgreement.FilePath);
				template = new TemplateModel {
					Template = contract
				};
				this.m_oServiceClient.Instance.SaveAgreement(loan.Customer.Id, model, loan.RefNumber, "Contract", template, path1, path2);
			} else {
				var guarantee = this._templates.GetTemplateByName(alibaba + "GuarantyAgreement");
				var guaranteeAgreement = new LoanAgreement("guarantee", loan, this._helper.GetOrCreateLoanAgreementTemplate(guarantee, isAlibaba ? LoanAgreementTemplateType.AlibabaGuarantyAgreement : LoanAgreementTemplateType.GuarantyAgreement));
				loan.Agreements.Add(guaranteeAgreement);

				path1 = Path.Combine(CurrentValues.Instance.AgreementPdfLoanPath1, guaranteeAgreement.FilePath);
				path2 = Path.Combine(CurrentValues.Instance.AgreementPdfLoanPath2, guaranteeAgreement.FilePath);
				template = new TemplateModel {
					Template = guarantee
				};
				this.m_oServiceClient.Instance.SaveAgreement(loan.Customer.Id, model, loan.RefNumber, "guarantee", template, path1, path2);

				var agreement = this._templates.GetTemplateByName(alibaba + "PrivateCompanyLoanAgreement");
				var agreementAgreement = new LoanAgreement("agreement", loan, this._helper.GetOrCreateLoanAgreementTemplate(agreement, isAlibaba ? LoanAgreementTemplateType.AlibabaPrivateCompanyLoanAgreement : LoanAgreementTemplateType.PrivateCompanyLoanAgreement));
				loan.Agreements.Add(agreementAgreement);

				path1 = Path.Combine(CurrentValues.Instance.AgreementPdfLoanPath1, agreementAgreement.FilePath);
				path2 = Path.Combine(CurrentValues.Instance.AgreementPdfLoanPath2, agreementAgreement.FilePath);
				template = new TemplateModel {
					Template = agreement
				};
				this.m_oServiceClient.Instance.SaveAgreement(loan.Customer.Id, model, loan.RefNumber, "agreement", template, path1, path2);
			}

			loan.AgreementModel = JsonConvert.SerializeObject(model);
		}

		private readonly AgreementsModelBuilder _builder;
		private readonly IAgreementsTemplatesProvider _templates;
		private readonly DatabaseDataHelper _helper;
		private readonly ServiceClient m_oServiceClient;
	}
}
