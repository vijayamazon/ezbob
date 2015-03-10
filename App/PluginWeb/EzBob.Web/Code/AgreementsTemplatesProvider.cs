﻿namespace EzBob.Models.Agreements {
	using System;
	using System.IO;
	using EZBob.DatabaseLib.Model.Loans;

	public interface IAgreementsTemplatesProvider {
		string GetTemplateByName(string name);
		string GetTemplate(LoanAgreementTemplateType templateType);
	} // interface IAgreementsTemplatesProvider

	public class AgreementsTemplatesProvider : IAgreementsTemplatesProvider {
		public string GetTemplateByName(string name) {
			return GetTemplate("\\Areas\\Customer\\Views\\Agreement\\", name);
		} // GetTemplateByName

		public string GetTemplate(LoanAgreementTemplateType templateType) {
			return GetTemplateByName(templateType.ToString());
		} // GetTemplate

		public string GetTemplate(string path, string name) {
			return File.ReadAllText(string.Format("{0}{1}{2}.cshtml", AppDomain.CurrentDomain.BaseDirectory, path, name));
		} // GetTemplate
	} // class AgreementsTemplatesProvider
} // namespace