namespace EzBob.Models.Agreements // EzBob.Web.Code.Agreements 
{
	using System.IO;
	using ConfigManager;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using EzBob.Web.Code;
	using EZBob.DatabaseLib;
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
		private readonly IAgreementsTemplatesProvider _templates;
		private readonly DatabaseDataHelper _helper;
		private readonly ServiceClient m_oServiceClient;

		public AgreementsGenerator(AgreementsModelBuilder builder, IAgreementsTemplatesProvider templates, DatabaseDataHelper helper) {
			_builder = builder;
			_templates = templates;
			_helper = helper;
			m_oServiceClient = new ServiceClient();
		}

		public void RenderAgreements(Loan loan, bool isRebuld, NL_Model nlModel = null) {
			var typeOfBusinessReduced = loan.Customer.PersonalInfo.TypeOfBusiness.AgreementReduce();

			var model = !isRebuld ? this._builder.Build(loan.Customer, loan.LoanAmount, loan) : _builder.ReBuild(loan.Customer, loan);

			// NL - AgreementModel in json  
			if (nlModel != null)
				nlModel.AgreementModel = JsonConvert.SerializeObject(this._builder.NL_BuildAgreementModel(loan.Customer, nlModel));

			var isAlibaba = loan.Customer.IsAlibaba;
			var isEverline = loan.Customer.CustomerOrigin.IsEverline();

			var origin = isAlibaba ? "Alibaba" : string.Empty;
			origin = isEverline ? "EVL" : origin;

			if (isEverline) {
				EverlineLoginLoanChecker checker = new EverlineLoginLoanChecker();
				var status = checker.GetLoginStatus(loan.Customer.Name);
				if (status.status == EverlineLoanStatus.ExistsWithCurrentLiveLoan) {
					origin = origin + "Refinance";
				}
			}

			string path1;
			string path2;
			TemplateModel template;
			if (typeOfBusinessReduced == TypeOfBusinessAgreementReduced.Personal) {

				// string - content of file D:\ezbob\App\PluginWeb\EzBob.Web\Areas\Customer\Views\Agreement\Pre-Contract-Agreement.cshtml
				var preContract = this._templates.GetTemplateByName(origin + "Pre-Contract-Agreement");

				// specific LoanAgreementTemplate for current type: 
				var preContractTemplate = this._helper.GetOrCreateLoanAgreementTemplate(preContract, isAlibaba ? LoanAgreementTemplateType.AlibabaPreContractAgreement : LoanAgreementTemplateType.PreContractAgreement);

				// LoanAgreement (table) instance by preContract string == table LoanAgreementTemplate.Template && LoanAgreementTemplate.TemplateType== Enum.LoanAgreementTemplateType ID
				var preContractAgreement = new LoanAgreement("precontract", loan, preContractTemplate);
				loan.Agreements.Add(preContractAgreement);

				path1 = Path.Combine(CurrentValues.Instance.AgreementPdfLoanPath1, preContractAgreement.FilePath);
				path2 = Path.Combine(CurrentValues.Instance.AgreementPdfLoanPath2, preContractAgreement.FilePath);
				template = new TemplateModel { Template = preContract };
				this.m_oServiceClient.Instance.SaveAgreement(loan.Customer.Id, model, loan.RefNumber, "precontract", template, path1, path2);

				// NL  - preContract 
				if (nlModel != null) {
					nlModel.Agreements.Add(item: new NLAgreementItem() {
						Agreement = new NL_LoanAgreements() {
							LoanAgreementTemplateID = preContractTemplate.Id,
							FilePath = preContractAgreement.FilePath
						},
						TemplateModel = template
					});
				}

				var contract = this._templates.GetTemplateByName(origin + "CreditActAgreement");
				var contractTemplate = this._helper.GetOrCreateLoanAgreementTemplate(contract, isAlibaba ? LoanAgreementTemplateType.AlibabaCreditActAgreement : LoanAgreementTemplateType.CreditActAgreement);
				var contractAgreement = new LoanAgreement("Contract", loan, contractTemplate);
				loan.Agreements.Add(contractAgreement);

				path1 = Path.Combine(CurrentValues.Instance.AgreementPdfLoanPath1, contractAgreement.FilePath);
				path2 = Path.Combine(CurrentValues.Instance.AgreementPdfLoanPath2, contractAgreement.FilePath);
				template = new TemplateModel { Template = contract };
				this.m_oServiceClient.Instance.SaveAgreement(loan.Customer.Id, model, loan.RefNumber, "Contract", template, path1, path2);

				//  NL - contract
				if (nlModel != null) {
					nlModel.Agreements.Add(item: new NLAgreementItem() {
						Agreement = new NL_LoanAgreements() {
							LoanAgreementTemplateID = contractTemplate.Id,
							FilePath = contractAgreement.FilePath
						},
						TemplateModel = template
					});
				}

			} else {
				var guarantee = this._templates.GetTemplateByName(origin + "GuarantyAgreement");
				var quaranteeTemplate = this._helper.GetOrCreateLoanAgreementTemplate(guarantee, isAlibaba ? LoanAgreementTemplateType.AlibabaGuarantyAgreement : LoanAgreementTemplateType.GuarantyAgreement);
				var guaranteeAgreement = new LoanAgreement("guarantee", loan, quaranteeTemplate);
				loan.Agreements.Add(guaranteeAgreement);

				path1 = Path.Combine(CurrentValues.Instance.AgreementPdfLoanPath1, guaranteeAgreement.FilePath);
				path2 = Path.Combine(CurrentValues.Instance.AgreementPdfLoanPath2, guaranteeAgreement.FilePath);
				template = new TemplateModel { Template = guarantee };
				this.m_oServiceClient.Instance.SaveAgreement(loan.Customer.Id, model, loan.RefNumber, "guarantee", template, path1, path2);

				// NL - guarantee 
				if (nlModel != null) {
					nlModel.Agreements.Add(item: new NLAgreementItem() {
						Agreement = new NL_LoanAgreements() {
							LoanAgreementTemplateID = quaranteeTemplate.Id,
							FilePath = guaranteeAgreement.FilePath
						},
						TemplateModel = template
					});
				}

				var agreement = this._templates.GetTemplateByName(origin + "PrivateCompanyLoanAgreement");
				var agreementTemplate = this._helper.GetOrCreateLoanAgreementTemplate(agreement, isAlibaba ? LoanAgreementTemplateType.AlibabaPrivateCompanyLoanAgreement : LoanAgreementTemplateType.PrivateCompanyLoanAgreement);
				var agreementAgreement = new LoanAgreement("agreement", loan, agreementTemplate);
				loan.Agreements.Add(agreementAgreement);

				path1 = Path.Combine(CurrentValues.Instance.AgreementPdfLoanPath1, agreementAgreement.FilePath);
				path2 = Path.Combine(CurrentValues.Instance.AgreementPdfLoanPath2, agreementAgreement.FilePath);
				template = new TemplateModel { Template = agreement };
				this.m_oServiceClient.Instance.SaveAgreement(loan.Customer.Id, model, loan.RefNumber, "agreement", template, path1, path2);

				// NL - agreement
				if (nlModel != null) {
					nlModel.Agreements.Add(item: new NLAgreementItem() {
						Agreement = new NL_LoanAgreements() {
							LoanAgreementTemplateID = agreementTemplate.Id,
							FilePath = agreementAgreement.FilePath
						},
						TemplateModel = template
					});
				}
			}

			loan.AgreementModel = JsonConvert.SerializeObject(model);
		}

		

	}
}