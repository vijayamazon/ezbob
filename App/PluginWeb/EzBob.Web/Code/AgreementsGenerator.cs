namespace EzBob.Web.Code.Agreements
{
    using System;
    using ConfigManager;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Loans;
	using EzBob.Models.Agreements;
	using Ezbob.Backend.Models;
	using Newtonsoft.Json;
	using System.IO;
    using Ezbob.Backend.ModelsWithDB.NewLoan;
    using ServiceClientProxy;

	public interface IAgreementsGenerator
	{
		void RenderAgreements(Loan loan, bool isRebuld);

		void NL_RenderAgreements(NL_Model loan, bool isRebuld);
	}

	public class AgreementsGenerator : IAgreementsGenerator
	{
		private readonly AgreementsModelBuilder _builder;
		private readonly IAgreementsTemplatesProvider _templates;
		private readonly DatabaseDataHelper _helper;
		private readonly ServiceClient m_oServiceClient;

		public AgreementsGenerator(AgreementsModelBuilder builder, IAgreementsTemplatesProvider templates, DatabaseDataHelper helper)
		{
			_builder = builder;
			_templates = templates;
			_helper = helper;
			m_oServiceClient = new ServiceClient();
		}

		public void RenderAgreements(Loan loan, bool isRebuld)
		{
			var typeOfBusinessReduced = loan.Customer.PersonalInfo.TypeOfBusiness.AgreementReduce();
			
			var model = !isRebuld ? _builder.Build(loan.Customer, loan.LoanAmount, loan) : _builder.ReBuild(loan.Customer, loan);

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
			TemplateModel template;
			if (typeOfBusinessReduced == TypeOfBusinessAgreementReduced.Personal) {
                
                //Alibaba can't be a Personal customer
                if (isAlibaba)
                    throw new Exception("Alibaba can't be a Personal customer");

                var preContract = _templates.GetTemplateByName(_templates.GetTemplatePath(LoanAgreementTemplateType.PreContract, isEverline, false, IsEverlineRefinanceLoan));
				var preContractAgreement = new LoanAgreement("precontract", loan, 
					_helper.GetOrCreateLoanAgreementTemplate(preContract,  LoanAgreementTemplateType.PreContract ));
				loan.Agreements.Add(preContractAgreement);

				path1 = Path.Combine(CurrentValues.Instance.AgreementPdfLoanPath1, preContractAgreement.FilePath);
				path2 = Path.Combine(CurrentValues.Instance.AgreementPdfLoanPath2, preContractAgreement.FilePath);
				template = new TemplateModel {Template = preContract};
				m_oServiceClient.Instance.SaveAgreement(loan.Customer.Id, model, loan.RefNumber, "precontract", template, path1, path2);

                var contract = _templates.GetTemplateByName(_templates.GetTemplatePath(LoanAgreementTemplateType.RegulatedLoanAgreement, isEverline, false, IsEverlineRefinanceLoan));
				var contractAgreement = new LoanAgreement("Contract", loan, 
					_helper.GetOrCreateLoanAgreementTemplate(contract, LoanAgreementTemplateType.RegulatedLoanAgreement));
				loan.Agreements.Add(contractAgreement);

				path1 = Path.Combine(CurrentValues.Instance.AgreementPdfLoanPath1, contractAgreement.FilePath);
				path2 = Path.Combine(CurrentValues.Instance.AgreementPdfLoanPath2, contractAgreement.FilePath);
				template = new TemplateModel { Template = contract };
				m_oServiceClient.Instance.SaveAgreement(loan.Customer.Id, model, loan.RefNumber, "Contract", template, path1, path2);
			}
			else
			{
                var guarantee = _templates.GetTemplateByName(_templates.GetTemplatePath(LoanAgreementTemplateType.GuarantyAgreement, isEverline, isAlibaba, IsEverlineRefinanceLoan));
				var guaranteeAgreement = new LoanAgreement("guarantee", loan, 
					_helper.GetOrCreateLoanAgreementTemplate(guarantee, isAlibaba ? LoanAgreementTemplateType.EzbobAlibabaGuarantyAgreement : LoanAgreementTemplateType.GuarantyAgreement ));
				loan.Agreements.Add(guaranteeAgreement);

				path1 = Path.Combine(CurrentValues.Instance.AgreementPdfLoanPath1, guaranteeAgreement.FilePath);
				path2 = Path.Combine(CurrentValues.Instance.AgreementPdfLoanPath2, guaranteeAgreement.FilePath);
				template = new TemplateModel { Template = guarantee };
				m_oServiceClient.Instance.SaveAgreement(loan.Customer.Id, model, loan.RefNumber, "guarantee", template, path1, path2);

                var agreement = _templates.GetTemplateByName(_templates.GetTemplatePath(LoanAgreementTemplateType.PrivateCompanyLoanAgreement, isEverline, isAlibaba, IsEverlineRefinanceLoan));
				var agreementAgreement = new LoanAgreement("agreement", loan,
					_helper.GetOrCreateLoanAgreementTemplate(agreement, isAlibaba ? LoanAgreementTemplateType.EzbobAlibabaPrivateCompanyLoanAgreement : LoanAgreementTemplateType.PrivateCompanyLoanAgreement));
				loan.Agreements.Add(agreementAgreement);

				path1 = Path.Combine(CurrentValues.Instance.AgreementPdfLoanPath1, agreementAgreement.FilePath);
				path2 = Path.Combine(CurrentValues.Instance.AgreementPdfLoanPath2, agreementAgreement.FilePath);
				template = new TemplateModel { Template = agreement };
				m_oServiceClient.Instance.SaveAgreement(loan.Customer.Id, model, loan.RefNumber, "agreement", template, path1, path2);
			}

			loan.AgreementModel = JsonConvert.SerializeObject(model);
		}

		public void NL_RenderAgreements(NL_Model loan, bool isRebuld) {
			//TODO EZ-3483 
			// 1. get model _builder.NL_BuildAgreementModel(...);
			// 2. generate agreement using template and the model
			// 3. create paths for file system
			// 4. save to FS using m_oServiceClient.Instance.SaveAgreement(loan.Customer.Id, model, loan.RefNumber, "agreement", template, path1, path2);

		}
		
	}
}