using EZBob.DatabaseLib;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Loans;
using EZBob.DatabaseLib.Model.Loans;
using EzBob.Models.Agreements;
using Newtonsoft.Json;

namespace EzBob.Web.Code.Agreements
{
	using System.IO;
	using Infrastructure;
	using Scorto.Configuration;


	public interface IAgreementsGenerator
	{
		void RenderAgreements(Loan loan, bool isRebuld);
	}

	public class AgreementsGenerator : IAgreementsGenerator
	{
		private readonly AgreementsModelBuilder _builder;
		private readonly IAgreementsTemplatesProvider _templates;
		private readonly DatabaseDataHelper _helper;
		protected RenderAgreementHandlerConfig Config { get; set; }
		private readonly ServiceClient m_oServiceClient;

		public AgreementsGenerator(AgreementsModelBuilder builder, IAgreementsTemplatesProvider templates, DatabaseDataHelper helper)
		{
			_builder = builder;
			_templates = templates;
			_helper = helper;
			Config = EnvironmentConfiguration.Configuration.GetConfiguration<RenderAgreementHandlerConfig>("RenderAgreementsHandler");
			m_oServiceClient = new ServiceClient();

		}

		public void RenderAgreements(Loan loan, bool isRebuld)
		{
			var typeOfBusinessReduced = loan.Customer.PersonalInfo.TypeOfBusiness.Reduce();
			var isConsumer = typeOfBusinessReduced == TypeOfBusinessReduced.Personal || typeOfBusinessReduced == TypeOfBusinessReduced.NonLimited;

			var model = !isRebuld ? _builder.Build(loan.Customer, loan.LoanAmount, loan) : _builder.ReBuild(loan.Customer, loan);

			string path1;
			string path2;
			TemplateModel template;
			if (isConsumer)
			{
				var preContract = _templates.GetTemplateByName("Pre-Contract-Agreement");
				var preContractAgreement = new LoanAgreement("precontract", loan, _helper.GetOrCreateLoanAgreementTemplate(preContract, 2));
				loan.Agreements.Add(preContractAgreement);

				path1 = Path.Combine(Config.PdfLoanAgreement, preContractAgreement.FilePath);
				path2 = Path.Combine(Config.PdfLoanAgreement2, preContractAgreement.FilePath);
				template = new TemplateModel {Template = preContract};
				m_oServiceClient.Instance.SaveAgreement(loan.Customer.Id, model, loan.RefNumber, "precontract", template, path1, path2);

				var contract = _templates.GetTemplateByName("CreditActAgreement");
				var contractAgreement = new LoanAgreement("Contract", loan, _helper.GetOrCreateLoanAgreementTemplate(contract, 3));
				loan.Agreements.Add(contractAgreement);

				path1 = Path.Combine(Config.PdfLoanAgreement, contractAgreement.FilePath);
				path2 = Path.Combine(Config.PdfLoanAgreement2, contractAgreement.FilePath);
				template = new TemplateModel { Template = contract };
				m_oServiceClient.Instance.SaveAgreement(loan.Customer.Id, model, loan.RefNumber, "Contract", template, path1, path2);
			}
			else
			{
				var guarantee = _templates.GetTemplateByName("GuarantyAgreement");
				var guaranteeAgreement = new LoanAgreement("guarantee", loan, _helper.GetOrCreateLoanAgreementTemplate(guarantee, 1));
				loan.Agreements.Add(guaranteeAgreement);

				path1 = Path.Combine(Config.PdfLoanAgreement, guaranteeAgreement.FilePath);
				path2 = Path.Combine(Config.PdfLoanAgreement2, guaranteeAgreement.FilePath);
				template = new TemplateModel { Template = guarantee };
				m_oServiceClient.Instance.SaveAgreement(loan.Customer.Id, model, loan.RefNumber, "guarantee", template, path1, path2);

				var agreement = _templates.GetTemplateByName("PrivateCompanyLoanAgreement");
				var agreementAgreement = new LoanAgreement("agreement", loan, _helper.GetOrCreateLoanAgreementTemplate(agreement, 4));
				loan.Agreements.Add(agreementAgreement);

				path1 = Path.Combine(Config.PdfLoanAgreement, agreementAgreement.FilePath);
				path2 = Path.Combine(Config.PdfLoanAgreement2, agreementAgreement.FilePath);
				template = new TemplateModel { Template = agreement };
				m_oServiceClient.Instance.SaveAgreement(loan.Customer.Id, model, loan.RefNumber, "agreement", template, path1, path2);
			}

			loan.AgreementModel = JsonConvert.SerializeObject(model);
		}
	}
}