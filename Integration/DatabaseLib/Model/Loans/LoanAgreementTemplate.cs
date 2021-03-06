﻿namespace EZBob.DatabaseLib.Model.Loans
{
	using System.Linq;
	using ApplicationMng.Repository;
	using FluentNHibernate.Mapping;
	using NHibernate;

	public enum LoanAgreementTemplateType {
		GuarantyAgreement = 1,
		PreContract = 2,
		RegulatedLoanAgreement = 3,
		PrivateCompanyLoanAgreement = 4,

		EzbobAlibabaGuarantyAgreement = 5,
		EzbobAlibabaPreContract = 6,
		EzbobAlibabaRegulatedLoanAgreement = 7,
		EzbobAlibabaPrivateCompanyLoanAgreement = 8,
		CreditFacility = 9,
	}

	public class LoanAgreementTemplate
	{
		public LoanAgreementTemplate()
		{
		}

		public LoanAgreementTemplate(string template)
		{
			Template = template;
		}

		public virtual int Id { get; set; }
		public virtual string Template { get; set; }
		public virtual int TemplateType { get; set; }
		public virtual bool IsUpdate { get; set; }
	}

	public class LoanAgreementTemplateMap : ClassMap<LoanAgreementTemplate>
	{
		public LoanAgreementTemplateMap()
		{
			Table("LoanAgreementTemplate");
			Id(x => x.Id);
			Map(x => x.Template).CustomType("StringClob");
			Map(x => x.TemplateType);
			Map(x => x.IsUpdate);
		}
	}

	public interface ILoanAgreementTemplateRepository : IRepository<LoanAgreementTemplate> {
		string GetUpdateTemplate(int templateType);
	}

	public class LoanAgreementTemplateRepository : NHibernateRepositoryBase<LoanAgreementTemplate>, ILoanAgreementTemplateRepository
	{
		public LoanAgreementTemplateRepository(ISession session)
			: base(session) {}

		public string GetUpdateTemplate(int templateType)
		{
			var agreementTemplate =  GetAll().FirstOrDefault(x => x.IsUpdate && x.TemplateType == templateType);
			if (agreementTemplate != null) {
				return agreementTemplate.Template;
			}
			return null;
		}
		
	}
}