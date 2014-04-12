﻿namespace EzBob.Web.Code
{
	using ConfigManager;
	using Scorto.Configuration;
	using System;
	using System.Globalization;
	using System.IO;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Repository;
	using EzBob.Models.Agreements;
	using StructureMap;


	public interface IConcentAgreementHelper
	{
		byte[] Generate(string fullName, DateTime date);
		byte[] GenerateWidhDataBase(Customer customer);
		string RenderAgreement(Customer customer);
		string GetTemplate();
		void Save(Customer customer, DateTime date);
		void SaveToBase(int id, string template, string fileName);
		string GetFileName(Customer customer);
		string GetFileName(int id, string firstName, string surname, DateTime date);
		string GetFilenameWithDir(Customer customer);
	}

	public class ConcentAgreementHelper : IConcentAgreementHelper
	{
		private readonly AgreementsTemplatesProvider _templatesProvider = new AgreementsTemplatesProvider();
		private readonly AgreementRenderer _agreementRenderer = new AgreementRenderer();
		private readonly IExperianConsentAgreementRepository _repository;
		private const string TemlatePath = "\\Areas\\Customer\\Views\\Consent\\";
		private readonly ServiceClient m_oServiceClient;

		public ConcentAgreementHelper()
		{
			_repository = ObjectFactory.GetInstance<ExperianConsentAgreementRepository>();
			m_oServiceClient = new ServiceClient();

		}

		public byte[] Generate(string fullName, DateTime date)
		{
			var template = GetTemplate();
			var model = new AgreementModel { FullName = fullName, CurrentDate = date };
			return _agreementRenderer.RenderAgreementToPdf(template, model);
		}

		public byte[] GenerateWidhDataBase(Customer customer)
		{
			var agreement = _repository.GetByCustomerId(customer.Id);
			if (agreement == null)
			{
				Save(customer, (DateTime)customer.GreetingMailSentDate);
				return Generate(customer.PersonalInfo.Fullname, (DateTime)customer.GreetingMailSentDate);
			}
			string template = string.Empty;
			template = !string.IsNullOrEmpty(agreement.Template) ? agreement.Template : GetTemplate();
			var model = new AgreementModel { FullName = customer.PersonalInfo.Fullname, CurrentDate = customer.GreetingMailSentDate ?? DateTime.UtcNow };
			return _agreementRenderer.RenderAgreementToPdf(template, model);
		}

		public string RenderAgreement(Customer customer)
		{
			var agreement = _repository.GetByCustomerId(customer.Id);
			var template = agreement == null ? GetTemplate() : agreement.Template;
			var model = new AgreementModel { FullName = customer.PersonalInfo.Fullname, CurrentDate = customer.GreetingMailSentDate ?? DateTime.UtcNow };
			return _agreementRenderer.RenderAgreement(template, model);
		}


		public string GetTemplate()
		{
			var templateCommercial = _templatesProvider.GetTemplate(TemlatePath, "Commercial");
			var templateAgreed = _templatesProvider.GetTemplate(TemlatePath, "Agreed");
			var template = templateCommercial + templateAgreed;
			return template;
		}

		public void Save(Customer customer, DateTime date)
		{
			var personalInfo = customer.PersonalInfo;
			var fileName = GetFilenameWithDir(customer);
			var model = new AgreementModel { FullName = personalInfo.Fullname, CurrentDate = DateTime.Now };
			var template = GetTemplate();
			SaveToBase(customer.Id, template, fileName);

			var path1 = Path.Combine(CurrentValues.Instance.AgreementPdfConsentPath1, fileName);
			var path2 = Path.Combine(CurrentValues.Instance.AgreementPdfConsentPath2, fileName);
			m_oServiceClient.Instance.SaveAgreement(customer.Id, model, null, "concent agreement",new TemplateModel{Template = template}, path1, path2);
		}

		public void SaveToBase(int id, string template, string fileName)
		{
			var experianConsentAgreement = new ExperianConsentAgreement
				{
					CustomerId = id,
					Template = template,
					FilePath = fileName
				};

			_repository.SaveOrUpdate(experianConsentAgreement);
		}

		public string GetFileName(Customer customer)
		{
			var personalInfo = customer.PersonalInfo;
			var currentDate = customer.GreetingMailSentDate ?? DateTime.UtcNow;
			return GetFileName(customer.Id, personalInfo.FirstName, personalInfo.Surname, currentDate);
		}

		public string GetFileName(int id, string firstName, string surname, DateTime date)
		{
			return string.Format("ExperianConsent_{0}_{1}_{2}_{3:000}.pdf", firstName, surname, id, date.ToString("dd-MM-yyyy_hh-mm-ss", CultureInfo.InvariantCulture));
		}

		public string GetFilenameWithDir(Customer customer)
		{
			var filename = GetFileName(customer);
			var currentDate = customer.GreetingMailSentDate ?? DateTime.UtcNow;
			var dirYear = (currentDate.Year).ToString(CultureInfo.InvariantCulture);
			var dirMonth = (currentDate.Month).ToString(CultureInfo.InvariantCulture);
			var dirDay = (currentDate.Day).ToString(CultureInfo.InvariantCulture);
			return Path.Combine(dirYear, dirMonth, dirDay, filename);
		}
	}
}