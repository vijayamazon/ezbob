using System;
using System.Globalization;
using System.IO;
using ApplicationMng.Repository;
using FluentNHibernate.Mapping;
using NHibernate;

namespace EZBob.DatabaseLib.Model.Loans
{
	using System.Linq;

	public class LoanAgreement
	{
		public LoanAgreement()
		{
		}

		//public LoanAgreement(string name, string template, Database.Loans.Loan loan, int templateId)
		public LoanAgreement(string name, Database.Loans.Loan loan, int templateID)
		{
			Name = name;
			Loan = loan;
			FilePath = LongFilenameWithDir();
			TemplateID = templateID;
		}

		public virtual int Id { get; set; }
		public virtual string Name { get; set; }

		public virtual Database.Loans.Loan Loan { get; set; }
		public virtual string FilePath { get; set; }
		public virtual int TemplateID { get; set; }

		public virtual string ShortFilename()
		{
			return String.Format("{0}_{1}_{2}_{3}.pdf",
								 Name,
								 Loan.Customer.PersonalInfo.FirstName,
								 Loan.Customer.PersonalInfo.Surname.Replace(":", string.Empty).Replace(@"\", string.Empty).Replace(@"/", string.Empty),
								 Loan.Date.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture));
		}

		public virtual string LongFilename()
		{
			return String.Format("{0}_{1}_{2}_{3:000}_{4}.pdf",
								 Name,
								 Loan.Customer.PersonalInfo.FirstName,
								 Loan.Customer.PersonalInfo.Surname.Replace(":", string.Empty).Replace(@"\", string.Empty).Replace(@"/", string.Empty),
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
		IQueryable<LoanAgreement> GetByLoanId(int loanId);
	}

	public class LoanAgreementRepository : NHibernateRepositoryBase<LoanAgreement>, ILoanAgreementRepository
	{
		public LoanAgreementRepository(ISession session)
			: base(session)
		{

		}

		public IQueryable<LoanAgreement> GetByLoanId(int loanId)
		{
			return GetAll().Where(l => l.Loan.Id == loanId);
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
            Map(x => x.TemplateID, "TemplateId");
            References(x => x.Loan, "LoanId");		    
		}
	}
}
