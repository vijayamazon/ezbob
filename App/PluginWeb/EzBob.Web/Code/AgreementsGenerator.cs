namespace EzBob.Models.Agreements 
{
	using System;
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
			this._builder = builder;
			this._templates = templates;
			this._helper = helper;
			this.m_oServiceClient = new ServiceClient();
		}

		/// <exception cref="OverflowException">The number of elements in is larger than <see cref="F:System.Int32.MaxValue" />.</exception>
		/// <exception cref="Exception">Alibaba can't be a Personal customer</exception>
		/// <exception cref="NullReferenceException"><paramref /> is null. </exception>
		public void RenderAgreements(Loan loan, bool isRebuld, NL_Model nlModel = null) {

			var typeOfBusinessReduced = loan.Customer.PersonalInfo.TypeOfBusiness.AgreementReduce();

			var model = !isRebuld ? this._builder.Build(loan.Customer, loan.LoanAmount, loan) : this._builder.ReBuild(loan.Customer, loan);

			// NL - AgreementModel in json  
			if (nlModel != null)
				nlModel.Loan.LastHistory().AgreementModel = JsonConvert.SerializeObject(this._builder.NL_BuildAgreementModel(loan.Customer, nlModel));

			var isAlibaba = loan.Customer.IsAlibaba;
			var isEverline = loan.Customer.CustomerOrigin.IsEverline();
			var IsEverlineRefinanceLoan = false;

			if (isEverline) {
				EverlineLoginLoanChecker checker = new EverlineLoginLoanChecker();
				var status = checker.GetLoginStatus(loan.Customer.Name);
				if (status.status == EverlineLoanStatus.ExistsWithCurrentLiveLoan) {
					IsEverlineRefinanceLoan = true;
				}
			}

			string path1;
			string path2;
			string nlpath1;
			string nlpath2;
			TemplateModel template;

			if (typeOfBusinessReduced == TypeOfBusinessAgreementReduced.Personal) {

				//Alibaba can't be a Personal customer
				if (isAlibaba)
					throw new Exception("Alibaba can't be a Personal customer");

				// string - content of file D:\ezbob\App\PluginWeb\EzBob.Web\Areas\Customer\Views\Agreement\Pre-Contract-Agreement.cshtml
				var preContract = this._templates.GetTemplateByName(this._templates.GetTemplatePath(LoanAgreementTemplateType.PreContract, isEverline, false, IsEverlineRefinanceLoan));
				var preContractTemplate = this._helper.GetOrCreateLoanAgreementTemplate(preContract, LoanAgreementTemplateType.PreContract);
				// LoanAgreement (table) instance by preContract string == table LoanAgreementTemplate.Template && LoanAgreementTemplate.TemplateType== Enum.LoanAgreementTemplateType ID
				var preContractAgreement = new LoanAgreement("precontract", loan, preContractTemplate);
				loan.Agreements.Add(preContractAgreement);

				path1 = Path.Combine(CurrentValues.Instance.AgreementPdfLoanPath1, preContractAgreement.FilePath);
				path2 = Path.Combine(CurrentValues.Instance.AgreementPdfLoanPath2, preContractAgreement.FilePath);
				template = new TemplateModel { Template = preContract };
				this.m_oServiceClient.Instance.SaveAgreement(loan.Customer.Id, model, loan.RefNumber, "precontract", template, path1, path2);

				nlpath1 = Path.Combine(CurrentValues.Instance.NL_AgreementPdfLoanPath1, preContractAgreement.FilePath);
				nlpath2 = Path.Combine(CurrentValues.Instance.NL_AgreementPdfLoanPath2, preContractAgreement.FilePath);
				this.m_oServiceClient.Instance.SaveAgreement(loan.Customer.Id, model, loan.RefNumber, "precontract", template, nlpath1, nlpath2);

				// NL  - preContract 
				if (nlModel != null) {
					nlModel.Loan.LastHistory().Agreements.Add(new NL_LoanAgreements() {LoanAgreementTemplateID = preContractTemplate.Id, FilePath = preContractAgreement.FilePath});
				}
				
				var contract = this._templates.GetTemplateByName(this._templates.GetTemplatePath(LoanAgreementTemplateType.RegulatedLoanAgreement, isEverline, false, IsEverlineRefinanceLoan));
				var contractTemplate = this._helper.GetOrCreateLoanAgreementTemplate(contract, LoanAgreementTemplateType.RegulatedLoanAgreement);
				var contractAgreement = new LoanAgreement("Contract", loan, contractTemplate);
				loan.Agreements.Add(contractAgreement);

				path1 = Path.Combine(CurrentValues.Instance.AgreementPdfLoanPath1, contractAgreement.FilePath);
				path2 = Path.Combine(CurrentValues.Instance.AgreementPdfLoanPath2, contractAgreement.FilePath);
				template = new TemplateModel { Template = contract };
				this.m_oServiceClient.Instance.SaveAgreement(loan.Customer.Id, model, loan.RefNumber, "Contract", template, path1, path2);

				nlpath1 = Path.Combine(CurrentValues.Instance.NL_AgreementPdfLoanPath1, contractAgreement.FilePath);
				nlpath2 = Path.Combine(CurrentValues.Instance.NL_AgreementPdfLoanPath2, contractAgreement.FilePath);
				this.m_oServiceClient.Instance.SaveAgreement(loan.Customer.Id, model, loan.RefNumber, "Contract", template, nlpath1, nlpath2);

				//  NL - contract
				if (nlModel != null) {
					nlModel.Loan.LastHistory().Agreements.Add(new NL_LoanAgreements() { LoanAgreementTemplateID = contractTemplate.Id, FilePath = contractAgreement.FilePath });
				}

			} else {
				var guarantee = this._templates.GetTemplateByName(this._templates.GetTemplatePath(LoanAgreementTemplateType.GuarantyAgreement, isEverline, isAlibaba, IsEverlineRefinanceLoan));
				var quaranteeTemplate = this._helper.GetOrCreateLoanAgreementTemplate(guarantee, isAlibaba ? LoanAgreementTemplateType.EzbobAlibabaGuarantyAgreement : LoanAgreementTemplateType.GuarantyAgreement);
				var guaranteeAgreement = new LoanAgreement("guarantee", loan, quaranteeTemplate);
				loan.Agreements.Add(guaranteeAgreement);

				path1 = Path.Combine(CurrentValues.Instance.AgreementPdfLoanPath1, guaranteeAgreement.FilePath);
				path2 = Path.Combine(CurrentValues.Instance.AgreementPdfLoanPath2, guaranteeAgreement.FilePath);
				template = new TemplateModel { Template = guarantee };
				this.m_oServiceClient.Instance.SaveAgreement(loan.Customer.Id, model, loan.RefNumber, "guarantee", template, path1, path2);

				nlpath1 = Path.Combine(CurrentValues.Instance.NL_AgreementPdfLoanPath1, guaranteeAgreement.FilePath);
				nlpath2 = Path.Combine(CurrentValues.Instance.NL_AgreementPdfLoanPath2, guaranteeAgreement.FilePath);
				this.m_oServiceClient.Instance.SaveAgreement(loan.Customer.Id, model, loan.RefNumber, "guarantee", template, nlpath1, nlpath2);

				// NL - guarantee 
				if (nlModel != null) {
					nlModel.Loan.LastHistory().Agreements.Add(new NL_LoanAgreements() { LoanAgreementTemplateID = quaranteeTemplate.Id, FilePath = guaranteeAgreement.FilePath });
				}
				
				var agreement = this._templates.GetTemplateByName(this._templates.GetTemplatePath(LoanAgreementTemplateType.PrivateCompanyLoanAgreement, isEverline, isAlibaba, IsEverlineRefinanceLoan));
				var agreementTemplate = this._helper.GetOrCreateLoanAgreementTemplate(agreement, isAlibaba ? LoanAgreementTemplateType.EzbobAlibabaPrivateCompanyLoanAgreement : LoanAgreementTemplateType.PrivateCompanyLoanAgreement);
				var agreementAgreement = new LoanAgreement("agreement", loan, agreementTemplate);
				loan.Agreements.Add(agreementAgreement);

				path1 = Path.Combine(CurrentValues.Instance.AgreementPdfLoanPath1, agreementAgreement.FilePath);
				path2 = Path.Combine(CurrentValues.Instance.AgreementPdfLoanPath2, agreementAgreement.FilePath);
				template = new TemplateModel { Template = agreement };
				this.m_oServiceClient.Instance.SaveAgreement(loan.Customer.Id, model, loan.RefNumber, "agreement", template, path1, path2);

				nlpath1 = Path.Combine(CurrentValues.Instance.NL_AgreementPdfLoanPath1, agreementAgreement.FilePath);
				nlpath2 = Path.Combine(CurrentValues.Instance.NL_AgreementPdfLoanPath2, agreementAgreement.FilePath);
				this.m_oServiceClient.Instance.SaveAgreement(loan.Customer.Id, model, loan.RefNumber, "agreement", template, nlpath1, nlpath2);

				// NL - agreement
				if (nlModel != null) {
					nlModel.Loan.LastHistory().Agreements.Add(new NL_LoanAgreements() { LoanAgreementTemplateID = agreementTemplate.Id, FilePath = agreementAgreement.FilePath });
				}
			}

			loan.AgreementModel = JsonConvert.SerializeObject(model);
		}
	}
}