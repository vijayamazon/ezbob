namespace EZBob.DatabaseLib.Model.Loans
{
	using ApplicationMng.Repository;
	using FluentNHibernate.Mapping;
	using NHibernate;

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
	}

	public class LoanAgreementTemplateMap : ClassMap<LoanAgreementTemplate>
	{
		public LoanAgreementTemplateMap()
		{
			Table("LoanAgreementTemplate");
			Id(x => x.Id);
			Map(x => x.Template).CustomType("StringClob");
			Map(x => x.TemplateType);
		}
	}

	public interface ILoanAgreementTemplateRepository : IRepository<LoanAgreementTemplate>
	{
	}

	public class LoanAgreementTemplateRepository : NHibernateRepositoryBase<LoanAgreementTemplate>, ILoanAgreementTemplateRepository
	{
		public LoanAgreementTemplateRepository(ISession session)
			: base(session)
		{
		}
	}
}