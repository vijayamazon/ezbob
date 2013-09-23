using System;
using System.Globalization;
using System.IO;
using ApplicationMng.Repository;
using FluentNHibernate.Mapping;
using NHibernate;

namespace EZBob.DatabaseLib.Model.Loans
{
	public class LoanAgreement
	{
		public LoanAgreement()
		{
		}

		//public LoanAgreement(string name, string template, Database.Loans.Loan loan, int templateId)
		public LoanAgreement(string name, Database.Loans.Loan loan, LoanAgreementTemplate template)
		{
			Name = name;
			Template = "OBSOLETE COLUMN!! This template can be found in LoanAgreementTemplate by TemplateId."; // Obsolete line, to be removed.
			Loan = loan;
			FilePath = LongFilenameWithDir();
			TemplateRef = template;
		}

		public virtual int Id { get; set; }
		public virtual string Name { get; set; }

		[Obsolete]
		public virtual string Template { get; set; } // Obsolete line, to be removed.
		
		public virtual Database.Loans.Loan Loan { get; set; }
		public virtual string FilePath { get; set; }
		public virtual LoanAgreementTemplate TemplateRef { get; set; }


		public virtual string ShortFilename()
		{
			return String.Format("{0}_{1}_{2}_{3}.pdf",
								 Name,
								 Loan.Customer.PersonalInfo.FirstName,
								 Loan.Customer.PersonalInfo.Surname,
								 Loan.Date.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture));
		}

		public virtual string LongFilename()
		{
			return String.Format("{0}_{1}_{2}_{3:000}_{4}.pdf",
								 Name,
								 Loan.Customer.PersonalInfo.FirstName,
								 Loan.Customer.PersonalInfo.Surname,
								 Loan.Customer.Id,
								 Loan.Date.ToString("dd-MM-yyyy_hh-mm-ss", CultureInfo.InvariantCulture));
		}

		public virtual string LongFilenameWithDir()
		{
			var filename = LongFilename();
			var dirYear = (Loan.Date.Year).ToString(CultureInfo.InvariantCulture);
			var dirMonth = (Loan.Date.Month).ToString(CultureInfo.InvariantCulture);
			var dirDay = (Loan.Date.Day).ToString(CultureInfo.InvariantCulture);
			return Path.Combine(dirYear, dirMonth, dirDay, Loan.RefNumber, filename);
		}
	}

	public interface ILoanAgreementRepository : IRepository<LoanAgreement>
	{

	}

	public class LoanAgreementRepository : NHibernateRepositoryBase<LoanAgreement>, ILoanAgreementRepository
	{
		public LoanAgreementRepository(ISession session)
			: base(session)
		{
		}
	}

	public class LoanAgreementMap : ClassMap<LoanAgreement>
	{
		public LoanAgreementMap()
		{
			Table("LoanAgreement");
			Id(x => x.Id).GeneratedBy.HiLo("100");
			Map(x => x.Name).Length(200);
			Map(x => x.FilePath).Length(400);
			Map(x => x.Template).CustomType("StringClob"); // Obsolete line, to be removed.
			References(x => x.Loan, "LoanId");
			References(x => x.TemplateRef, "TemplateId").Cascade.SaveUpdate();
		}
	}
}